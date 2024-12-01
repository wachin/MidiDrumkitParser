using System;
using System.Collections.Generic;
using Commons.Music.Midi;

namespace MIDI_Drumkit_Parser
{
    public struct NoteEvent
    {
        public byte Channel;
        public byte Velocity;
        public TimeSpan Timestamp;

        public NoteEvent(byte channel, byte velocity, TimeSpan timestamp)
        {
            Channel = channel;
            Velocity = velocity;
            Timestamp = timestamp;
        }
    }

    public class MidiReader
    {
        // Opciones de depuración
        private bool DebugPrintMIDI = true;    // Muestra todos los mensajes MIDI
        private bool DebugPrintTimestamps = true; // Muestra la secuencia de notas capturadas

        // Variables internas
        private List<NoteEvent> noteEvents;
        private MidiAccessManager midiAccess;
        private IMidiInput inputDevice;
        private DateTime baseTime;

        public MidiReader()
        {
            // Inicializa la lista de eventos
            noteEvents = new List<NoteEvent>();
            midiAccess = MidiAccessManager.Default;

            // Verifica si hay dispositivos MIDI disponibles
            if (midiAccess.Inputs.Count == 0)
            {
                Console.WriteLine("No MIDI devices found.");
                Environment.Exit(0);
            }

            // Abre el primer dispositivo MIDI disponible
            inputDevice = midiAccess.OpenInputAsync(midiAccess.Inputs[0].Id).Result;
            inputDevice.MessageReceived += OnMessageReceived;

            Console.WriteLine($"MIDI device connected: {inputDevice.Details.Name}");
        }

        public void Start()
        {
            // Inicia la grabación
            baseTime = DateTime.Now;
            Console.WriteLine("Starting MIDI recording...");
        }

        public void Stop()
        {
            // Finaliza la grabación
            inputDevice.MessageReceived -= OnMessageReceived;
            inputDevice.Dispose();

            Console.WriteLine("Recording stopped.");
            if (DebugPrintTimestamps)
            {
                foreach (var evt in noteEvents)
                {
                    Console.WriteLine($"Note {evt.Channel}, Vel {evt.Velocity}, Time {evt.Timestamp.TotalMilliseconds}");
                }
            }
        }

        public List<NoteEvent> FetchEventList()
        {
            return noteEvents;
        }

        private void OnMessageReceived(object sender, MidiReceivedEventArgs e)
        {
            // Procesa los mensajes MIDI recibidos
            int index = 0;
            while (index < e.Length)
            {
                byte status = e.Data[index];
                byte messageType = (byte)(status & 0xF0); // Máscara para obtener el tipo de mensaje
                byte channel = (byte)(status & 0x0F); // Máscara para obtener el canal
                byte data1 = e.Data[index + 1];
                byte data2 = e.Data[index + 2];

                if (DebugPrintMIDI)
                {
                    Console.WriteLine($"MIDI Message: Status={status}, Type={messageType}, Channel={channel}, Data1={data1}, Data2={data2}");
                }

                // Filtra los mensajes relevantes
                switch (messageType)
                {
                    case 0x90: // NoteOn
                        if (data2 > 0) // Velocidad > 0 indica que es una nota activa
                        {
                            if (noteEvents.Count == 0)
                                baseTime = DateTime.Now;

                            noteEvents.Add(new NoteEvent(data1, data2, DateTime.Now - baseTime));
                        }
                        break;

                    case 0x80: // NoteOff
                        // Opcional: Procesar eventos NoteOff si es necesario
                        break;

                    case 0xF0: // SysEx Message
                        Console.WriteLine("SysEx Message received.");
                        break;

                    case 0xB0: // Control Change
                        Console.WriteLine($"Control Change: Controller={data1}, Value={data2}");
                        break;

                    case 0xC0: // Program Change
                        Console.WriteLine($"Program Change: Program={data1}");
                        break;

                    case 0xE0: // Pitch Bend
                        Console.WriteLine($"Pitch Bend: Value={data1 + (data2 << 7)}");
                        break;

                    default:
                        Console.WriteLine($"Unhandled MIDI Message: {status}");
                        break;
                }

                // Incrementa el índice para procesar el siguiente mensaje (3 bytes por mensaje MIDI)
                index += 3;
            }
        }
    }
}
