using System;
using System.Collections.Generic;
using System.Text;
using SourceAFIS.Meta;
using SourceAFIS.General;
using SourceAFIS.Extraction.Templates;

namespace SourceAFIS.Matching.Minutia
{
    public sealed class MinutiaMatcher
    {
        [Nested]
        public ExhaustiveRootSelector RootSelector = new ExhaustiveRootSelector();
        [Nested]
        public MinutiaPairing Pairing = new MinutiaPairing();
        [Nested]
        public EdgeTable EdgeTablePrototype = new EdgeTable();
        [Nested]
        public EdgeConstructor EdgeConstructor = new EdgeConstructor();
        [Nested]
        public PairSelector PairSelector = new PairSelector();
        [Nested]
        public MatchAnalysis MatchAnalysis = new MatchAnalysis();
        [Nested]
        public MatchScoring MatchScoring = new MatchScoring();
        [Nested]
        public EdgeLookup EdgeLookup = new EdgeLookup();

        [Parameter(Upper = 10000)]
        public int MaxTriedRoots = 10000;

        ProbeIndex Probe;
        EdgeTable CandidateEdges = new EdgeTable();

        public ProbeIndex CreateIndex(Template probe)
        {
            ProbeIndex index = new ProbeIndex();
            index.Template = probe;
            index.Edges = ParameterSet.ClonePrototype(EdgeTablePrototype);
            index.Edges.Reset(probe);
            return index;
        }

        public void SelectProbe(ProbeIndex probe)
        {
            Probe = probe;
            Pairing.SelectProbe(probe.Template);
        }

        public float Match(Template candidate)
        {
            PrepareCandidate(candidate);

            int rootIndex = 0;
            float bestScore = 0;
            int bestRootIndex = -1;
            foreach (MinutiaPair root in RootSelector.GetRoots(Probe.Template, candidate))
            {
                Logger.Log(this, "Root", root);
                float score = TryRoot(root, candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestRootIndex = rootIndex;
                }
                ++rootIndex;
                if (rootIndex >= MaxTriedRoots)
                    break;
            }
            Logger.Log(this, "BestRootIndex", bestRootIndex);
            Logger.Log(this, "Score", bestScore);
            return bestScore;
        }

        void PrepareCandidate(Template candidate)
        {
            Pairing.SelectCandidate(candidate);
            PairSelector.Clear();
            CandidateEdges.Reset(candidate);
        }

        float TryRoot(MinutiaPair root, Template candidate)
        {
            Pairing.Reset();
            Pairing.Add(root);
            BuildPairing(candidate);

            MatchAnalysis.Analyze(Pairing, Probe.Template, candidate);
            return MatchScoring.Compute(MatchAnalysis);
        }

        void BuildPairing(Template candidate)
        {
            while (true)
            {
                CollectEdges(candidate);
                PairSelector.SkipPaired(Pairing);
                if (PairSelector.Count == 0)
                    break;
                Pairing.Add(PairSelector.Dequeue());
            }
            Logger.Log(this, "Pairing", Pairing);
        }

        void CollectEdges(Template candidate)
        {
            List<EdgeLookup.EdgePair> edgePairs = EdgeLookup.FindMatchingPairs(
                Probe.Edges.Table[Pairing.LastAdded.Probe], CandidateEdges.Table[Pairing.LastAdded.Candidate]);
            foreach (EdgeLookup.EdgePair edgePair in edgePairs)
            {
                NeighborEdge probeEdge = Probe.Edges.Table[Pairing.LastAdded.Probe][edgePair.ProbeIndex];
                NeighborEdge candidateEdge = CandidateEdges.Table[Pairing.LastAdded.Candidate][edgePair.CandidateIndex];
                if (!Pairing.IsCandidatePaired(candidateEdge.Neighbor) && !Pairing.IsProbePaired(probeEdge.Neighbor))
                    PairSelector.Enqueue(new MinutiaPair(probeEdge.Neighbor, candidateEdge.Neighbor), candidateEdge.Edge.Length);
                else if (Pairing.GetCandidateByProbe(probeEdge.Neighbor) == candidateEdge.Neighbor)
                    Pairing.AddSupportByProbe(probeEdge.Neighbor);
            }
        }
    }
}
