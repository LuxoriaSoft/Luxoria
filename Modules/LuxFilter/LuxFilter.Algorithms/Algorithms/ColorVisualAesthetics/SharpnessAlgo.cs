using LuxFilter.Algorithms.Interfaces;

namespace LuxFilter.Algorithms.ColorVisualAesthetics
{
    public class SharpnessAlgo : IFilterAlgorithm
    {
        public string Name => "Sharpness";
        public string Description => "Sharpness algorithm";
        public IScore Compute(byte[] pixels, int height, int width)
        {
            throw new NotImplementedException();
        }
    }
}
