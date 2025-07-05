using System;
using System.Collections.Generic;
using System.Linq;
using LuxEditor.Models;

namespace LuxEditor.Services
{
    public sealed class LuxFilterManager
    {
        public static LuxFilterManager Instance { get; } = new();
        private LuxFilterManager() { }

        private readonly HashSet<string> _algorithms = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, (double Min, double Max)> _ranges = new(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<string> AvailableAlgorithms => _algorithms.OrderBy(n => n, StringComparer.OrdinalIgnoreCase);

        public event Action? AlgorithmsChanged;
        private void RaiseChanged() => AlgorithmsChanged?.Invoke();

        private static bool IsRealAlgorithm(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            string n = name.Trim();
            return !n.Contains("flag", StringComparison.OrdinalIgnoreCase) &&
                   !n.Equals("keep", StringComparison.OrdinalIgnoreCase) &&
                   !n.Equals("ignore", StringComparison.OrdinalIgnoreCase);
        }

        public void Clear()
        {
            _algorithms.Clear();
            _ranges.Clear();
            RaiseChanged();
        }

        public bool Register(string name)
        {
            if (!IsRealAlgorithm(name)) return false;
            bool added = _algorithms.Add(name);
            if (added) RaiseChanged();
            return added;
        }

        public int RegisterMany(IEnumerable<string>? names)
        {
            if (names == null) return 0;
            int before = _algorithms.Count;
            foreach (string n in names)
                Register(n);
            return _algorithms.Count - before;
        }

        public int RegisterFromImages(IEnumerable<EditableImage>? images)
        {
            if (images == null) return 0;
            int before = _algorithms.Count;

            foreach (var img in images)
            {
                foreach (var kv in img.FilterData.GetScores())
                {
                    var name = kv.Key;
                    if (!IsRealAlgorithm(name)) continue;

                    double val = kv.Value;
                    if (_ranges.TryGetValue(name, out var r))
                    {
                        if (val < r.Min) r.Min = val;
                        if (val > r.Max) r.Max = val;
                        _ranges[name] = r;
                    }
                    else
                    {
                        _ranges[name] = (val, val);
                    }
                    _algorithms.Add(name);
                }
            }
            if (_algorithms.Count != before)
                RaiseChanged();
            return _algorithms.Count - before;
        }

        public (double Min, double Max) GetRange(string algo)
        {
            return _ranges.TryGetValue(algo, out var r) ? r : (0, 1);
        }

        public double Normalize(string algo, double raw)
        {
            if (!_ranges.TryGetValue(algo, out var r) || r.Max <= r.Min) return 0;
            double norm = (raw - r.Min) / (r.Max - r.Min);
            if (norm < 0) norm = 0;
            if (norm > 1) norm = 1;
            return norm;
        }
    }
}
