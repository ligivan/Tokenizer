using System;

namespace Tokenizer.Extensions
{
    public static class TokenExtensions
    {
        public static string GetStringValue(this Token token)
        {
            return string.Join("", token.Content);
        }

        public static decimal GetNumberValue(this Token token)
        {
            if(token.Kind != TokenKind.Number)
            {
                throw new InvalidOperationException("token kind is not a number");
            }

            return decimal.Parse(GetStringValue(token));
        }
    }
}
