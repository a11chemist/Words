using System;
using System.ServiceModel;
using Words.Interfaces;

namespace Words.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string server = args.Length > 0 ? args[0] : "localhost";
                int port = args.Length > 1 ? int.Parse(args[1]) : 8888;
                #region Wcf Channel
                var factory = new ChannelFactory<IWordContract>(new NetTcpBinding()
                    , new EndpointAddress($"net.tcp://{server}:{port}"));
                IWordContract client = factory.CreateChannel();
                #endregion Wcf Channel
                while (true)
                {
                    try
                    {
                        string line = Console.ReadLine();
                        Console.WriteLine(client.GetWordsAsync(line, 10).GetAwaiter().GetResult());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
                Console.ReadLine();
            }
        }
    }
}
