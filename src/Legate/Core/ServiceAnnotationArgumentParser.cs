using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Legate.Core.Models;

namespace Legate.Core
{
    public interface IServiceAnnotationArgumentParser
    {
        bool TryParse(string annotation, [NotNullWhen(true)] out IReadOnlyDictionary<string, string>? arguments, ref ParseErrors parseErrors);
    }

    public class ServiceAnnotationArgumentParser : IServiceAnnotationArgumentParser
    {
        public bool TryParse(string annotation, [NotNullWhen(true)] out IReadOnlyDictionary<string, string>? arguments, ref ParseErrors parseErrors)
        {
            try
            {
                var tokens = GetTokens(annotation);
                arguments = BuildArguments(tokens).ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                parseErrors.Add(e.Message);
                arguments = default;
            }

            return arguments != null;
        }

        private static IEnumerable<Token> GetTokens(IEnumerable<char> input)
        {
            var inQuote = false;

            foreach (var c in input)
            {
                if (c == '"'
                    || c == '\'')
                {
                    if (!inQuote)
                    {
                        yield return new Token(TokenType.QuoteStart, c);
                        inQuote = true;
                    }
                    else
                    {
                        yield return new Token(TokenType.QuoteEnd, c);
                        inQuote = false;
                    }
                }
                else if (!inQuote
                         && (c == ';' || c == ','))
                {
                    yield return new Token(TokenType.Separator, c);
                }
                else if (!inQuote
                         && (c == '='))
                {
                    yield return new Token(TokenType.Equals, c);
                }
                else if (!inQuote
                         && (c == ' '
                             || c == '\t'
                             || c == '\n'
                             || c == '\n'))
                {
                    yield return new Token(TokenType.Trivia, c);
                }
                else
                {
                    yield return new Token(TokenType.Literal, c);
                }
            }

            yield return new Token(TokenType.Eol, '\0');
        }

        private static IEnumerable<Argument> BuildArguments(IEnumerable<Token> tokens)
        {
            var quotes = 0;
            var builder = new StringBuilder();
            var position = 0;

            string? argumentName = default;
            foreach (var token in tokens)
            {
                switch (token.Type)
                {
                    case TokenType.QuoteStart:
                    case TokenType.QuoteEnd:
                        quotes++;
                        break;
                    case TokenType.Literal:
                        builder.Append(token.Value);
                        break;
                    case TokenType.Separator:
                    case TokenType.Eol:
                    {
                        yield return argumentName != null
                            ? new Argument(position, argumentName, builder.ToString())
                            : new Argument(position, position.ToString(), builder.ToString());
                        argumentName = default;
                        position++;
                        builder.Clear();
                        break;
                    }
                    case TokenType.Equals:
                        argumentName = builder.ToString();
                        builder.Clear();
                        break;
                }
            }

            if (quotes % 2 != 0)
            {
                throw new Exception("Unbalanced quotes detected, tokenization is now ambiguous and will fail.");
            }
        }

        private enum TokenType
        {
            [UsedImplicitly]
            Unknown = 0,
            QuoteStart,
            QuoteEnd,
            Literal,
            Separator,
            Equals,
            Trivia,
            Eol
        }

        private record Token(TokenType Type, char Value);

        private record Argument(int Position, string Name, string Value);
    }
}