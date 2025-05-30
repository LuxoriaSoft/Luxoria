
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models;

[ExcludeFromCodeCoverage]
public class FilterData
{
    /// <summary>
    /// Stores the rating of the asset
    /// </summary>
    private double _rating = -1;

    /// <summary>
    /// Stores the scores of algorithms applied to the asset
    /// </summary>
    private ConcurrentDictionary<string, double> _scores { get; } = new()
    {
        ["Flag_Keep"] = 0,
        ["Flag_Ignore"] = 0
    };

    /// <summary>
    /// Public property to check if the asset is rated
    /// </summary>
    public bool IsRated => _rating >= 0;

    /// <summary>
    /// Public property to get or set the rating of the asset
    /// </summary>
    public double Rating
    {
        get
        {
            return _rating;
        }
        set
        {
            if (value < -1 || value > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Rating must be between -1 and 5.");
            }
            _rating = value;
        }
    }

    /// <summary>
    /// Set the rating of the asset
    /// </summary>
    /// <param name="algoName">Algorithm name</param>
    /// <param name="score">Score provided by the algorithm</param>
    public void SetScore(string algoName, double score) => _scores[algoName] = score;

    /// <summary>
    /// FlagType enum to represent the type of flag applied to the asset
    /// </summary>
    public enum FlagType
    {
        Keep,
        Ignore
    }

    /// <summary>
    /// Set the flag for the asset
    /// </summary>
    /// <param name="flag">Using FlagType</param>
    public void SetFlag(FlagType? flag)
    {
        _scores["Flag_Keep"] = 0;
        _scores["Flag_Ignore"] = 0;
        _scores["Flag_Keep"] = flag == FlagType.Keep ? 1 : 0;
        _scores["Flag_Ignore"] = flag == FlagType.Ignore ? 1 : 0;
    }

    /// <summary>
    /// Get the flag applied to the asset
    /// </summary>
    /// <returns>Returns null if no flag has been assigned</returns>
    public FlagType? GetFlag()
    {
        if (_scores["Flag_Keep"] > 0 && _scores["Flag_Ignore"] > 0)
        {
            return null; // Both flags are set, which is ambiguous
        }
        if (_scores["Flag_Keep"] > 0)
        {
            return FlagType.Keep;
        }
        if (_scores["Flag_Ignore"] > 0)
        {
            return FlagType.Ignore;
        }
        return null;
    }

    /// <summary>
    /// Get each algorithm name that has been applied to the asset
    /// </summary>
    /// <returns>A collection which contains each name</returns>
    public ICollection<string> GetFilteredAlgorithms() => [.. _scores.Keys];

    /// <summary>
    /// et the score of each algorithm applied to the asset
    /// </summary>
    /// <returns>Return a dictionnary under the form (algoName, score)</returns>
    public IDictionary<string, double> GetScores() => _scores;
}
