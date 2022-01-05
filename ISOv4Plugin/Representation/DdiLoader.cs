/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AgGateway.ADAPT.ISOv4Plugin.Representation
{
    public static class DdiLoader
    {
        private static string _ddiDataLocation = null;


        /// <summary>
        /// Static property to allow applications (particuarly web and cloud) to override the default location of resources as they may require.
        /// Set AgGateway.ADAPT.ISOv4Plugin.Representation.DdiLoader.DDIDataFile = {Path to ddiExport.txt} 
        /// </summary>
        public static string DDIDataFile
        {
            get
            {
                if (_ddiDataLocation == null)
                {
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ddiExport.txt");
                }
                else
                {
                    return _ddiDataLocation;
                }
            }
            set { _ddiDataLocation = value; }
        }

        private static Dictionary<int, DdiDefinition> _ddis;
        public static Dictionary<int, DdiDefinition> Ddis
        {
            get { return _ddis ?? Load(); }
        }

        public static Dictionary<int, DdiDefinition> Load(string ddiExportFileContents = null)
        {
            if (ddiExportFileContents == null)
            {
                ddiExportFileContents = File.ReadAllText(DDIDataFile);
            }

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

            // "Resolution: 0,001"
            var split = resolution.Split(':');
            var value = split[1].Trim().Replace(',', '.');

            double doubleValue;
            //191112 MSp double.TryParse(value, out doubleValue);
            //191112 MSp With German language settings "0,001" repectively "0.001" becomes 1 (instead of 0.001).
            double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out doubleValue); //191112 MSp
            return doubleValue;
        }

        private static int ParseId(string value)
        {
            // DD Entity: 144 Yaw Angle
            var regex = new Regex("\\d+");
            return int.Parse(regex.Matches(value)[0].Value);
        }

        private static string ParseName(string value)
        {
            // DD Entity: 144 Yaw Angle
            var regex = new Regex("\\d+");
            var match = regex.Matches(value)[0];

            return value.Substring(match.Index + match.Length + 1);
        }

        private static string ParseDefinition(string value)
        {
            // Definition: Pivot / Yaw Angle of a DeviceElement
            return value.Substring(12).TrimEnd();
        }

        private static string ParseUnit(string value)
        {
            // Unit: ° - Angle
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var unitDescriptionLocation = value.IndexOf(" - ", StringComparison.Ordinal);
            if (unitDescriptionLocation == -1)
                unitDescriptionLocation = value.IndexOf(" (", StringComparison.Ordinal);

            string parsedUnit = value.Substring(6, unitDescriptionLocation - 6);
            if (parsedUnit.Contains("("))
            {
                //This unit description contained extraneous parenthesized information left of the hyphen
                unitDescriptionLocation = parsedUnit.IndexOf(" (", StringComparison.Ordinal);
                return parsedUnit.Substring(0, unitDescriptionLocation);
            }
            else
            {
                return parsedUnit;
            }
        }
    }
}
