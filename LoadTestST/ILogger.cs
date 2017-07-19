using System;

namespace LoadTestST
{
    public interface ILogger
    {
        void Verbose(string text);
        void Verbose(string text, params object[] args);

        void Info(string text);
        void Info(string text, params object[] args);

        void Warning(string text);
        void Warning(string text, params object[] args);

        void Error(string text);
        void Error(string text, params object[] args);

        void Milestone(string text);
        void Milestone(string text, params object[] args);

        IDisposable VerboseSection(string text);
        IDisposable VerboseSection(string text, params object[] args);

        IDisposable InfoSection(string text);
        IDisposable InfoSection(string text, params object[] args);

        IDisposable WarningSection(string text);
        IDisposable WarningSection(string text, params object[] args);

        IDisposable ErrorSection(string text);
        IDisposable ErrorSection(string text, params object[] args);

        IDisposable MilestoneSection(string text);
        IDisposable MilestoneSection(string text, params object[] args);
    }
}