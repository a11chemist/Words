using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Words.Interfaces;

namespace Words.Service
{
    /// <summary>
    /// Сетевой сервис словаря 
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WordService: IWordContract, IDisposable
    {
        #region Properties
        /// <summary>
        /// Название сервиса
        /// </summary>
        private string ServiceName { get; }

        /// <summary>
        /// Номер порта
        /// </summary>
        private int Port { get; }

        /// <summary>
        /// Бинарное дерево содержащее уникальные слова и их ранг 
        /// </summary>
        private Word Words { get; set; }

        /// <summary>
        /// Список префиксов слов
        /// </summary>
        public IEnumerable<string> Prefixes { get; private set; }

        private ServiceHost _serviceHost;
        #endregion Properties

        #region Constructors
        /// <summary>
        /// Инициализирует сетевой сервис словаря на всех сетевых интерфейсах по указанному порту
        /// </summary>
        /// <param name="stream">Поток данных словаря</param>
        /// <param name="port">Номер порта</param>
        public WordService(Stream stream, int port)
        {
            ServiceName = nameof(WordService);
            Log.Write($"Создание {ServiceName}");
            LoadWords(stream);
            Log.Write($"Словарь для {ServiceName} загружен");
            Port = port;
        }
        #endregion Constructors
        
        #region Methods
        /// <summary>
        /// Загружает словарь 
        /// </summary>
        /// <param name="basestream">Последовательность байт содержащих словарь</param>
        private void LoadWords(Stream basestream)
        {
            using (var stream = new StreamReader(basestream))
            {
                string line = stream.ReadLine();
                // ReSharper disable once AssignNullToNotNullAttribute
                int wordsCount = int.Parse(line);
                // Чтение уникальных слов с рейтингом
                Words = new Word();
                for (int i = 0; i < wordsCount; i++)
                {
                    line = stream.ReadLine();
                    // ReSharper disable once PossibleNullReferenceException
                    string[] lineSplited = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Words.Add(lineSplited[0], int.Parse(lineSplited[1]));
                }
                // Пользовательский ввод
                line = stream.ReadLine();
                // ReSharper disable once AssignNullToNotNullAttribute
                wordsCount = int.Parse(line);
                var userWords = new string[wordsCount];
                for (int i = 0; i < wordsCount; i++)
                {
                    userWords[i] = stream.ReadLine();
                }
                Prefixes = userWords;
            }
        }

        /// <summary>
        /// Возвращает список наиболее часто употребляемых слов, в порядке убывания частоты
        /// </summary>
        /// <param name="word">Начало слова</param>
        /// <param name="count">Максимальное количество возвращаемых слов</param>
        /// <returns></returns>
        public IEnumerable<Word> GetWords(string word, int count = 10)
        {
            return Words.GetAll(word, count);
        }

        /// <summary>
        /// Запускает wcf сервис
        /// </summary>
        public void RunService()
        {
            Log.Write($"Запуск {ServiceName}");
            var adres = new Uri($"net.tcp://localhost:{Port}");
            _serviceHost = new ServiceHost(this, adres);
            _serviceHost.Open();
            Log.Write($"Сервис {ServiceName} запущен на {adres}");
        }
        #endregion Methods

        #region IWordContract
        /// <summary>
        /// Реализует асинхронный метод контракта службы для получения списка слов
        /// </summary>
        /// <param name="prefix">Префикс слов</param>
        /// <param name="count">Максимальное количество слов в результате</param>
        /// <returns></returns>
        public async Task<string> GetWordsAsync(string prefix, int count)
        {
            return await Task<string>.Factory.StartNew(() =>
            {
                return string.Join("", GetWords(prefix, count).Select(s => s.Letters + "\r\n"));
            }).ConfigureAwait(false);
        }
        #endregion

        #region IDisposable Support
        private bool _disposedValue; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _serviceHost?.Close();
                }
                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
