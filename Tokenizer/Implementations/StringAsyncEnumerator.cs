using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tokenizer.Implementations
{
    class StringAsyncEnumerator : PositionAwareAsyncCharEnumeratorBase
    {
        private readonly IEnumerator<char> _input;

        public StringAsyncEnumerator(string inputString)
        {
            _input = inputString.GetEnumerator();
        }

        public override char Current { get => _input.Current; }

        public override ValueTask DisposeAsync()
        {
            _input.Dispose();
            return ValueTask.CompletedTask;
        }

        protected override ValueTask<bool> MoveNextInternalAsync()
        {
            var result = _input.MoveNext();
            return ValueTask.FromResult(result);
        }
    }
}
