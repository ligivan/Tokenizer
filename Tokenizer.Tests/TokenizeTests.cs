using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Tokenizer.Extensions;

namespace Tokenizer.Tests
{
    [TestClass]
    public class TokenizeTests
    {
        private TokenizerFactory _tokenizerFactory;

        [TestInitialize]
        public void Initialize()
        {
            var logger = LogManager.GetCurrentClassLogger();
            _tokenizerFactory = new TokenizerFactory(logger);
        }

        [TestMethod]
        public async Task TokenizeString()
        {
            var testString = "abc 123 _bb45;\t\r\n123.456\"";
            await using (var tokenizer = _tokenizerFactory.CreateTokenizerFromString(testString))
            {
                await GetAndCheckTokensAsync(tokenizer,
                    CreateTokenForTest(1, 1, TokenKind.Lexem, "abc"),
                    CreateTokenForTest(1, 4, TokenKind.WhiteSpace, " "),
                    CreateTokenForTest(1, 5, TokenKind.Number, 123),
                    CreateTokenForTest(1, 8, TokenKind.WhiteSpace, " "),
                    CreateTokenForTest(1, 9, TokenKind.Lexem, "_bb45"),
                    CreateTokenForTest(1, 14, TokenKind.Symbol, ";"),
                    CreateTokenForTest(1, 15, TokenKind.WhiteSpace, "\t"),
                    CreateTokenForTest(1, 16, TokenKind.EOL, "\r\n"),
                    CreateTokenForTest(2, 1, TokenKind.Number, 123.456),
                    CreateTokenForTest(2, 8, TokenKind.Quote, "\""),
                    CreateTokenForTest(2, 9, TokenKind.EOF, string.Empty));
            }
        }

        [TestMethod]
        public async Task TokenizeStringWithSkipSpaces()
        {
            var testString = "abc 123 _bb45;\t\r\n123.456\"";
            await using (var tokenizer = _tokenizerFactory.CreateTokenizerFromString(testString, true))
            {
                await GetAndCheckTokensAsync(tokenizer,
                    CreateTokenForTest(1, 1, TokenKind.Lexem, "abc"),
                    CreateTokenForTest(1, 5, TokenKind.Number, 123),
                    CreateTokenForTest(1, 9, TokenKind.Lexem, "_bb45"),
                    CreateTokenForTest(1, 14, TokenKind.Symbol, ";"),
                    CreateTokenForTest(1, 16, TokenKind.EOL, "\r\n"),
                    CreateTokenForTest(2, 1, TokenKind.Number, 123.456),
                    CreateTokenForTest(2, 8, TokenKind.Quote, "\""),
                    CreateTokenForTest(2, 9, TokenKind.EOF, string.Empty));
            }
        }

        [TestMethod]
        public async Task TokenizeStream()
        {
            var testString = "abc 123 _bb45;\t\r\n123.456\"";
            using (var memoryStream = new MemoryStream(Encoding.Default.GetBytes(testString)))
            {
                memoryStream.Position = 0;
                await using (var tokenizer = _tokenizerFactory.CreateTokenizerFromStream(memoryStream))
                {
                    await GetAndCheckTokensAsync(tokenizer,
                        CreateTokenForTest(1, 1, TokenKind.Lexem, "abc"),
                        CreateTokenForTest(1, 4, TokenKind.WhiteSpace, " "),
                        CreateTokenForTest(1, 5, TokenKind.Number, 123),
                        CreateTokenForTest(1, 8, TokenKind.WhiteSpace, " "),
                        CreateTokenForTest(1, 9, TokenKind.Lexem, "_bb45"),
                        CreateTokenForTest(1, 14, TokenKind.Symbol, ";"),
                        CreateTokenForTest(1, 15, TokenKind.WhiteSpace, "\t"),
                        CreateTokenForTest(1, 16, TokenKind.EOL, "\r\n"),
                        CreateTokenForTest(2, 1, TokenKind.Number, 123.456),
                        CreateTokenForTest(2, 8, TokenKind.Quote, "\""),
                        CreateTokenForTest(2, 9, TokenKind.EOF, string.Empty));
                }
            }
        }

        [TestMethod]
        public async Task TokenizeStreamWithSkipSpaces()
        {
            var testString = "abc 123 _bb45;\t\r\n123.456\"";
            using (var memoryStream = new MemoryStream(Encoding.Default.GetBytes(testString)))
            {
                memoryStream.Position = 0;
                await using (var tokenizer = _tokenizerFactory.CreateTokenizerFromStream(memoryStream, true))
                {
                    await GetAndCheckTokensAsync(tokenizer,
                        CreateTokenForTest(1, 1, TokenKind.Lexem, "abc"),
                        CreateTokenForTest(1, 5, TokenKind.Number, 123),
                        CreateTokenForTest(1, 9, TokenKind.Lexem, "_bb45"),
                        CreateTokenForTest(1, 14, TokenKind.Symbol, ";"),
                        CreateTokenForTest(1, 16, TokenKind.EOL, "\r\n"),
                        CreateTokenForTest(2, 1, TokenKind.Number, 123.456),
                        CreateTokenForTest(2, 8, TokenKind.Quote, "\""),
                        CreateTokenForTest(2, 9, TokenKind.EOF, string.Empty));
                }
            }
        }

        [TestMethod]
        public async Task TokenizeEmptyString()
        {
            var testString = "";
            await using (var tokenizer = _tokenizerFactory.CreateTokenizerFromString(testString))
            {
                await GetAndCheckTokensAsync(tokenizer, CreateTokenForTest(1, 1, TokenKind.EOF, string.Empty));
            }
        }

        [TestMethod]
        public async Task TokenizeEmptyStream()
        {
            var testString = "";
            using (var memoryStream = new MemoryStream(Encoding.Default.GetBytes(testString)))
            {
                memoryStream.Position = 0;
                await using (var tokenizer = _tokenizerFactory.CreateTokenizerFromStream(memoryStream))
                {
                    await GetAndCheckTokensAsync(tokenizer, CreateTokenForTest(1, 1, TokenKind.EOF, string.Empty));
                }
            }
        }

        private async Task GetAndCheckTokensAsync(ITokenizer tokenizer, params Token[] testTokens)
        {
            foreach(var testToken in testTokens)
            {
                var nextToken = await tokenizer.GetNextTokenAsync();
                Assert.AreEqual(testToken.Kind, nextToken.Kind);

                if(testToken.Kind == TokenKind.Number)
                {
                    Assert.AreEqual(testToken.GetNumberValue(), nextToken.GetNumberValue());
                } 
                else
                {
                    Assert.AreEqual(testToken.GetStringValue(), nextToken.GetStringValue());
                }
                
                Assert.AreEqual(testToken.CharNumber, nextToken.CharNumber);
                Assert.AreEqual(testToken.LineNumber, nextToken.LineNumber);
            }
        }

        private static Token CreateTokenForTest(int lineNumber, int charNumber, TokenKind kind, string content) => 
            new Token(lineNumber, charNumber, kind, content.ToCharArray());

        private static Token CreateTokenForTest(int lineNumber, int charNumber, TokenKind kind, double content) =>
           CreateTokenForTest(lineNumber, charNumber, kind, content.ToString());
    }
}
