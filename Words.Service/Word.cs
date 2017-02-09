using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Words.Service
{
    /// <summary>
    /// Описывает слово в словаре, и является потокобезопасным бинарным деревом поска
    /// </summary>
    public class Word :ConcurrentDictionary<char, Word>, IComparable<Word>
    {
        #region Properties
        /// <summary>
        /// Слово
        /// </summary>
        public string Letters { get; private set; }

        /// <summary>
        /// Количество повторений в текстах
        /// </summary>
        public int Rank { get; private set; }
        #endregion Properties

        #region Constructors
        /// <summary>
        /// Создаёт пустое слово 
        /// </summary>
        public Word()
            : base(Environment.ProcessorCount * 2, 15)
        {
            Letters = string.Empty;
            Rank = 0;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Добавляет слово в коллекцию
        /// </summary>
        /// <param name="letters"></param>
        /// <param name="rank"></param>
        public void Add(string letters, int rank)
        {
            Word word = this;
            foreach (char ch in letters)
            {
                if (!word.ContainsKey(ch))
                    word.AddOrUpdate(ch, new Word(), (v, k) => new Word());
                word = word[ch];
            }
            word.Letters = letters;
            word.Rank = rank;
        }

        /// <summary>
        /// Возвращает узел дерева связанный со словом или null если узла не существует
        /// </summary>
        /// <param name="letters">Искомое слово</param>
        /// <returns></returns>
        public Word Get(string letters)
        {
            Word word = this;
            foreach (char ch in letters)
            {
                if (word.ContainsKey(ch))
                    word = word[ch];
                else return null;
            }
            return word;
        }

        /// <summary>
        /// Возвращает перечисление всех слов начинающиеся с word и отсортированных по убыванию ранга и алфавиту
        /// </summary>
        /// <param name="letters">Начало искомых слов</param>
        /// <param name="count">Количество результатов</param>
        /// <returns></returns>
        public IEnumerable<Word> GetAll(string letters, int count)
        {
            if (string.IsNullOrEmpty(letters)) return new Word[] { };

            Word word = Get(letters);
            if (word == null) return new Word[] { };

            var result = new LinkedList<Word>(Enumerable.Repeat(new Word(), count));
            //for (int i = 0; i < count; i++)
            //    result.AddLast(new Word());

            GetAll(word, ref result);
            return result.TakeWhile(t => t.Rank > 0);
        }

        /// <summary>
        /// Служебная рекурсивная функция поиска слов по дереву
        /// </summary>
        /// <param name="word">Начало искомых слов</param>
        /// <param name="result">Ссылка на список с результатами поиска</param>
        private void GetAll(Word word, ref LinkedList<Word> result)
        {
            AddResult(word, ref result);
            foreach (var w in word)
            {
                AddResult(w.Value, ref result);
                foreach (var child in w.Value)
                    GetAll(child.Value, ref result);
            }
        }

        /// <summary>
        /// Служебная функция. Добавляет слово в отсортированный список результатов если оно попадает в диапазон
        /// </summary>
        /// <param name="word">Слово</param>
        /// <param name="list">Ссылка на список с результатами поиска</param>
        private void AddResult(Word word, ref LinkedList<Word> list)
        {
            for (var n = list.First; n != null; n = n.Next)
            {
                if (word.CompareTo(n.Value) > 0)
                {
                    list.AddBefore(n, word);
                    list.RemoveLast();
                    break;
                }
            }
        }

        /// <summary>
        /// Функция сравнения слов
        /// </summary>
        /// <param name="other">Сравниваемое слово</param>
        /// <returns></returns>
        public int CompareTo(Word other)
        {
            int result = Rank - other.Rank;
            return result == 0 ? string.CompareOrdinal(other.Letters, Letters) : result;
        }
        #endregion Methods
    }
}