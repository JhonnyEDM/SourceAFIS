using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SourceAFIS.Tuning.Reports
{
    public sealed class DatabaseStatistics
    {
        public int TotalDatabases;
        public int TotalFingers;
        public int TotalViews;
        public int MatchingPairCount;
        public int NonMatchingPairCount;

        public void Collect(TestDatabase database)
        {
            TotalDatabases = database.Databases.Count;
            TotalFingers = database.AllFingers.Count();
            TotalViews = database.AllViews.Count();
            MatchingPairCount = database.GetMatchingPairCount();
            NonMatchingPairCount = database.GetNonMatchingPairCount();
        }
    }
}
