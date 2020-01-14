﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    /* Types of Drum. */
    public enum Drum
    {
        Snare, TomHigh, TomMid, TomLow, HatOpen, HatClosed, HatClosing, CrashLeft, CrashRight, Kick
    }

    /* A class for a rhythm, making up a list of the drums used in each semiquaver,
     * and the interval between beats in milliseconds. */
    public class RhythmStructure
    {
        public double beatInterval;
        public List<HashSet<Drum>> drums;

        public RhythmStructure(double _beatInterval)
        {
            beatInterval = _beatInterval;
            drums = new List<HashSet<Drum>>();
        }

        /* Functions to get the set of Drums played on a certain semiquaver,
         * add drums to a semiquaver, and copy a subsection of the rhythm. */
        public HashSet<Drum> GetAtIndex(int beatIndex, int semiQIndex)
        {
            return drums[beatIndex * 4 + semiQIndex];
        }

        public HashSet<Drum> GetAtIndex(int index)
        {
            return drums[index];
        }

        public void AddDrums(HashSet<Drum> drumsIn)
        {
            drums.Add(drumsIn);
        }

        public RhythmStructure CopySub(int startIndex, int length, double interval)
        {
            RhythmStructure copy = new RhythmStructure(interval);

            for (int i = startIndex; i < startIndex + length; i++)
            {
                copy.AddDrums(GetAtIndex(i));
            }

            return copy;
        }

        // Check all of the input rhythm against the subsection of this rhythm given by the parameters.
        public bool CheckMatch(RhythmStructure otherRhythm, int startIndex)
        {
            bool match = true;

            for (int i = startIndex; i < startIndex + otherRhythm.drums.Count; i++)
            {
                int otherRhythmIndex = i - startIndex;
                if (!drums[i].SetEquals(otherRhythm.drums[otherRhythmIndex]))
                {
                    match = false;
                }
            }

            return match;
        }
    }

    public class HierarchicalRhythmNode
    {
        public HierarchicalRhythmNode Left;
        public HierarchicalRhythmNode Right;
        public HashSet<Drum> drums;

        public HierarchicalRhythmNode()
        {
            drums = new HashSet<Drum>();
        }

        public void SetDrums(HashSet<Drum> _drums)
        {
            drums = _drums;
        }

        public void Deepen(int depth)
        {
            if (depth == 0)
            {
                return;
            }
            else
            {
                Left = new HierarchicalRhythmNode();
                Left.Deepen(depth - 1);
                Right = new HierarchicalRhythmNode();
                Right.Deepen(depth - 1);
            }
        }
    }

    public class HierarchicalRhythm
    {
        public HierarchicalRhythmNode Root;
        public double interval;

        // Here "depth" is the base 2 logarithm of the number of units in the rhythm.
        public HierarchicalRhythm(double _interval, int depth)
        {
            interval = _interval;
            Root = new HierarchicalRhythmNode();
            Root.Deepen(depth);
        }

        // Here "level" is the base 2 logarithm of the number of units on that level.
        private void AddDrumAt(int level, int index, Drum drum, HierarchicalRhythmNode node)
        {
            if (level < 0)
                throw new Exception("Attempted to add drum at level " + level);

            // Top of tree, insert here
            if (level == 0)
            {
                node.drums.Add(drum);
            }
            else
            {
                // Go down the left branch
                if (index < Math.Pow(2, level))
                {
                    AddDrumAt(level - 1, index, drum, node.Left);
                }
                // Go down the right branch
                else
                {
                    int newIndex = index - Convert.ToInt32(Math.Pow(2, level - 1));
                    AddDrumAt(level - 1, newIndex, drum, node.Right);
                }
            }
        }

        public void AddDrum(int level, int index, Drum drum)
        {
            AddDrumAt(level, index, drum, Root);
        }
    }
}