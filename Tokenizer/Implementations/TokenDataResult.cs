namespace Tokenizer.Implementations
{
    class TokenDataResult
    {
        public bool IsSuccess { get; }

        public char[] Data { get; }

        public TokenDataResult(bool isSuccess, params char[] data)
        {
            IsSuccess = isSuccess;
            Data = data;       
        }
    }
}
