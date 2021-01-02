using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Legate.Core;
using Legate.Core.Models;
using NSubstitute;
using Xunit;

namespace Legate.Tests.Core
{
    public class ServiceAnnotationMapperFacts
    {
        private static readonly Fixture AutoFixture = new();

        [Fact]
        public void When_arguments_are_valid_then_TryMap_should_return_true()
        {
            var arguments = new Dictionary<string, string>
            {
                { "0", "name" },
                { "1", "22" },
            };
            var parserMock = Substitute.For<IServiceAnnotationArgumentParser>();
            SetupParserToReturn(parserMock, true, arguments);

            var mapper = new ServiceAnnotationMapper(parserMock);

            // Act
            var result = mapper.TryMap(AutoFixture.Create<string>(), AutoFixture.Create<string>(), new List<PodPort>(), out _, out _);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void When_arguments_are_invalid_then_TryMap_should_return_false()
        {
            var parserMock = Substitute.For<IServiceAnnotationArgumentParser>();
            SetupParserToReturn(parserMock, false, new Dictionary<string, string>());

            var mapper = new ServiceAnnotationMapper(parserMock);

            // Act
            var result = mapper.TryMap(AutoFixture.Create<string>(), AutoFixture.Create<string>(), new List<PodPort>(), out _, out _);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void When_port_is_defined_then_TryMap_should_use_provided_port()
        {
            var arguments = new Dictionary<string, string>
            {
                { "0", "name" },
                { "1", "22" },
            };
            var parserMock = Substitute.For<IServiceAnnotationArgumentParser>();
            SetupParserToReturn(parserMock, true, arguments);

            var mapper = new ServiceAnnotationMapper(parserMock);

            // Act
            mapper.TryMap(AutoFixture.Create<string>(), AutoFixture.Create<string>(), new List<PodPort>(), out var result, out _);

            // Assert
            result!.Port.Should().Be(22);
        }

        [Fact]
        public void When_port_is_defined_and_refers_to_a_named_port_then_TryMap_should_use_provided_port()
        {
            var arguments = new Dictionary<string, string>
            {
                { "0", "name" },
                { "1", "ssh" },
            };
            var podPorts = new List<PodPort>
            {
                new(AutoFixture.Create<string>(), "ssh", 22)
            };

            var parserMock = Substitute.For<IServiceAnnotationArgumentParser>();
            SetupParserToReturn(parserMock, true, arguments);

            var mapper = new ServiceAnnotationMapper(parserMock);

            // Act
            mapper.TryMap(AutoFixture.Create<string>(), AutoFixture.Create<string>(), podPorts, out var result, out _);

            // Assert
            result!.Port.Should().Be(22);
        }

        [Fact]
        public void When_port_is_not_defined_and_only_one_container_port_exists_then_TryMap_should_use_container_port()
        {
            var arguments = new Dictionary<string, string>
            {
                { "0", "name" }
            };
            var podPorts = new List<PodPort>
            {
                new(AutoFixture.Create<string>(), "ssh", 22)
            };

            var parserMock = Substitute.For<IServiceAnnotationArgumentParser>();
            SetupParserToReturn(parserMock, true, arguments);

            var mapper = new ServiceAnnotationMapper(parserMock);

            // Act
            mapper.TryMap(AutoFixture.Create<string>(), AutoFixture.Create<string>(), podPorts, out var result, out _);

            // Assert
            result!.Port.Should().Be(22);
        }

        [Fact]
        public void When_port_is_not_defined_and_multiple_container_port_exist_then_TryMap_should_fail()
        {
            var arguments = new Dictionary<string, string>
            {
                { "0", "name" }
            };
            var podPorts = new List<PodPort>
            {
                new(AutoFixture.Create<string>(), "ssh", 22),
                new(AutoFixture.Create<string>(), "ftp", 21),
            };

            var parserMock = Substitute.For<IServiceAnnotationArgumentParser>();
            SetupParserToReturn(parserMock, true, arguments);

            var mapper = new ServiceAnnotationMapper(parserMock);

            // Act
            var result = mapper.TryMap(AutoFixture.Create<string>(), AutoFixture.Create<string>(), podPorts, out _, out _);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void When_port_is_not_defined_and_no_container_ports_exist_then_TryMap_should_fail()
        {
            var arguments = new Dictionary<string, string>
            {
                { "0", "name" }
            };

            var parserMock = Substitute.For<IServiceAnnotationArgumentParser>();
            SetupParserToReturn(parserMock, true, arguments);

            var mapper = new ServiceAnnotationMapper(parserMock);

            // Act
            var result = mapper.TryMap(AutoFixture.Create<string>(), AutoFixture.Create<string>(), new List<PodPort>(), out _, out _);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void When_port_is_defined_and_and_refers_to_a_missing_container_port_then_TryMap_should_fail()
        {
            var arguments = new Dictionary<string, string>
            {
                { "0", "name" },
                { "1", "ssh" },
            };

            var parserMock = Substitute.For<IServiceAnnotationArgumentParser>();
            SetupParserToReturn(parserMock, true, arguments);

            var mapper = new ServiceAnnotationMapper(parserMock);

            // Act
            var result = mapper.TryMap(AutoFixture.Create<string>(), AutoFixture.Create<string>(), new List<PodPort>(), out _, out _);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void When_service_name_not_defined_then_TryMap_should_set_name()
        {
            var arguments = new Dictionary<string, string>
            {
                { "name", "name" },
                { "port", "22" }
            };

            var parserMock = Substitute.For<IServiceAnnotationArgumentParser>();
            SetupParserToReturn(parserMock, true, arguments);

            var mapper = new ServiceAnnotationMapper(parserMock);

            // Act
            mapper.TryMap(AutoFixture.Create<string>(), AutoFixture.Create<string>(), new List<PodPort>(), out var result, out _);

            // Assert
            result!.ServiceName.Should().Be("name");
        }

        [Fact]
        public void When_service_name_is_not_defined_then_TryMap_should_fail()
        {
            var arguments = new Dictionary<string, string>
            {
                { "tags", "tags" }
            };

            var parserMock = Substitute.For<IServiceAnnotationArgumentParser>();
            SetupParserToReturn(parserMock, true, arguments);

            var mapper = new ServiceAnnotationMapper(parserMock);

            // Act
            var result = mapper.TryMap(AutoFixture.Create<string>(), AutoFixture.Create<string>(), new List<PodPort>(), out _, out _);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void When_tags_are_defined_then_TryMap_should_set_tags()
        {
            var arguments = new Dictionary<string, string>
            {
                { "name", "name" },
                { "port", "22" },
                { "tags", "one,two,three" },
            };

            var parserMock = Substitute.For<IServiceAnnotationArgumentParser>();
            SetupParserToReturn(parserMock, true, arguments);

            var mapper = new ServiceAnnotationMapper(parserMock);

            // Act
            mapper.TryMap(AutoFixture.Create<string>(), AutoFixture.Create<string>(), new List<PodPort>(), out var result, out _);

            // Assert
            result!.Tags.Should().Contain(new List<string>
            {
                "one",
                "two",
                "three"
            });
        }

        [Fact]
        public void When_tags_contain_whitespace_then_TryMap_should_trim_tags()
        {
            var arguments = new Dictionary<string, string>
            {
                { "name", "name" },
                { "port", "22" },
                { "tags", "one , two , three " },
            };

            var parserMock = Substitute.For<IServiceAnnotationArgumentParser>();
            SetupParserToReturn(parserMock, true, arguments);

            var mapper = new ServiceAnnotationMapper(parserMock);

            // Act
            mapper.TryMap(AutoFixture.Create<string>(), AutoFixture.Create<string>(), new List<PodPort>(), out var result, out _);

            // Assert
            result!.Tags.Should().Contain(new List<string>
            {
                "one",
                "two",
                "three"
            });
        }

        private static void SetupParserToReturn(IServiceAnnotationArgumentParser parserMock,
                                                bool result,
                                                IReadOnlyDictionary<string, string> arguments)
        {
            var temp = new ParseErrors();
            parserMock.TryParse(null!, out _, ref temp).ReturnsForAnyArgs(info =>
            {
                info[1] = arguments;
                return result;
            });
        }
    }
}