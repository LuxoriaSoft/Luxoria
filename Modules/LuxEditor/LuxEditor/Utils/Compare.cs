using LuxEditor.Logic;
using LuxEditor.Models;
using Luxoria.Modules.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using static LuxEditor.Models.EditableImage;

namespace LuxEditor.Utils
{
    internal class Compare
    {
        public static bool AreSnapshotsEqual(EditableImageSnapshot a, EditableImageSnapshot b)
        {
            if (a.FileName != b.FileName)
                return false;

            if (!CompareBitmaps(a.EditedBitmap, b.EditedBitmap))
                return false;

            if (!CompareSettings(a.Settings, b.Settings))
                return false;

            if (!CompareFilterData(a.FilterData, b.FilterData))
                return false;

            if (!CompareLayers(a.LayerManager, b.LayerManager))
                return false;

            if (!CompareMetadata(a.Metadata, b.Metadata))
                return false;

            return true;
        }

        private static bool CompareLayers(LayerManager a, LayerManager b)
        {
            var listA = a.Layers;
            var listB = b.Layers;

            if (listA.Count != listB.Count)
                return false;

            for (int i = 0; i < listA.Count; i++)
            {
                var la = listA[i];
                var lb = listB[i];

                if (la.Id != lb.Id ||
                    la.Name != lb.Name ||
                    la.Visible != lb.Visible ||
                    la.Invert != lb.Invert ||
                    Math.Abs(la.Strength - lb.Strength) > 0.01 ||
                    !CompareColors(la.OverlayColor, lb.OverlayColor) ||
                    !CompareFilters(la.Filters, lb.Filters))
                    return false;

                if (!CompareOperations(la.Operations, lb.Operations))
                    return false;
            }

            return true;
        }

        private static bool CompareColors(Color a, Color b)
        {
            return a.A == b.A && a.R == b.R && a.G == b.G && a.B == b.B;
        }

        private static bool CompareFilters(Dictionary<string, object> a, Dictionary<string, object> b)
        {
            if (a.Count != b.Count)
                return false;

            foreach (var kv in a)
            {
                if (!b.TryGetValue(kv.Key, out var valB))
                    return false;

                if (kv.Value is float fa && valB is float fb)
                {
                    if (Math.Abs(fa - fb) > 1e-3) return false;
                }
                else if (kv.Value is byte[] ba && valB is byte[] bb)
                {
                    if (ba.Length != bb.Length) return false;
                    for (int i = 0; i < ba.Length; i++)
                        if (ba[i] != bb[i]) return false;
                }
                else if (!Equals(kv.Value, valB))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CompareOperations(IList<MaskOperation> a, IList<MaskOperation> b)
        {
            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                var oa = a[i];
                var ob = b[i];

                if (oa.Id != ob.Id || oa.Mode != ob.Mode || oa.Tool.ToolType != ob.Tool.ToolType)
                    return false;
            }

            return true;
        }

        private static bool CompareBitmaps(SKBitmap a, SKBitmap b)
        {
            if (a.Width != b.Width || a.Height != b.Height)
                return false;

            if (a.ColorType != b.ColorType || a.AlphaType != b.AlphaType)
                return false;

            var spanA = a.GetPixelSpan();
            var spanB = b.GetPixelSpan();

            return spanA.SequenceEqual(spanB);
        }


        private static bool CompareSettings(Dictionary<string, object> a, Dictionary<string, object> b)
        {
            if (a.Count != b.Count)
                return false;

            foreach (var kv in a)
            {
                if (!b.TryGetValue(kv.Key, out var valB))
                    return false;

                if (kv.Value is List<float> listA && valB is List<float> listB)
                {
                    if (listA.Count != listB.Count || !listA.SequenceEqual(listB))
                        return false;
                }
                else if (!Equals(kv.Value, valB))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool CompareMetadata(ReadOnlyDictionary<string, string> a, ReadOnlyDictionary<string, string> b)
        {
            if (a.Count != b.Count)
                return false;

            foreach (var kv in a)
            {
                if (!b.TryGetValue(kv.Key, out var valB) || kv.Value != valB)
                    return false;
            }
            return true;
        }

        private static bool CompareFilterData(FilterData a, FilterData b)
        {
            if (a.Rating != b.Rating)
                return false;

            if (a.GetFlag() != b.GetFlag())
                return false;

            var scoresA = a.GetScores();
            var scoresB = b.GetScores();

            if (scoresA.Count != scoresB.Count)
                return false;

            foreach (var kv in scoresA)
            {
                if (!scoresB.TryGetValue(kv.Key, out var valB) || kv.Value != valB)
                    return false;
            }

            return true;
        }
    }
}
