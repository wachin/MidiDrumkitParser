using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public static class TempoInferrer
    {
        // Opciones de depuración
        private static bool DebugPrintEvents = true;
        private static bool DebugPrintClusters = false;
        private static bool DebugPrintRatedClusters = true;

        private const double ClusterWidth = 70; // Anchura de los clusters en milisegundos
        private const double EventWidth = 70;   // Tiempo para agrupar eventos en un único BeatEvent

        // Convierte una lista de NoteEvents en una lista de BeatEvents agrupados
        public static List<BeatEvent> NotesToEvents(List<NoteEvent> inputNoteEvents)
        {
            var events = new List<BeatEvent>();

            foreach (var note in inputNoteEvents)
            {
                double timestamp = note.Timestamp.TotalMilliseconds;
                bool eventFound = false;

                foreach (var evt in events)
                {
                    if (Math.Abs(timestamp - evt.Time) < EventWidth)
                    {
                        evt.AddNote(note);
                        eventFound = true;
                        break;
                    }
                }

                if (!eventFound)
                {
                    events.Add(new BeatEvent(note));
                }
            }

            if (DebugPrintEvents)
            {
                Console.WriteLine("Detected Beat Events:");
                foreach (var evt in events)
                {
                    Console.WriteLine($"Event at {evt.Time}ms with {evt.Notes.Count} notes.");
                }
            }

            return events;
        }

        // Convierte una lista de BeatEvents en clusters de intervalos
        public static List<IntervalCluster> EventsToClusters(List<BeatEvent> beatEvents)
        {
            var clusters = new List<IntervalCluster>();

            for (int i = 0; i < beatEvents.Count; i++)
            {
                for (int j = i + 1; j < beatEvents.Count; j++)
                {
                    var interval = new EventInterval(beatEvents[i], beatEvents[j]);

                    if (interval.Length < 2000) // Limita los intervalos largos a 2 segundos
                    {
                        var match = clusters.FirstOrDefault(c => Math.Abs(c.MeanLength - interval.Length) < ClusterWidth);
                        if (match != null)
                        {
                            match.AddInterval(interval);
                        }
                        else
                        {
                            clusters.Add(new IntervalCluster(interval));
                        }
                    }
                }
            }

            if (DebugPrintClusters)
            {
                Console.WriteLine("Interval Clusters:");
                foreach (var cluster in clusters)
                {
                    Console.WriteLine($"Cluster: Mean={cluster.MeanLength}ms, Intervals={cluster.Intervals.Count}");
                }
            }

            return clusters;
        }

        // Calcula un peso para clasificar los clusters en función de múltiplos pequeños
        private static int Weight(int i)
        {
            if (i >= 1 && i <= 4)
                return 6 - i;
            if (i >= 5 && i <= 8)
                return 1;
            return 0;
        }

        // Clasifica los clusters por su relevancia como tempos plausibles
        public static List<IntervalCluster> RateClusters(List<IntervalCluster> clusters)
        {
            foreach (var baseCluster in clusters)
            {
                foreach (var comparisonCluster in clusters)
                {
                    for (int i = 1; i < 9; i++)
                    {
                        if (Math.Abs(baseCluster.MeanLength - (i * comparisonCluster.MeanLength)) < ClusterWidth)
                        {
                            baseCluster.Rating += Weight(i) * comparisonCluster.Intervals.Count;
                        }
                    }
                }
            }

            // Ordena los clusters por su puntuación y elimina valores fuera de rango
            clusters = clusters.OrderByDescending(c => c.Rating)
                               .Where(c => c.MeanLength >= 250 && c.MeanLength <= 1000) // Entre 60 y 240 BPM
                               .ToList();

            if (DebugPrintRatedClusters)
            {
                Console.WriteLine("Rated Clusters:");
                foreach (var cluster in clusters)
                {
                    Console.WriteLine($"Cluster: {cluster.GetBPM()} BPM, Intervals={cluster.Intervals.Count}, Rating={cluster.Rating}");
                }
            }

            return clusters;
        }

        // Genera un resumen de los clusters detectados
        public static void PrintClusterSummary(List<IntervalCluster> clusters)
        {
            Console.WriteLine("Cluster Summary:");
            foreach (var cluster in clusters)
            {
                Console.WriteLine($"Mean Length: {cluster.MeanLength}ms, Rating: {cluster.Rating}, BPM: {cluster.GetBPM()}");
            }
        }
    }
}
