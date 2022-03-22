using System.IO;
using Tokenizer.Implementations;
using Tokenizer.Extensions;
using NLog;

namespace Tokenizer
{
    public class TokenizerFactory : ITokenizerFactory
    {
        private readonly ILogger _logger;

        public TokenizerFactory(ILogger logger)
        {
            _logger = logger;
        }

        public ITokenizer CreateTokenizerFromStream(Stream input, bool skipSpaces = false) =>
            new Implementations.Tokenizer(input.GetAsyncEnumerator(), _logger, skipSpaces);

        public ITokenizer CreateTokenizerFromString(string input, bool skipSpaces = false) => 
            new Implementations.Tokenizer(input.GetAsyncEnumerator(), _logger, skipSpaces);
    }
}
