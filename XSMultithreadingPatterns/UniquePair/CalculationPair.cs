using System;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public class PairingData<PartType, GlobalDataType> : IDisposable
    {
        public PartType[] Stack1 { get; set; }
        public PartType[] Stack2 { get; set; }
        public GlobalDataType GlobalData { get; set; }

        public bool CalculateInternally { get; set; }

        public PairingData(PartType[] stack1, PartType[] stack2, GlobalDataType globalData, bool calculateIntern)
        {
            Stack1 = stack1;
            Stack2 = stack2;
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