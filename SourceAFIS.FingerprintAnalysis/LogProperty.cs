﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceAFIS.FingerprintAnalysis
{
    public class LogProperty
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public Func<bool, Options> Filter;
        public Func<bool, FingerprintOptions> FingerprintFilter;
    }
}
