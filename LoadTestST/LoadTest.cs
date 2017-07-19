using System;

namespace LoadTestST
{
    public abstract class LoadTest<TResultData>
    {
        protected LoadTest(ILogger logger)
        {
            Logger = logger;
        }
        public void Run(int threadID)
        {
            var threadIndex = threadID;
            Logger.Verbose("test {0} started...", threadIndex);
            try
            {
                var result = ExecuteTest(out _executionTime);
                Logger.Verbose("test {0} result obtained... execution time {1}s.", threadIndex, (double)_executionTime / 1000);
                CheckResult(result);
                ResultData = result;
            }
            catch (Exception e)
            {
                Error = e;
                Logger.Error("test {0} encounters an exception...", threadIndex);
            }
        }

        protected abstract void CheckResult(TResultData resultData);

        protected abstract TResultData ExecuteTest(out long executionTime);

        protected ILogger Logger { get; }

        public bool IsSuccess => Error == null && ResultData != null;

        public Exception Error { get; private set; }

        public TResultData ResultData { get; private set; }


        private long _executionTime;
        public long ExecutionTime => _executionTime;
    }
}
