using System.IO;
using Tokenizer.Implementations;

namespace Tokenizer.Extensions
{
    public static class StreamExtensions
    {
        public static IPositionAwareAsyncCharEnumerator GetAsyncEnumerator(this Stream input)
        {
            return new StreamAsyncEnumerator(input);
        }
    }
}
