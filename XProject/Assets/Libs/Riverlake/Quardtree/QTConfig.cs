namespace Riverlake.Scene
{
    public class QTConfig
    {
        // this value determines the smallest cell size
        // the space-partition process would stop dividing if cell size is smaller than this value
        public static float CellSizeThreshold = 50.0f;

        // swap-in distance of cells
        public static float CellSwapInDist = 100.0f;
        // swap-out distance of cells 
        //  (would be larger than swap-in to prevent poping)
        public static float CellSwapOutDist = 150.0f;

        // time interval to update the focus point,
        // so that a new swap would potentially triggered (in seconds)
        public static float SwapTriggerInterval = 0.5f;
        // time interval to update the in/out swapping queues (in seconds)
        public static float SwapProcessInterval = 0.2f;
    }
}