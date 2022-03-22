using System.Collections.Generic;

namespace Tokenizer
{
    public interface IPositionAwareAsyncCharEnumerator : IAsyncEnumerator<char>
    {
        int LineNumber { get; }

        int ColumnNumber { get; }
    }
}
