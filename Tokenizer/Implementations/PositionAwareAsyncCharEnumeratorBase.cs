using System.Threading.Tasks;

namespace Tokenizer.Implementations
{
    abstract class PositionAwareAsyncCharEnumeratorBase : IPositionAwareAsyncCharEnumerator
    {
        private bool _needToChangeLine = false;

        public int LineNumber { get; private set; } = 1;

        public int ColumnNumber { get; private set; } = 0;

        public abstract char Current { get; }

        public abstract ValueTask DisposeAsync();

        public async ValueTask<bool> MoveNextAsync()
        {
            var result = await MoveNextInternalAsync();

            if (result)
            {
                if (_needToChangeLine)
                {
                    LineNumber++;
                    ColumnNumber = 1;

                    _needToChangeLine = false;
                }
                else
                {
                    ColumnNumber++;
                }

                if (Current == '\n')
                {
                    _needToChangeLine = true;
                }
            }

            return result;
        }

        protected abstract ValueTask<bool> MoveNextInternalAsync();
    }
}
