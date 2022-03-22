namespace Tokenizer
{
    public class Token
    {
        public int LineNumber { get; }

        public int CharNumber { get; }

        public char[] Content { get; }

        public TokenKind Kind { get; }

        public Token(int lineNumber, int charNumber, TokenKind kind, char[] content)
        {
            LineNumber = lineNumber;
            CharNumber = charNumber;
            Content = content;
            Kind = kind;
        }

        public override string ToString()
        {
            return $"Token {string.Join("", Content)} Kind: {Kind} at Line: {LineNumber}; Column: {CharNumber}";
        }
    }
}
