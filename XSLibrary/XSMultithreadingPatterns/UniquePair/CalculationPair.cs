using System;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public class CalculationPair<PartType, GlobalDataType> : IDisposable
    {
        public PartType[] Stack1 { get; set; }
        public PartType[] Stack2 { get; set; }
        public GlobalDataType GlobalData { get; set; }

        public bool CalculateInternally { get; set; }

        public CalculationPair(PartType[] part1, PartType[] part2, GlobalDataType globalData, bool calculateIntern)
        {
            Stack1 = part1;
            Stack2 = part2;
            GlobalData = globalData;

            CalculateInternally = calculateIntern;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stack1 = null;
                Stack2 = null;
            }
        }
    }
}
