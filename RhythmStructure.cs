using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    // Tipos de batería
    public enum Drum
    {
        Snare, TomHigh, TomMid, TomLow, HatOpen, HatClosed, HatClosing, CrashLeft, CrashRight, Kick
    }

    // Estructura principal del ritmo
    public class RhythmStructure
    {
        public double BeatInterval { get; set; } // Intervalo entre beats en milisegundos
        public List<HashSet<Drum>> Drums { get; set; } // Lista de conjuntos de tambores en cada semiquaver

        public RhythmStructure(double beatInterval)
        {
            BeatInterval = beatInterval;
            Drums = new List<HashSet<Drum>>();
        }

        // Constructor de copia profunda
        public RhythmStructure(RhythmStructure rhythm)
        {
            BeatInterval = rhythm.BeatInterval;
            Drums = new List<HashSet<Drum>>();
            foreach (var drumSet in rhythm.Drums)
            {
                Drums.Add(new HashSet<Drum>(drumSet));
            }
        }

        // Obtiene los tambores tocados en una semiquaver específica
        public HashSet<Drum> GetAtIndex(int beatIndex, int semiQIndex)
        {
            return Drums[beatIndex * 4 + semiQIndex];
        }

        public HashSet<Drum> GetAtIndex(int index)
        {
            return Drums[index];
        }

        // Agrega un conjunto de tambores a la estructura
        public void AddDrums(HashSet<Drum> drumsIn)
        {
            Drums.Add(drumsIn);
        }

        // Elimina un tambor específico de un índice
        public void RemoveDrumAt(int index, Drum drum)
        {
            if (Drums[index].Contains(drum))
            {
                Drums[index].Remove(drum);
            }
        }

        // Copia un segmento de la estructura
        public RhythmStructure CopySub(int startIndex, int length, double interval)
        {
            var copy = new RhythmStructure(interval);
            for (int i = startIndex; i < startIndex + length; i++)
            {
                copy.AddDrums(new HashSet<Drum>(GetAtIndex(i)));
            }
            return copy;
        }

        // Verifica si un segmento de la estructura coincide con otra
        public bool CheckMatch(RhythmStructure otherRhythm, int startIndex)
        {
            for (int i = 0; i < otherRhythm.Drums.Count; i++)
            {
                if (!Drums[startIndex + i].SetEquals(otherRhythm.Drums[i]))
                {
                    return false;
                }
            }
            return true;
        }

        // Método para imprimir la estructura completa
        public void Print()
        {
            Console.WriteLine($"Beat Interval: {BeatInterval} ms");
            for (int i = 0; i < Drums.Count; i++)
            {
                Console.Write($"SemiQuaver {i}: ");
                foreach (var drum in Drums[i])
                {
                    Console.Write($"{drum} ");
                }
                Console.WriteLine();
            }
        }
    }
}
