namespace LuxFilter.Algorithms.Interfaces;

public interface IScore
{
    /// <summary>
    /// Score algorithm name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Get the score of the algorithm
    /// </summary>
    /// <returns></returns>
    double GetScore();
}
