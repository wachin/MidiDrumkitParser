using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public class HierarchicalRhythmNode
    {
        public HierarchicalRhythmNode Left { get; set; }
        public HierarchicalRhythmNode Right { get; set; }
        public HashSet<Drum> Drums { get; private set; }

        // Constructor para inicializar los tambores
        public HierarchicalRhythmNode()
        {
            Drums = new HashSet<Drum>();
        }

        // Establece los tambores de este nodo
        public void SetDrums(HashSet<Drum> drums)
        {
            Drums = drums;
        }

        // Crea una jerarquía profunda de nodos
        public void Deepen(int depth)
        {
            if (depth == 0) return;

            Left = new HierarchicalRhythmNode();
            Right = new HierarchicalRhythmNode();

            Left.Deepen(depth - 1);
            Right.Deepen(depth - 1);
        }

        // Imprime la estructura del nodo a un nivel específico
        public void Print(int level)
        {
            if (level == 0)
            {
                Console.Write("{");
                foreach (var drum in Drums)
                {
                    Console.Write($"{drum} ");
                }
                Console.Write("} ");
            }
            else
            {
                Left?.Print(level - 1);
                Right?.Print(level - 1);
            }
        }

        // Compara dos nodos para determinar si tienen los mismos tambores
        public bool IsEqual(HierarchicalRhythmNode otherNode)
        {
            return Drums.SetEquals(otherNode.Drums);
        }
    }

    public class HierarchicalRhythm
    {
        public HierarchicalRhythmNode Root { get; private set; }
        public double Interval { get; private set; }
        private int Depth { get; set; }

        // Constructor para inicializar la jerarquía
        public HierarchicalRhythm(double interval, int depth)
        {
            Interval = interval;
            Root = new HierarchicalRhythmNode();
            Root.Deepen(depth);
            Depth = depth;
        }

        // Agrega un tambor en un nivel y posición específicos
        public void AddDrum(int level, int index, Drum drum)
        {
            AddDrumAt(level, index, drum, Root);
        }

        private void AddDrumAt(int level, int index, Drum drum, HierarchicalRhythmNode node)
        {
            if (level == 0)
            {
                node.Drums.Add(drum);
            }
            else
            {
                int midPoint = (int)Math.Pow(2, level - 1);
                if (index < midPoint)
                {
                    AddDrumAt(level - 1, index, drum, node.Left);
                }
                else
                {
                    AddDrumAt(level - 1, index - midPoint, drum, node.Right);
                }
            }
        }

        // Imprime la jerarquía completa
        public void Print()
        {
            for (int i = 0; i <= Depth; i++)
            {
                Root.Print(i);
                Console.WriteLine();
            }
        }

        // Devuelve un nivel específico como una lista de nodos
        public List<HierarchicalRhythmNode> GetLevel(int level)
        {
            var nodes = new List<HierarchicalRhythmNode>();
            CollectLevelNodes(Root, level, nodes);
            return nodes;
        }

        private void CollectLevelNodes(HierarchicalRhythmNode node, int level, List<HierarchicalRhythmNode> nodes)
        {
            if (level == 0)
            {
                nodes.Add(node);
            }
            else
            {
                node.Left?.CollectLevelNodes(node.Left, level - 1, nodes);
                node.Right?.CollectLevelNodes(node.Right, level - 1, nodes);
            }
        }

        // Compara esta jerarquía con otra
        public bool IsEqual(HierarchicalRhythm otherRhythm)
        {
            return CompareNodes(Root, otherRhythm.Root);
        }

        private bool CompareNodes(HierarchicalRhythmNode thisNode, HierarchicalRhythmNode otherNode)
        {
            if (thisNode == null && otherNode == null) return true;
            if (thisNode == null || otherNode == null) return false;

            if (!thisNode.IsEqual(otherNode)) return false;

            return CompareNodes(thisNode.Left, otherNode.Left) &&
                   CompareNodes(thisNode.Right, otherNode.Right);
        }
    }
}
