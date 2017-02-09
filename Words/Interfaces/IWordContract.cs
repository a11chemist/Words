using System.ServiceModel;
using System.Threading.Tasks;

namespace Words.Interfaces
{
    /// <summary>
    /// Интерфейс контракта вебслужбы слов
    /// </summary>
    [ServiceContract]
    public interface IWordContract
    {
        [OperationContract]
        Task<string> GetWordsAsync(string prefix, int count);
    }
}
