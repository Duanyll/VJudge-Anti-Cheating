using System.Collections.Generic;

namespace vjac{
    public struct CheckResult
    {
        public Status Status { get; set; }
        public bool IsCheat { get; set; }
        public List<(string Source, int Similarity)> PossibleSources{ get; set; }
    }
}