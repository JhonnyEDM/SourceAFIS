using System;
using System.Collections.Generic;
using System.Text;
using SourceAFIS.General;
using System.Xml.Serialization;

namespace SourceAFIS.Tuning.Errors
{
    public struct ROCPoint
    {
        [XmlAttribute]
        public float FAR;
        [XmlAttribute]
        public float FRR;
        [XmlAttribute]
        public float Threshold;

        public void Average(List<ROCPoint> partial)
        {
            FAR = Calc.Average(partial.ConvertAll<float>(point => point.FAR));
            FRR = Calc.Average(partial.ConvertAll<float>(point => point.FRR));
            Threshold = Calc.Average(partial.ConvertAll<float>(point => point.Threshold));
        }
    }
}
