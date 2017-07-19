using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoadTestST.Models;

namespace LoadTestST
{
    public abstract class TestLoader<TTestResultData>
    {
        protected TestLoader(ILogger logger)
        {
            Logger = logger;
        }
        public void Run<TTest>(int loadNum = 1000) where TTest : LoadTest<TTestResultData>
        {
            var tasks = new Task[loadNum];
            var testInstances = new LoadTest<TTestResultData>[loadNum];

            Logger.Info("launching {0} tasks...", loadNum);

            for (var i = 0; i < loadNum; i++)
            {
                var test = (TTest)Activator.CreateInstance(typeof(TTest), Logger);
                testInstances[i] = test;
                var i1 = i;
                tasks[i] = Task.Run(() => test.Run(i1));
            }

            Task.WaitAll(tasks);

            Logger.Info("All tasks are complete");

            ProcessResult(testInstances);
        }

        protected virtual void ProcessResult(LoadTest<TTestResultData>[] testInstances)
        {
            var loadNum = testInstances.Length;

            long sucessExecutionTimeTotal = 0, exceptionExecutionTimeTotal = 0;
            var exceptionDict = new Dictionary<Type, DataCounter<IDictionary<string, DataCounter<Tuple<Exception, ISet<int>>>>>>();
            for (var i = 0; i < loadNum; i++)
            {
                var testInstance = testInstances[i];
                if (!testInstance.IsSuccess)
                {
                    exceptionExecutionTimeTotal += testInstance.ExecutionTime;

                    StoreException(exceptionDict, testInstance.Error, i);
                }
                else
                {
                    sucessExecutionTimeTotal += testInstance.ExecutionTime;
                }
            }

            var resultData = GetInstanceResultData(testInstances);

            PrintResult(exceptionDict, resultData, loadNum, sucessExecutionTimeTotal, exceptionExecutionTimeTotal);
        }

        private static void StoreException(
            IDictionary<Type, DataCounter<IDictionary<string, DataCounter<Tuple<Exception, ISet<int>>>>>> exceptionDict,
            Exception exception, int testInstanceIndex)
        {
            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    StoreException(exceptionDict, innerException, testInstanceIndex);
                }
                return;
            }

            var exceptionType = exception.GetType();

            if (!exceptionDict.ContainsKey(exceptionType))
                exceptionDict[exceptionType] =
                    new DataCounter<IDictionary<string, DataCounter<Tuple<Exception, ISet<int>>>>>(
                        new Dictionary<string, DataCounter<Tuple<Exception, ISet<int>>>>());

            var exceptionGroupInfo = exceptionDict[exceptionType];
            exceptionGroupInfo.Count++;

            var errorMessage = exception.Message;
            if (!exceptionGroupInfo.Data.ContainsKey(errorMessage))
                exceptionGroupInfo.Data[errorMessage] =
                    new DataCounter<Tuple<Exception, ISet<int>>>(
                        new Tuple<Exception, ISet<int>>(exception, new HashSet<int>()));

            var exceptionInfo = exceptionGroupInfo.Data[errorMessage];
            exceptionInfo.Count++;
            exceptionInfo.Data.Item2.Add(testInstanceIndex);
        }

        protected virtual void PrintResult(
            IDictionary<Type, DataCounter<IDictionary<string, DataCounter<Tuple<Exception, ISet<int>>>>>> exceptionDict,
            string resultData, int loadNum, long sucessExecutionTimeTotal, long exceptionExecutionTimeTotal)
        {
            var exceptionTotal = exceptionDict.Select(pair => pair.Value.Count).Sum();
            var successTotal = loadNum - exceptionTotal;

            var executionTimeTotal = sucessExecutionTimeTotal + exceptionExecutionTimeTotal;

            if (resultData != null)
            {
                using (Logger.InfoSection("Result Data:"))
                {
                    Logger.Info(resultData);
                }
            }

            if (successTotal == loadNum)
            {
                Logger.Milestone("test instances all passed");
            }
            else
            {
                Logger.Milestone($"{successTotal} / {loadNum} tests passed, average execution time {sucessExecutionTimeTotal / 1000d / successTotal}s.");

                using (Logger.WarningSection($"{exceptionTotal} / {loadNum} tests failed, total execution time {exceptionExecutionTimeTotal / 1000d / exceptionTotal}s."))
                {
                    foreach (var exceptionGroupInfo in exceptionDict)
                    {
                        using (Logger.WarningSection($"Exception Type: {exceptionGroupInfo.Key.Name}. Occurances: {exceptionGroupInfo.Value.Count}"))
                        {
                            foreach (var exceptionInfo in exceptionGroupInfo.Value.Data)
                            {
                                using (
                                    Logger.WarningSection(
                                        $"Error Message: {exceptionInfo.Key}. Occurances: {exceptionInfo.Value.Count}"))
                                {
                                    Logger.Info($"Occurred in test: [{string.Join(", ", exceptionInfo.Value.Data.Item2)}]");
                                    using (Logger.InfoSection("Exception StackTrace:"))
                                    {
                                        Logger.Info(exceptionInfo.Value.Data.Item1.StackTrace);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Logger.Milestone("Average Total Execution Time: {0}s", (double)executionTimeTotal / 1000 / loadNum);
        }

        protected abstract string GetInstanceResultData(LoadTest<TTestResultData>[] testInstances);

        protected ILogger Logger { get; }
    }
}