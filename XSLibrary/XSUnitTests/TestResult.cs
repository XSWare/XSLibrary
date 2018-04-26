using System;

namespace XSLibrary.UnitTests
{
    public class TestResult
    {
        public bool Successful { get; set; } = false;
        public TimeSpan Duration { get; set; } = new TimeSpan(0);
    }
}
