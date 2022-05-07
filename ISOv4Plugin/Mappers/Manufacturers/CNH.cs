using System.Collections.Generic;
using System.Globalization;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers.Manufacturers
{
    internal class CNH : IManufacturer
    {
        private Dictionary<int, string> _supportedCrops = new Dictionary<int, string>
        {
            {0, "Beans-Soybean"},
            {1, "Corn"},
            {2, "Wheat"},
            {3, "Oats"},
            {4, "Rye-Winter"},
            {5, "Barley-Winter"},
            {6, "Sorghum"},
            {7, "Popcorn"},
            {8, "Beans-Edible"},
            {9, "Corn2" },
            {10, "Canola"},
            {11, "Rice"},
            {12, "Sunflower"},
            {13, "Maize_CCM"},
            {14, "Maize"},
            {15, "Grains-Other"},
            {16, "Alfalfa"},
            {17, "Barley-Fall"},
            {18, "Barley-Spring"},
            {19, "Beans-Adzuki"},
            {20, "Beans-Faber"},
            {21, "Beans-Field"},
            {22, "Buckwheat"},
            {23, "Borage"},
            {24, "Clover-Crimson"},
            {25, "Clover-White"},
            {26, "Flax " },
            {27, "Grass-Bent"},
            {28, "Grass-Blue"},
            {29, "Grass-Red_Fescue"},
            {30, "Grass-Orchard"},
            {31, "Lentils"},
            {32, "Grass"},
            {33, "Ryegrass-Annual"},
            {34, "Lupins"},
            {35, "Meadowfoam"},
            {36, "Millet"},
            {37, "Mustard"},
            {38, "Peanuts"},
            {39, "Peas-Chick"},
            {40, "Peas-Field"},
            {41, "Rape_Seed"},
            {42, "Rice-Wild"},
            {43, "Ryegrass-Perennial"},
            {44, "Rye-Spring"},
            {45, "Safflower"},
            {46, "Triticale"},
            {47, "Wheat-Durum"},
            {48, "Wheat-Spring"},
            {49, "Wheat-Winter"},
            {50, "ELS"},
            {51, "Upland_Cotton"},
            {52, "Cotton_3" },
            {53, "Cotton_4" },
            {54, "None"},
            {55, "Beans-Broad"},
            {56, "Berries"},
            {57, "Bulbs"},
            {58, "Cabbage"},
            {59, "Cotton"},
            {60, "Fruit-Misc"},
            {61, "Grapes"},
            {62, "Hay_&_Forage"},
            {63, "Hemp"},
            {64, "Horseradish"},
            {65, "Lettuce"},
            {66, "Melons"},
            {67, "Nuts"},
            {68, "Onions"},
            {69, "Peas-Pigeon"},
            {70, "Potatoes"},
            {71, "Spices"},
            {72, "Sugar_Beets"},
            {73, "Sugarcane"},
            {74, "Tomatoes"},
            {75, "Vegetable-Misc"}
        };
        private const string CropTypeAttributeName = "P094_Crop_Type";

        public string GetCropName(ISOElement isoElement)
        {
            Dictionary<string, string> schemaProperties = isoElement?.ProprietarySchemaExtensions;
            if (schemaProperties == null || schemaProperties.Count == 0)
            {
                return string.Empty;
            }

            if (!schemaProperties.TryGetValue(CropTypeAttributeName, out string cropTypeAsString) ||
                !int.TryParse(cropTypeAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int cropType) ||
                !_supportedCrops.TryGetValue(cropType, out string cropName))
            {
                return string.Empty;
            }

            return cropName;
        }
    }
}
