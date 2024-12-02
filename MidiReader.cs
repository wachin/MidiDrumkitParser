using System;
using Commons.Music.Midi;

namespace MIDI_Drumkit_Parser
{
    public class MidiReader
    {
        private IMidiInput input;

        public void Start()
        {
            var access = MidiAccessManager.Default; // Obtener el acceso MIDI predeterminado
            var inputs = access.Inputs;

            if (inputs.Count == 0)
            {
                Console.WriteLine("No se encontraron dispositivos MIDI de entrada.");
                return;
            }

            // Abrir el primer dispositivo de entrada MIDI
            input = access.OpenInputAsync(inputs[0].Id).Result;
            input.MessageReceived += OnMessageReceived;
            Console.WriteLine($"Dispositivo MIDI conectado: {inputs[0].Name}");
        }

        private void OnMessageReceived(object sender, MidiReceivedEventArgs e)
        {
            Console.WriteLine("Mensaje MIDI recibido:");
            foreach (var data in e.Data)
            {
                Console.Write($"{data:X2} ");
            }
            Console.WriteLine();
        }

        public void Stop()
        {
            if (input != null)
            {
                input.CloseAsync().Wait();
                Console.WriteLine("Conexión MIDI cerrada.");
            }
        }
    }
}
