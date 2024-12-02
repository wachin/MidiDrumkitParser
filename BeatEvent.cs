using System.Collections.Generic;

namespace MIDI_Drumkit_Parser
{
    public class BeatEvent
    {
        public double Time { get; set; } // Tiempo en milisegundos
        public List<Note> Notes { get; set; } // Notas asociadas al evento

        public BeatEvent(double time)
        {
            Time = time;
            Notes = new List<Note>();
        }
    }

    public class Note
    {
        public int Channel { get; set; } // Canal MIDI
        public int Velocity { get; set; } // Velocidad de la nota

        public Note(int channel, int velocity)
        {
            Channel = channel;
            Velocity = velocity;
        }
    }
}
