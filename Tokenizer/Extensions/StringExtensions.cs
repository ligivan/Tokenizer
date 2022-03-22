using Tokenizer.Implementations;

namespace Tokenizer.Extensions
{
    static class StringExtensions
    {
        public static IPositionAwareAsyncCharEnumerator GetAsyncEnumerator(this string input)
        {
            return new StringAsyncEnumerator(input);
        }
    }
}
