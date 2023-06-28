
namespace SAIN.Components.CoverFinder
{
    public class FrameCounter
    {
        private int Count = 0;
        private int Max;
        public FrameCounter(int count) 
        {
            Max = count;
        }
        public bool FrameWait
        {
            get
            {
                if (Count == Max)
                {
                    Count = 0;
                    return true;
                }
                Count++;
                return false;
            }
        }
    }
}
