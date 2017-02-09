using System;
using System.IO;

namespace Words.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Stream stream = args.Length > 0 ? new FileStream(args[0], FileMode.Open) : Console.OpenStandardInput();
                // Порт по умолчанию
                int port = args.Length > 1 ? int.Parse(args[1]) : 8888;

                var wordService = new WordService(stream, port);

                wordService.RunService();

                Console.WriteLine("Для остановки сервиса нажмите <Enter>");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Log.WriteError($"{ex.Message}\r\n{ex.StackTrace}");
#if DEBUG
                Console.ReadLine();
#endif
            }
        }
    }
}
