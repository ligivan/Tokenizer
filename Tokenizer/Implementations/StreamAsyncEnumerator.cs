using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tokenizer.Implementations
{
    class StreamAsyncEnumerator : PositionAwareAsyncCharEnumeratorBase
    {
        private readonly StreamReader _input;
        private Queue<char> _charPortion = new Queue<char>();
        private const int BufferSize = 1024;
        private char _current;

        public StreamAsyncEnumerator(Stream input)
        {
            _input = new StreamReader(input);
        }

        public override char Current { get => _current; }

        public override ValueTask DisposeAsync()
        {
            _input.Dispose();

            return ValueTask.CompletedTask;
        }

        protected async override ValueTask<bool> MoveNextInternalAsync()
        {
            char result;
            if(_charPortion.Count == 0)
            {
                _charPortion = await ReadNextCharsPortion();
            }

            if (_charPortion.TryDequeue(out result))
            {
                _current = result;
                return true;
            }

            return false;
        }

        private async Task<Queue<char>> ReadNextCharsPortion()
        {
            var buffer = new char[BufferSize];

            var bytesRead = await _input.ReadAsync(buffer, 0, BufferSize);
            var resultArray = new char[bytesRead];
            Array.Copy(buffer, resultArray, bytesRead);
            return new Queue<char>(resultArray);
        }
    }
}
