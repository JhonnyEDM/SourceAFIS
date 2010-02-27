using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Drawing;

namespace SourceAFIS.General
{
    public sealed class Threader
    {
        public delegate void Task();
        public delegate void RangeFunction(Range range);
        public delegate void IterationFunction(int at);
        public delegate void ApplyFunction<T>(T value);

        public abstract class Ticket
        {
            public abstract void Wait();
        }

        public static readonly int HwThreadCount = Environment.ProcessorCount;
        public static readonly int WorkerCount = HwThreadCount - 1;

        static List<Worker> Workers = new List<Worker>();
        static List<bool> AvaliableWorkers = new List<bool>();

        static Threader()
        {
            for (int i = 0; i < WorkerCount; ++i)
            {
                Worker worker = new Worker();
                Thread thread = new Thread(worker.Run);
                thread.IsBackground = true;
                thread.Start();
                Workers.Add(worker);
                AvaliableWorkers.Add(true);
            }
        }

        public static Ticket Schedule(Task task)
        {
            TicketImplementation ticket = new TicketImplementation(task);
            
            lock (AvaliableWorkers)
            {
                for (int i = 0; i < AvaliableWorkers.Count; ++i)
                    if (AvaliableWorkers[i])
                    {
                        AvaliableWorkers[i] = false;
                        int savedIndex = i;
                        Workers[i].Submit(ticket, delegate()
                        {
                            lock (AvaliableWorkers)
                                AvaliableWorkers[savedIndex] = true;
                        });
                        return ticket;
                    }
            }

            ticket.Run();
            return ticket;
        }

        public static void Wait(IEnumerable<Ticket> tickets)
        {
            foreach (Ticket ticket in tickets)
                ticket.Wait();
        }

        public static void Split(Range range, RangeFunction function)
        {
            List<Ticket> tickets = new List<Ticket>();
            int count = Math.Min(HwThreadCount, range.Length);
            for (int i = 0; i < count; ++i)
            {
                Range subrange = new Range(range.Interpolate(i, count), range.Interpolate(i + 1, count));
                tickets.Add(Schedule(delegate() { function(subrange); }));
            }
            Wait(tickets);
        }

        public static void Split(int count, RangeFunction function)
        {
            Split(new Range(count), function);
        }

        public static void Split(Range range, IterationFunction function)
        {
            Split(range, delegate(Range subrange)
            {
                for (int i = subrange.Begin; i < subrange.End; ++i)
                    function(i);
            });
        }

        public static void Split(int count, IterationFunction function)
        {
            Split(new Range(count), function);
        }

        public static void Split<T>(IList<T> list, ApplyFunction<T> function)
        {
            Split(list.Count, delegate(Range subrange)
            {
                for (int i = subrange.Begin; i < subrange.End; ++i)
                    function(list[i]);
            });
        }

        public static void SplitY(Size size, ApplyFunction<Point> function)
        {
            Split(size.Height, delegate(Range yRange)
            {
                for (int y = yRange.Begin; y < yRange.End; ++y)
                    for (int x = 0; x < size.Width; ++x)
                        function(new Point(x, y));
            });
        }

        public static void Split(Range range, IList<RangeFunction> functions)
        {
            List<Ticket> tickets = new List<Ticket>();
            int threadCount = Math.Min(functions.Count, Math.Min(HwThreadCount, range.Length));
            for (int i = 0; i < threadCount; ++i)
            {
                RangeFunction function = functions[i];
                Range subrange = new Range(range.Interpolate(i, threadCount), range.Interpolate(i + 1, threadCount));
                tickets.Add(Schedule(delegate() { function(subrange); }));
            }
            Wait(tickets);
        }

        sealed class TicketImplementation : Ticket
        {
            Task Task;
            ManualResetEvent Finished = new ManualResetEvent(false);
            Exception TaskException;

            public TicketImplementation(Task task)
            {
                Task = task;
            }

            public void Run()
            {
                try
                {
                    Task();
                }
                catch (Exception e)
                {
                    TaskException = e;
                }
                finally
                {
                    Finished.Set();
                }
            }

            public override void Wait()
            {
                Finished.WaitOne();
                if (TaskException != null)
                    throw new Exception("Threaded task failed", TaskException);
            }
        }

        sealed class Worker
        {
            volatile TicketImplementation Ticket;
            volatile Task OnFinished;
            AutoResetEvent TaskSubmitted = new AutoResetEvent(false);

            public void Submit(TicketImplementation ticket, Task onFinished)
            {
                Ticket = ticket;
                OnFinished = onFinished;
                TaskSubmitted.Set();
            }

            public void Run()
            {
                while (true)
                {
                    TaskSubmitted.WaitOne();
                    Ticket.Run();
                    OnFinished();
                }
            }
        }
    }
}
