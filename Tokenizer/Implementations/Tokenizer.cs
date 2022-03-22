using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tokenizer.Implementations
{
    class Tokenizer : ITokenizer
    {
        private readonly IPositionAwareAsyncCharEnumerator _input;
        private static readonly HashSet<char> _symbolChars = new HashSet<char> {
            '=', '+', '-', '/', ',', '.', '*', '~', '!', '@', '#',
            '$', '%', '^', '&', '(', ')', '{', '}', '[', ']', ':',
            ';', '<', '>', '?', '|', '\\'};

        private static readonly HashSet<char> _quoteChars = new HashSet<char> {
            '"', '\'', '`', '„', '“', '«', '»'
        };

        private readonly (TokenKind TokenKind, Func<char, Task<TokenDataResult>> Provider)[] _tokenDataProviders;

        private readonly ILogger _logger;

        private char? _lastChar = null;

        private bool SkipSpaces { get; }

        public Tokenizer(
            IPositionAwareAsyncCharEnumerator input,
            ILogger logger,
            bool skipSpaces)
        {
            _input = input;
            _logger = logger;
            SkipSpaces = skipSpaces;

            _tokenDataProviders = new (TokenKind TokenKind, Func<char, Task<TokenDataResult>> Provider)[]
            {
                ( TokenKind.WhiteSpace, TryGetWhiteSpaceTokenData ),
                ( TokenKind.EOL, TryGetNewLineTokenData ),
                ( TokenKind.Lexem, TryGetLexemTokenData ),
                ( TokenKind.Number, TryGetNumberTokenData ),
                ( TokenKind.Quote, TryGetQuoteTokenData ),
                ( TokenKind.Symbol, TryGetSymbolTokenData ),
                ( TokenKind.Unknown, TryGetDefaultTokenData )
            };
        }

        public async Task<Token> GetNextTokenAsync()
        {
            do
            {
                var next = await ReadCharAsync();
                var lineNumber = _input.LineNumber;
                var columnNumber = _input.ColumnNumber;

                if (next == null)
                {
                    return CreateToken(lineNumber, columnNumber + 1, TokenKind.EOF);
                }

                foreach (var provider in _tokenDataProviders)
                {
                    var tokenResult = await provider.Provider(next.Value);
                    if (tokenResult.IsSuccess)
                    {
                        if(provider.TokenKind == TokenKind.WhiteSpace && SkipSpaces)
                        {
                            break;
                        }

                        return CreateToken(lineNumber, columnNumber, provider.TokenKind, tokenResult.Data);
                    }
                }
            }
            while (true);
        }
 
        private async Task<TokenDataResult> TryGetNewLineTokenData(char symbol)
        {
            var tokenChars = new List<char>();
            char? nextSymbol = symbol;
            if (symbol == '\r')
            {
                nextSymbol = await ReadCharAsync();
                if (nextSymbol != '\n')
                {
                    BackChar();
                    return new TokenDataResult(false);
                }

                tokenChars.Add(symbol);
            }

            if (nextSymbol == '\n')
            {
                tokenChars.Add(nextSymbol.Value);
                return new TokenDataResult(true, tokenChars.ToArray());
            }

            return new TokenDataResult(false);
        }

        private Task<TokenDataResult> TryGetWhiteSpaceTokenData(char symbol)
        {
            TokenDataResult result;

            if (symbol == ' ' || symbol == '\t')
            {
                result = new TokenDataResult(true, symbol);
            }
            else
            {
                result = new TokenDataResult(false);
            }

            return Task.FromResult(result);
        }

        private async Task<TokenDataResult> TryGetNumberTokenData(char symbol)
        {
            var result = new List<char>();
            var hasDot = false;

            if (!char.IsDigit(symbol))
            {
                return new TokenDataResult(false);
            }

            char? nextSymbol = symbol;
            do
            {
                result.Add(nextSymbol.Value);

                nextSymbol = await ReadCharAsync();
                if(nextSymbol == null)
                {
                    break;
                }

                if (!char.IsDigit(nextSymbol.Value))
                {
                    if (!hasDot && nextSymbol == '.')
                    {
                        hasDot = true;
                    }
                    else
                    {
                        BackChar();
                        break;
                    }
                }
            }
            while (true);

            return new TokenDataResult(result.Count > 0, result.ToArray());
        }

        private Task<TokenDataResult> TryGetQuoteTokenData(char symbol) =>
            Task.FromResult(_quoteChars.Contains(symbol) ?
                new TokenDataResult(true, symbol) : new TokenDataResult(false));

        private Task<TokenDataResult> TryGetSymbolTokenData(char symbol) =>
            Task.FromResult(_symbolChars.Contains(symbol) ?
                new TokenDataResult(true, symbol) : new TokenDataResult(false));

        private async Task<TokenDataResult> TryGetLexemTokenData(char symbol)
        {
            var result = new List<char>();
            char? nextSymbol = symbol;

            if(!char.IsLetter(nextSymbol.Value) && symbol != '_')
            {
                return new TokenDataResult(false);
            }

            do
            {
                result.Add(nextSymbol.Value);

                nextSymbol = await ReadCharAsync();
                if (nextSymbol == null)
                {
                    break;
                }

                if (!char.IsLetter(nextSymbol.Value) && !char.IsDigit(nextSymbol.Value))
                {
                    BackChar();
                    break;
                }
            }
            while (true);

            return new TokenDataResult(true, result.ToArray());
        }

        private Task<TokenDataResult> TryGetDefaultTokenData(char symbol) => 
            Task.FromResult(new TokenDataResult(true, symbol));

        private Token CreateToken(int lineNumber, int columnNumber, TokenKind tokenKind, params char[] content)
        {
            var token = new Token(lineNumber, columnNumber, tokenKind, content);

            _logger.Trace($"{token}");

            return token;
        }

        private void BackChar()
        {
            _lastChar = _input.Current;
        }

        private async Task<char?> ReadCharAsync()
        {
            if(_lastChar != null)
            {
                var result = _lastChar;
                _lastChar = null;
                return result;
            }

            var isSuccess = await _input.MoveNextAsync();
            if (!isSuccess)
            {
                return null;
            }

            return _input.Current;
        }

        public ValueTask DisposeAsync() => _input.DisposeAsync();
    }
}
