using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Legate.Core.Models;

namespace Legate.Core
{
    public interface IServiceAnnotationMapper
    {
        bool TryMap(string annotationKey,
                    string annotationValue,
                    IReadOnlyCollection<PodPort> podPorts,
                    [NotNullWhen(true)] out PodServiceAnnotation? serviceAnnotation,
                    out ParseErrors parseErrors);
    }

    public class ServiceAnnotationMapper : IServiceAnnotationMapper
    {
        private readonly IServiceAnnotationArgumentParser _annotationArgumentParser;

        public ServiceAnnotationMapper(IServiceAnnotationArgumentParser annotationArgumentParser)
        {
            _annotationArgumentParser = annotationArgumentParser;
        }

        public bool TryMap(string annotationKey,
                           string annotationValue,
                           IReadOnlyCollection<PodPort> podPorts,
                           [NotNullWhen(true)] out PodServiceAnnotation? serviceAnnotation,
                           out ParseErrors parseErrors)
        {
            serviceAnnotation = default;
            parseErrors = new ParseErrors();

            if (_annotationArgumentParser.TryParse(annotationValue, out var arguments, ref parseErrors)
                && TryGetServiceName(arguments, out var serviceName, ref parseErrors)
                && TryGetServicePort(arguments, podPorts, out var servicePort, ref parseErrors))
            {
                var serviceTags = GetServiceTags(arguments);
                var serviceScope = GetServiceScope(arguments, ref parseErrors);
                serviceAnnotation = new PodServiceAnnotation(annotationKey, serviceName, servicePort, serviceTags, serviceScope);
            }

            return serviceAnnotation != default;
        }

        private static bool TryGetServiceName(IReadOnlyDictionary<string, string> arguments,
                                              [NotNullWhen(true)] out string? serviceName,
                                              ref ParseErrors parseErrors)
        {
            if (!TryGetByNameOrPosition(0, "name", arguments, out serviceName))
            {
                parseErrors.Add("Required argument 'service name' defined by the position '0' or the name 'name' is invalid. "
                                + $"Got '{serviceName ?? "<null>"}'.");
            }

            return !string.IsNullOrWhiteSpace(serviceName);
        }

        private static bool TryGetServicePort(IReadOnlyDictionary<string, string> arguments,
                                              IReadOnlyCollection<PodPort> podPorts,
                                              out int servicePort,
                                              ref ParseErrors parseErrors)
        {
            if (TryGetByNameOrPosition(1, "port", arguments, out var servicePortStr))
            {
                if (int.TryParse(servicePortStr, out servicePort))
                {
                    return true;
                }

                var namedPort = podPorts.SingleOrDefault(x => servicePortStr.Equals(x.Name))?.Port;
                if (namedPort != null)
                {
                    servicePort = namedPort.Value;
                    return true;
                }

                parseErrors.Add("Argument 'service port' defined by the position '1' or the name 'port' was specified, "
                                + "but is neither a number or a named container port."
                                + $"Got '{servicePortStr}'.");
                return false;
            }

            if (podPorts.Count == 1)
            {
                servicePort = podPorts.Single().Port;
                return true;
            }

            parseErrors.Add("Argument 'service port' defined by the position '1' or the name 'port' was not specified, "
                            + "and exactly one container port was not found. Unable to automatically detect the service port to use.");

            servicePort = default;
            return false;
        }

        private static string[] GetServiceTags(IReadOnlyDictionary<string, string> arguments)
        {
            if (TryGetByNameOrPosition(3, "tags", arguments, out var serviceTagsStr))
            {
                return serviceTagsStr.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            return new string[0];
        }

        private static PodServiceScope GetServiceScope(IReadOnlyDictionary<string, string> arguments, ref ParseErrors parseErrors)
        {
            if (TryGetByNameOrPosition(4, "scope", arguments, out var serviceScopeStr))
            {
                if (Enum.TryParse<PodServiceScope>(serviceScopeStr, true, out var serviceScope))
                {
                    return serviceScope;
                }

                parseErrors.Add(
                    $"Argument 'service scope' defined by the position '4' or the name 'scope' was specified, but the value '{serviceScopeStr}' is invalid. "
                    + $"A default value to '{PodServiceScope.Node}' will be used.");
            }

            return PodServiceScope.Node;
        }

        private static bool TryGetByNameOrPosition(int position, string name, IReadOnlyDictionary<string, string> arguments,
                                                   [NotNullWhen(true)] out string? value)
        {
            return (arguments.TryGetValue(position.ToString(), out value)
                    || arguments.TryGetValue(name, out value))
                   && !string.IsNullOrWhiteSpace(value);
        }
    }
}