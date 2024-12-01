using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MIDI_Drumkit_Parser
{
    public struct LabelSymbolPair
    {
        public string Label;
        public string Symbol;

        public LabelSymbolPair(string label, string symbol)
        {
            Label = label;
            Symbol = symbol;
        }
    }

    public static class AsciiTabRenderer
    {
        private static readonly Dictionary<Drum, LabelSymbolPair> DrumToName = new()
        {
            [Drum.Snare] = new LabelSymbolPair(" S", "o"),
            [Drum.TomHigh] = new LabelSymbolPair("T1", "o"),
            [Drum.TomMid] = new LabelSymbolPair("T2", "o"),
            [Drum.TomLow] = new LabelSymbolPair("FT", "o"),
            [Drum.HatOpen] = new LabelSymbolPair("HH", "o"),
            [Drum.CrashLeft] = new LabelSymbolPair("LC", "x"),
            [Drum.CrashRight] = new LabelSymbolPair("RC", "x"),
            [Drum.Kick] = new LabelSymbolPair("BD", "o"),
            [Drum.HatClosing] = new LabelSymbolPair("HH", "x"),
            [Drum.HatClosed] = new LabelSymbolPair("HH", "x")
        };

        public static void RenderAsciiTab(RhythmStructure rhythm)
        {
            var tab = new Dictionary<string, string>();

            foreach (var pair in DrumToName.Values)
            {
                tab[pair.Label] = $"{pair.Label}|";
            }

            for (int i = 0; i < rhythm.Drums.Count; i++)
            {
                var drums = rhythm.Drums[i];
                foreach (var index in tab.Keys)
                {
                    tab[index] += "-";
                }

                foreach (var drum in drums)
                {
                    if (DrumToName.ContainsKey(drum))
                    {
                        var pair = DrumToName[drum];
                        tab[pair.Label] = tab[pair.Label].Remove(tab[pair.Label].Length - 1).Insert(tab[pair.Label].Length - 1, pair.Symbol);
                    }
                }
            }

            using var writer = new StreamWriter("tab.txt");
            writer.WriteLine(Convert.ToInt32(rhythm.BeatInterval));
            foreach (var str in tab.Values)
            {
                writer.WriteLine(str);
            }

            Console.WriteLine("ASCII Tab rendered to tab.txt.");
        }
    }
}
