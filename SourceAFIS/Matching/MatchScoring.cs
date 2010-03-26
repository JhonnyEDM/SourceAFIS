using System;
using System.Collections.Generic;
using System.Text;
using SourceAFIS.Meta;

namespace SourceAFIS.Matching
{
    public sealed class MatchScoring
    {
        [Parameter(Upper = 10)]
        public float PairCountFactor = 0.48f;
        [Parameter(Upper = 100)]
        public float PairFractionFactor = 8.26f;
        [Parameter(Upper = 10)]
        public float CorrectTypeFactor = 0.1f;
        [Parameter(Upper = 10)]
        public float SupportedCountFactor = 0.4f;
        [Parameter(Upper = 10, Precision = 3)]
        public float EdgeCountFactor = 0.208f;

        public float Compute(MatchAnalysis analysis)
        {
            float score = 0;
            score += PairCountFactor * analysis.PairCount;
            score += CorrectTypeFactor * analysis.CorrectTypeCount;
            score += SupportedCountFactor * analysis.SupportedCount;
            score += PairFractionFactor * analysis.PairFraction;
            score += EdgeCountFactor * analysis.EdgeCount;
            return score;
        }
    }
}
