using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public static class HierarchicalRhythmInferrer
    {
        // Encuentra la unidad repetitiva más larga en una estructura rítmica
        public static RhythmStructure FindRepeatingUnit(RhythmStructure rhythm)
        {
            int rhythmLength = rhythm.Drums.Count;
            int maxRepeatingUnitLength = 4 * (rhythmLength / 4); // Máxima longitud en múltiplos de 4
            bool repeatingUnitFound = false;
            int unitLength = 4;

            var rhythmUnit = new RhythmStructure(rhythm.BeatInterval);

            while (!repeatingUnitFound && unitLength <= maxRepeatingUnitLength)
            {
                rhythmUnit = rhythm.CopySub(0, unitLength, rhythm.BeatInterval);
                int repetitions = rhythmLength / unitLength - 1;
                repeatingUnitFound = true;

                for (int i = 0; i < repetitions; i++)
                {
                    if (!rhythm.CheckMatch(rhythmUnit, (i + 1) * unitLength))
                    {
                        repeatingUnitFound = false;
                        break;
                    }
                }

                unitLength += 4;
            }

            return rhythmUnit;
        }

        // Crea una jerarquía rítmica a partir de una estructura rítmica plana
        public static HierarchicalRhythm CreateHierarchicalRhythm(RhythmStructure rhythm)
        {
            int treeDepth = (int)Math.Log2(rhythm.Drums.Count);
            var hRhythm = new HierarchicalRhythm(rhythm.BeatInterval, treeDepth);

            foreach (Drum drum in Enum.GetValues(typeof(Drum)))
            {
                for (int level = 0; level <= treeDepth; level++)
                {
                    int intervalBetweenNotes = (int)Math.Pow(2, level);

                    for (int shift = 0; shift < intervalBetweenNotes; shift++)
                    {
                        bool notesLineUp = true;

                        for (int i = shift; i < rhythm.Drums.Count; i += intervalBetweenNotes)
                        {
                            if (!rhythm.GetAtIndex(i).Contains(drum))
                            {
                                notesLineUp = false;
                                break;
                            }
                        }

                        if (notesLineUp)
                        {
                            hRhythm.AddDrum(level, shift, drum);

                            // Remueve los tambores que ya fueron alineados
                            for (int i = shift; i < rhythm.Drums.Count; i += intervalBetweenNotes)
                            {
                                rhythm.GetAtIndex(i).Remove(drum);
                            }
                        }
                    }
                }
            }

            return hRhythm;
        }

        // Compara dos jerarquías rítmicas para determinar si son equivalentes
        public static bool CompareHierarchicalRhythms(HierarchicalRhythm rhythm1, HierarchicalRhythm rhythm2)
        {
            return rhythm1.IsEqual(rhythm2);
        }

        // Convierte una jerarquía rítmica en un formato plano (RhythmStructure)
        public static RhythmStructure FlattenHierarchicalRhythm(HierarchicalRhythm hRhythm)
        {
            var rhythm = new RhythmStructure(hRhythm.Interval);

            // Recorre todos los niveles de la jerarquía y combina los tambores
            for (int i = 0; i < Math.Pow(2, hRhythm.GetLevel(0).Count); i++)
            {
                var combinedDrums = new HashSet<Drum>();

                foreach (var node in hRhythm.GetLevel(0))
                {
                    combinedDrums.UnionWith(node.Drums);
                }

                rhythm.AddDrums(combinedDrums);
            }

            return rhythm;
        }
    }
}
