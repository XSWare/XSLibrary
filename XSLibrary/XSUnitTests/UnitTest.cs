using XSLibrary.Utility;
using System.Diagnostics;
using System;

namespace XSLibrary.UnitTests
{
    public abstract class UnitTest : IDisposable
    {
        public string TestName { get; set; } = "Default";

        protected Stopwatch m_stopWatch;
        public Logger m_log;

        public UnitTest()
        {
            m_stopWatch = new Stopwatch();
            m_log = new NoLog();
        }

        public TestResult Run()
        {
            m_log.Log("Starting unit test \"" + TestName + "\".");

            TestResult result = new TestResult();

            Initializing();

            m_stopWatch.Restart();
            TestRoutine(result);
            m_stopWatch.Stop();
            result.Duration = m_stopWatch.Elapsed;

            PostProcessing();

            m_log.Log("Test duration: " + result.Duration.TotalMilliseconds + "ms");
            m_log.Log("Test " + (result.Successful ? "was successful." : "failed.") + "\n");

            return result;
        }

        protected abstract void Initializing();
        protected abstract void TestRoutine(TestResult result);
        protected abstract void PostProcessing();
        public abstract void Dispose();
    }
}
