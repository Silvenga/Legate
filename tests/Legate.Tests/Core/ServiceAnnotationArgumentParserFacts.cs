using FluentAssertions;
using FluentAssertions.Execution;
using Legate.Core;
using Legate.Core.Models;
using Xunit;

namespace Legate.Tests.Core
{
    public class ServiceAnnotationArgumentParserFacts
    {
        [Fact]
        public void When_parsing_a_valid_string_then_TryParse_should_return_true()
        {
            var parser = new ServiceAnnotationArgumentParser();
            var parseErrors = new ParseErrors();

            // Act
            var result = parser.TryParse(" service-name ;22;tags='one= two'", out _, ref parseErrors);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void When_parsing_a_invalid_string_then_TryParse_should_return_true()
        {
            var parser = new ServiceAnnotationArgumentParser();
            var parseErrors = new ParseErrors();

            // Act
            var result = parser.TryParse("'service-name", out _, ref parseErrors);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void When_given_a_positional_argument_then_TryParse_should_return_argument()
        {
            var parser = new ServiceAnnotationArgumentParser();
            var parseErrors = new ParseErrors();

            // Act
            parser.TryParse("service-name", out var result, ref parseErrors);

            // Assert
            result!["0"].Should().Be("service-name");
        }

        [Fact]
        public void When_given_a_named_argument_then_TryParse_should_return_argument()
        {
            var parser = new ServiceAnnotationArgumentParser();
            var parseErrors = new ParseErrors();

            // Act
            parser.TryParse("named=service-name", out var result, ref parseErrors);

            // Assert
            result!["named"].Should().Be("service-name");
        }

        [Fact]
        public void When_given_multiple_arguments_then_TryParse_should_return_arguments()
        {
            var parser = new ServiceAnnotationArgumentParser();
            var parseErrors = new ParseErrors();

            // Act
            parser.TryParse("service-name;named=one", out var result, ref parseErrors);

            // Assert
            using (new AssertionScope())
            {
                result!["0"].Should().Be("service-name");
                result!["named"].Should().Be("one");
            }
        }

        [Fact]
        public void When_given_arguments_with_whitespace_then_returned_arguments_should_be_stripped()
        {
            var parser = new ServiceAnnotationArgumentParser();
            var parseErrors = new ParseErrors();

            // Act
            parser.TryParse(" service-name ; named = one ", out var result, ref parseErrors);

            // Assert
            using (new AssertionScope())
            {
                result!["0"].Should().Be("service-name");
                result!["named"].Should().Be("one");
            }
        }

        [Fact]
        public void When_parsing_string_with_out_closing_quotes_then_TryParse_should_return_error()
        {
            var parser = new ServiceAnnotationArgumentParser();
            var parseErrors = new ParseErrors();

            // Act
            parser.TryParse("'service-name", out _, ref parseErrors);

            // Assert
            parseErrors.GetErrorMessages().Should().ContainSingle()
                       .Which.Should().Be("Unbalanced quotes detected, tokenization is now ambiguous and will fail.");
        }
    }
}