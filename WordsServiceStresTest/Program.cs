using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Words;
using Words.Interfaces;
using Words.Service;

namespace WordsServiceStresTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 8888;
            Task<int>[] tasks = new Task<int>[Environment.ProcessorCount * 2]; // Количество потоков
            var service = new WordService(new FileStream("test.in", FileMode.Open), port);
            service.RunService();
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = new Task<int>(() =>
                {
                    var timer = new Stopwatch();
                    int count = 0;
                    try
                    {
                        var factory = new ChannelFactory<IWordContract>(new NetTcpBinding()
                            , new EndpointAddress($"net.tcp://localhost:{port}"));
                        IWordContract client = factory.CreateChannel();
                        timer.Start();
                        foreach (var prefix in service.Prefixes)
                        {
                            client.GetWordsAsync(prefix, 10).GetAwaiter().GetResult();
                            count++;
                            if (count % 1000 == 0)
                                Log.Write($"Поток {Thread.CurrentThread.ManagedThreadId}: {count / timer.Elapsed.TotalSeconds} запросов/сек");
                        }
                        timer.Stop();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteError($"{Thread.CurrentThread.ManagedThreadId}: {ex.Message}\r\n{ex.StackTrace}");
                    }
                    return (int)(count / timer.Elapsed.TotalSeconds);
                });
                tasks[i].Start();
                Thread.Sleep(500);
            }
            Task.WhenAll(tasks).Wait(Timeout.Infinite);
            Log.Write($"Всего запросов/сек: {tasks.Select(s => s.Result).Sum()}");
            Console.ReadLine();
        }
    }
}
