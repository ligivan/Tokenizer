using System.IO;

namespace Tokenizer
{
    public interface ITokenizerFactory
    {
        ITokenizer CreateTokenizerFromStream(Stream input, bool skipSpaces = false);

        ITokenizer CreateTokenizerFromString(string input, bool skipSpaces = false);
    }
}
