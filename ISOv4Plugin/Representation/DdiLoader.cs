/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AgGateway.ADAPT.ISOv4Plugin.Resources;

namespace AgGateway.ADAPT.ISOv4Plugin.Representation
{
    public static class DdiLoader
    {
        private static Dictionary<int, DdiDefinition> _ddis;
        public static Dictionary<int, DdiDefinition> Ddis
        {
            get { return _ddis ?? Load(); }
        }

        public static Dictionary<int, DdiDefinition> Load(string ddiExportFileContents = null)
        {
            if (ddiExportFileContents == null)
                ddiExportFileContents = Resource.ddiExport;

            _ddis = ParseFile(ddiExportFileContents)
                    .Where(d => d.Unit != "n.a." && d.Unit != "not defined" && d.Unit != "")
                    .ToDictionary(ddiDefinition => ddiDefinition.Id);

            return _ddis;
        }

        private static IEnumerable<DdiDefinition> ParseFile(string ddiExportFileContents)
        {
            var definitions = new List<string>();
            var lines = ddiExportFileContents.Split('\n');

            foreach (var line in lines)
            {
                if (line.Contains("DD Entity:"))
                {
                    if (definitions.Any())
                    {
                        yield return CreateDefinition(definitions);
                    }

                    definitions = new List<string> {line};
                }
                else
                {
                    if(definitions.Any())
                        definitions.Add(line);  
                }
            }

            if(definitions.Any())
                yield return CreateDefinition(definitions);

        }

        private static DdiDefinition CreateDefinition(List<string> definitionLines)
        {
            var nameId = definitionLines.Single(l => l.StartsWith("DD Entity:"));
            var unit = definitionLines.FirstOrDefault(l => l.StartsWith("Unit:"));
            var definition = definitionLines.Single(l => l.StartsWith("Definition:"));
            var resolution = definitionLines.FirstOrDefault(l => l.StartsWith("Resolution:"));

            return CreateNumericDdiDefinition(nameId, definition, unit, resolution);
        }

        private static DdiDefinition CreateNumericDdiDefinition(string nameId, string definition, string unit, string resolution)
        {
            return new DdiDefinition
            {
                Id = ParseId(nameId),
                Name = ParseName(nameId),
                Definition = ParseDefinition(definition),
                Unit = ParseUnit(unit),
                Resolution = ParseResolution(resolution)
            };
        }

        private static double ParseResolution(string resolution)
        {
            if (resolution == null)
                return 0;

            var split = resolution.Split(':');
            var value = split[1].Trim().Replace(',', '.');

            double doubleValue;
            double.TryParse(value, out doubleValue);
            return doubleValue;
        }

        private static int ParseId(string value)
        {
            var regex = new Regex("\\d+");
            return int.Parse(regex.Matches(value)[0].Value);
        }

        private static string ParseName(string value)
        {
            var regex = new Regex("\\d+");
            var match = regex.Matches(value)[0];

            return value.Substring(match.Index + match.Length + 1);
        }

        private static string ParseDefinition(string value)
        {
            return value.Substring(12).TrimEnd();
        }

        private static string ParseUnit(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var unitDescriptionLocation = value.IndexOf(" - ", StringComparison.Ordinal);
            if (unitDescriptionLocation == -1)
                unitDescriptionLocation = value.IndexOf(" (", StringComparison.Ordinal);

            return value.Substring(6, unitDescriptionLocation - 6);
        }
    }
}
