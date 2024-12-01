/* This project uses the .NET MIDI library, under the MIT license.
 * As per the MIT license, the license text is included below.
 * 
 * Copyright (c) 2006 Leslie Sanford
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software. 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Welcome to MIDI Drumkit Parser!");
            Console.WriteLine("This program converts MIDI drum tracks into ASCII tablatures and Sonic Pi code.");
            Console.WriteLine("Press any key to begin...");
            Console.ReadKey();

            // Inicializa el lector MIDI
            var reader = new MidiReader();

            Console.WriteLine("Press any key to start recording MIDI input...");
            Console.ReadKey();
            reader.Start();

            Console.WriteLine("Recording MIDI events. Press any key to stop.");
            Console.ReadKey();
            reader.Stop();

            // Procesa los datos MIDI capturados
            Console.WriteLine("Processing MIDI data...");
            var noteEvents = reader.FetchEventList();

            if (!noteEvents.Any())
            {
                Console.WriteLine("No MIDI events were captured. Exiting...");
                return;
            }

            // Convertir notas a eventos rítmicos
            var beatEvents = TempoInferrer.NotesToEvents(noteEvents);

            // Inferencia de tempo
            var clusters = TempoInferrer.EventsToClusters(beatEvents);
            var ratedClusters = TempoInferrer.RateClusters(clusters);

            if (!ratedClusters.Any())
            {
                Console.WriteLine("No valid tempo clusters found. Exiting...");
                return;
            }

            // Determina el tempo final
            var finalBeat = BeatInferrer.FindBeat(ratedClusters, beatEvents);

            // Genera la estructura rítmica
            var rhythm = RhythmCreator.CreateRhythm(finalBeat, beatEvents);

            Console.WriteLine("Generating ASCII tablature...");
            AsciiTabRenderer.RenderAsciiTab(rhythm);

            Console.WriteLine("Generating Sonic Pi code...");
            SonicPiEmitter.EmitSonicPi();

            Console.WriteLine("Processing complete!");
            Console.WriteLine("Outputs:");
            Console.WriteLine(" - ASCII tablature saved as 'tab.txt'");
            Console.WriteLine(" - Sonic Pi code saved as 'sonicpi.txt'");

            // Opcional: Muestra el mapeo de muestras de Sonic Pi
            Console.WriteLine("\nWould you like to see the Sonic Pi sample mapping? (y/n)");
            var showMapping = Console.ReadLine();
            if (showMapping?.ToLower() == "y")
            {
                SonicPiEmitter.PrintSampleMapping();
            }

            // Finalización del programa
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
