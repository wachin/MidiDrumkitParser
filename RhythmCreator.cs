using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public static class RhythmCreator
    {
        // Mapeo entre valores MIDI y tipos de tambores
        private static readonly Dictionary<int, Drum> MidiToDrumMapping = new()
        {
            [38] = Drum.Snare,
            [48] = Drum.TomHigh,
            [45] = Drum.TomMid,
            [43] = Drum.TomLow,
            [46] = Drum.HatOpen,
            [44] = Drum.HatClosing,
            [42] = Drum.HatClosed,
            [49] = Drum.CrashLeft,
            [51] = Drum.CrashRight,
            [36] = Drum.Kick
        };

        // Crea una estructura rítmica basada en los eventos y el tracker de beats
        public static RhythmStructure CreateRhythm(BeatTracker beatTracker, List<BeatEvent> beatEvents)
        {
            var rhythm = new RhythmStructure(beatTracker.Interval / 4);

            foreach (var evt in beatEvents)
            {
                int index = (int)(evt.Time / rhythm.BeatInterval);

                while (rhythm.Drums.Count <= index)
                {
                    rhythm.Drums.Add(new HashSet<Drum>());
                }

                foreach (var note in evt.Notes)
                {
                    if (MidiToDrumMapping.ContainsKey(note.Channel))
                    {
                        rhythm.Drums[index].Add(MidiToDrumMapping[note.Channel]);
                    }
                }
            }

            return rhythm;
        }

        // Genera una estructura rítmica simplificada a partir de eventos
        public static RhythmStructure SimplifyRhythm(RhythmStructure rhythm, int factor)
        {
            var simplifiedRhythm = new RhythmStructure(rhythm.BeatInterval * factor);

            for (int i = 0; i < rhythm.Drums.Count; i += factor)
            {
                var combinedDrums = new HashSet<Drum>();
                for (int j = 0; j < factor && i + j < rhythm.Drums.Count; j++)
                {
                    combinedDrums.UnionWith(rhythm.GetAtIndex(i + j));
                }
                simplifiedRhythm.AddDrums(combinedDrums);
            }

            return simplifiedRhythm;
        }

        // Imprime el mapeo MIDI a consola para depuración
        public static void PrintMidiMapping()
        {
            Console.WriteLine("MIDI to Drum Mapping:");
            foreach (var mapping in MidiToDrumMapping)
            {
                Console.WriteLine($"MIDI Value: {mapping.Key}, Drum: {mapping.Value}");
            }
        }
    }
}
