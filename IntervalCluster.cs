using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public class EventInterval
    {
        public double Length { get; private set; }
        public BeatEvent StartEvent { get; private set; }
        public BeatEvent EndEvent { get; private set; }

        public EventInterval(BeatEvent start, BeatEvent end)
        {
            StartEvent = start;
            EndEvent = end;
            Length = end.Time - start.Time;
        }

        public void Print()
        {
            Console.WriteLine($"Interval: {Length} ms (Start: {StartEvent.Time} ms, End: {EndEvent.Time} ms)");
        }
    }

    public class IntervalCluster
    {
        public List<EventInterval> Intervals { get; private set; }
        public double MeanLength { get; private set; }
        public int Rating { get; set; }

        public IntervalCluster(EventInterval initialInterval)
        {
            Intervals = new List<EventInterval> { initialInterval };
            MeanLength = initialInterval.Length;
            Rating = 0;
        }

        public void AddInterval(EventInterval interval)
        {
            Intervals.Add(interval);
            MeanLength = (MeanLength * (Intervals.Count - 1) + interval.Length) / Intervals.Count;
        }

        public double GetBPM()
        {
            return 60000 / MeanLength;
        }

        public void PrintClusterDetails()
        {
            Console.WriteLine($"Cluster Details: Mean Length = {MeanLength} ms, BPM = {GetBPM()}, Rating = {Rating}");
            foreach (var interval in Intervals)
            {
                interval.Print();
            }
        }

        public double ComputeStandardDeviation()
        {
            if (Intervals.Count <= 1)
            {
                return 0;
            }

            double sum = 0;
            foreach (var interval in Intervals)
            {
                double deviation = interval.Length - MeanLength;
                sum += deviation * deviation;
            }

            return Math.Sqrt(sum / Intervals.Count);
        }
    }
}
