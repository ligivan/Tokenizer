using System;
using System.Threading.Tasks;

namespace Tokenizer
{
    public interface ITokenizer : IAsyncDisposable
    {
        Task<Token> GetNextTokenAsync();
    }
}
