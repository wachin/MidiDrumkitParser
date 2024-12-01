using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public class BeatTracker
    {
        public double Interval { get; set; }
        public double NextPrediction { get; set; }
        public List<BeatEvent> ProcessedItems { get; set; }
        public double Rating { get; set; }
        public double OriginalScore { get; set; }

        private const bool DebugPrintTrackers = true;

        // Constructor principal
        public BeatTracker(double interval, BeatEvent firstEvent, double originalScore)
        {
            Interval = interval;
            NextPrediction = firstEvent.Time + interval;
            ProcessedItems = new List<BeatEvent> { firstEvent };
            Rating = firstEvent.Notes.Sum(n => n.Velocity);
            OriginalScore = originalScore;
        }

        // Constructor de copia
        public BeatTracker(BeatTracker tracker)
        {
            Interval = tracker.Interval;
            NextPrediction = tracker.NextPrediction;
            ProcessedItems = new List<BeatEvent>(tracker.ProcessedItems);
            Rating = tracker.Rating;
            OriginalScore = tracker.OriginalScore;
        }

        // Mezcla la información de dos trackers, manteniendo el de mejor puntuación
        public void TakeBestTracker(BeatTracker tracker)
        {
            if (tracker.Rating > Rating)
            {
                Interval = tracker.Interval;
                NextPrediction = tracker.NextPrediction;
                ProcessedItems = new List<BeatEvent>(tracker.ProcessedItems);
                Rating = tracker.Rating;
                OriginalScore = tracker.OriginalScore;
            }
        }

        // Imprime la información del tracker para depuración
        public void PrintTracker()
        {
            if (DebugPrintTrackers)
            {
                Console.WriteLine($"Interval: {Interval} ms, or {GetBPM()} BPM");
                Console.WriteLine($"Beat Events Processed: {ProcessedItems.Count}");
                Console.WriteLine($"Rating: {Rating}");
            }
        }

        // Calcula el BPM basado en el intervalo
        public double GetBPM()
        {
            return (1 / Interval) * 60 * 1000;
        }
    }

    public static class BeatInferrer
    {
        private const double InnerWindow = 40; // Tolerancia para predicciones precisas
        private const double OuterWindowFactor = 0.3; // Tolerancia extendida para predicciones
        private const double InitialPeriod = 5000; // Primeros eventos usados para inicializar trackers
        private const double MaximumInterval = 3000; // Tiempo máximo entre eventos en un tracker
        private const double CorrectionFactor = 0.2; // Ajuste del intervalo según errores

        // Compara trackers para verificar si son similares (fuzzy matching)
        private static bool SimilarTrackers(BeatTracker first, BeatTracker second)
        {
            return Math.Abs(first.Interval - second.Interval) < 10 &&
                   Math.Abs(first.NextPrediction - second.NextPrediction) < 20;
        }

        // Encuentra el mejor tracker que alinea los eventos con el tempo detectado
        public static BeatTracker FindBeat(List<IntervalCluster> tempoHypotheses, List<BeatEvent> events)
        {
            var trackers = new List<BeatTracker>();

            // Crea trackers iniciales basados en las hipótesis de tempo
            foreach (var cluster in tempoHypotheses)
            {
                foreach (var startEvent in events.Where(e => e.Time < InitialPeriod))
                {
                    trackers.Add(new BeatTracker(cluster.MeanLength, startEvent, cluster.Rating));
                }
            }

            // Procesa cada evento en la lista
            foreach (var evt in events)
            {
                var newTrackers = new List<BeatTracker>();

                for (int i = trackers.Count - 1; i >= 0; i--)
                {
                    var tracker = trackers[i];

                    // Elimina trackers que hayan excedido el intervalo máximo
                    if (evt.Time - tracker.ProcessedItems.Last().Time > MaximumInterval)
                    {
                        trackers.RemoveAt(i);
                    }
                    else
                    {
                        // Actualiza predicciones futuras del tracker
                        while (tracker.NextPrediction + (OuterWindowFactor * tracker.Interval) < evt.Time)
                        {
                            tracker.NextPrediction += tracker.Interval;
                        }

                        // Verifica si el evento se alinea con la predicción del tracker
                        if (evt.Time > tracker.NextPrediction - (OuterWindowFactor * tracker.Interval) &&
                            evt.Time < tracker.NextPrediction + (OuterWindowFactor * tracker.Interval))
                        {
                            // Crea un nuevo tracker si la predicción es imprecisa
                            if (Math.Abs(evt.Time - tracker.NextPrediction) > InnerWindow)
                            {
                                newTrackers.Add(new BeatTracker(tracker));
                            }

                            // Ajusta el tracker actual para alinearse con el evento
                            double error = evt.Time - tracker.NextPrediction;
                            tracker.Interval += error * CorrectionFactor;
                            tracker.NextPrediction = evt.Time + tracker.Interval;
                            tracker.ProcessedItems.Add(evt);
                            tracker.Rating += (1 - (Math.Abs(error) / (2 * tracker.NextPrediction))) * evt.Notes.Sum(n => n.Velocity);
                        }
                    }
                }

                trackers.AddRange(newTrackers);

                // Elimina trackers duplicados que sean funcionalmente iguales
                var nextTrackers = new List<BeatTracker>();
                foreach (var tracker in trackers)
                {
                    bool matchFound = false;
                    foreach (var nextTracker in nextTrackers)
                    {
                        if (!matchFound && SimilarTrackers(tracker, nextTracker))
                        {
                            nextTracker.TakeBestTracker(tracker);
                            matchFound = true;
                        }
                    }
                    if (!matchFound)
                    {
                        nextTrackers.Add(tracker);
                    }
                }

                trackers = nextTrackers;
            }

            // Ajusta las puntuaciones finales usando la puntuación original de los clusters
            foreach (var tracker in trackers)
            {
                tracker.Rating *= tracker.OriginalScore;
            }

            // Ordena los trackers por puntuación y devuelve el mejor
            trackers = trackers.OrderByDescending(t => t.Rating).ToList();
            foreach (var tracker in trackers.Take(10))
            {
                tracker.PrintTracker();
            }

            return trackers.First();
        }
    }
}
