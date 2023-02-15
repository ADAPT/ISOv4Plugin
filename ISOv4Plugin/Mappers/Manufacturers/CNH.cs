using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
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
        private const string ProductFormAttributeName = "P094_Product_Form";
        private const string ProductUsageAttributeName = "P094_Product_Usage";
        private const string ProductManufacturer = "P094_Product_Manufacturer";

        private string GetAttributeByName(ISOElement isoElement, string name)
        {
            Dictionary<string, string> schemaProperties = isoElement?.ProprietarySchemaExtensions;
            if (schemaProperties == null || schemaProperties.Count == 0)
            {
                return string.Empty;
            }

            return schemaProperties.TryGetValue(name, out string value)
                ? value
                : string.Empty;
        }

        public string GetCropName(ISOElement isoElement)
        {
            var cropTypeAsString = GetAttributeByName(isoElement, CropTypeAttributeName);

            if (string.IsNullOrEmpty(cropTypeAsString) ||
                !int.TryParse(cropTypeAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int cropType) ||
                !_supportedCrops.TryGetValue(cropType, out string cropName))
            {
                return string.Empty;
            }

            return cropName;
        }

        public ProductFormEnum? GetProductForm(ISOProduct isoProduct)
        {
            var productFormAsString = GetAttributeByName(isoProduct, ProductFormAttributeName);

            if (string.IsNullOrEmpty(productFormAsString) ||
               !int.TryParse(productFormAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int productForm))
            {
                return null;
            }

            switch (productForm)
            {
                case 0: // Anhydrous
                    return ProductFormEnum.Gas;
                case 1: // Granular/Other
                case 3: // Plant
                case 4: // Seed
                case 5: // Bulk Seed
                    return ProductFormEnum.Solid;
                case 2: // Liquid
                    return ProductFormEnum.Liquid;
                default:
                    return null;
            }
        }

        public ProductTypeEnum? GetProductType(ISOProduct isoProduct)
        {
            var productFormAsString = GetAttributeByName(isoProduct, ProductFormAttributeName);

            if (string.IsNullOrEmpty(productFormAsString) ||
               !int.TryParse(productFormAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int productForm))
            {
                return null;
            }

            switch (productForm)
            {
                case 0: // Anhydrous
                    return ProductTypeEnum.Fertilizer;
                default:
                    return null;
            }
        }

        public CategoryEnum? GetProductCategory(ISOProduct isoProduct)
        {
            var productUsageAsString = GetAttributeByName(isoProduct, ProductUsageAttributeName);

            if (string.IsNullOrEmpty(productUsageAsString) ||
               !int.TryParse(productUsageAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int productUsage))
            {
                return null;
            }

            switch (productUsage)
            {
                case 0: // Generic
                    return CategoryEnum.Unknown;
                case 1: // Carrier
                    return CategoryEnum.Carrier;
                case 2: // Fertilizer
                    return CategoryEnum.Fertilizer;
                case 3: // Herbicide
                    return CategoryEnum.Herbicide;
                case 4: // Fungicide
                    return CategoryEnum.Fungicide;
                case 5: // Insecticide
                    return CategoryEnum.Insecticide;
                case 6: // Pesticide
                    return CategoryEnum.Pesticide;
                case 7: // Seed/Plant
                    return CategoryEnum.Variety;
            }
            return null;
        }

        public string GetProductManufacturer(ISOProduct isoProduct)
        {
            var productManufacturer = GetAttributeByName(isoProduct, ProductManufacturer);

            if (string.IsNullOrEmpty(productManufacturer))
            {
                return string.Empty;
            }

            return productManufacturer;
        }


        public IEnumerable<OperationData> PostProcessOperationData(TaskDataMapper taskDataMapper, IEnumerable<OperationData> operationDatas)
        {
            var result = new List<OperationData>();

            var catalog = taskDataMapper.AdaptDataModel.Catalog;
            foreach (var operationData in operationDatas)
            {
                var deviceModels = operationData.GetAllSections()
                    .Select(x => catalog.DeviceElementConfigurations.FirstOrDefault(y => y.Id.ReferenceId == x.DeviceConfigurationId))
                    .Where(x => x != null)
                    .Select(x => catalog.DeviceElements.FirstOrDefault(y => y.Id.ReferenceId == x.DeviceElementId))
                    .Where(x => x != null)
                    .Select(x => x.DeviceModelId)
                    .Distinct()
                    .Select(x => catalog.DeviceModels.FirstOrDefault(y => y.Id.ReferenceId == x))
                    .Where(x => x != null)
                    .ToList();
                if (deviceModels.Count == 1 && !string.IsNullOrWhiteSpace(deviceModels[0].Description))
                {
                    var trimmed = deviceModels[0].Description.Trim();
                    if (trimmed.EqualsIgnoreCase("Trip Computer Data"))
                    {
                        operationData.OperationType = OperationTypeEnum.DataCollection;
                    }
                    else if (trimmed.EqualsIgnoreCase("Vehicle Geometry"))
                    {
                        continue;
                    }
                }
                result.Add(operationData);
            }
            return result;
        }

        public void PostProcessPolygons(List<Polygon> polygons)
        {
            var groupedPolygons = polygons.GroupBy(x => x.ExteriorRing != null).ToDictionary(x => x.Key, x => x.ToList());
            if (!groupedPolygons.TryGetValue(true, out var exteriorPolygons) ||
                !groupedPolygons.TryGetValue(false, out var interiorPolygons))
            {
                return;
            }

            foreach (var exteriorPolygon in exteriorPolygons)
            {
                var exteriorRing = exteriorPolygon.ExteriorRing;
                var boundingBox = new BoundingBox
                {
                    MaxX = new NumericRepresentationValue(null, new NumericValue(null, exteriorRing.Points.Max(p => p.X))),
                    MinX = new NumericRepresentationValue(null, new NumericValue(null, exteriorRing.Points.Min(p => p.X))),
                    MaxY = new NumericRepresentationValue(null, new NumericValue(null, exteriorRing.Points.Max(p => p.Y))),
                    MinY = new NumericRepresentationValue(null, new NumericValue(null, exteriorRing.Points.Min(p => p.Y))),
                };

                foreach (var interiorPolygon in interiorPolygons)
                {
                    if (interiorPolygon.InteriorRings == null || interiorPolygon.InteriorRings.Count <= 0)
                    {
                        continue;
                    }

                    // Test if a single point from interior polygon lies within exterior polygon
                    if (IsPointInPolygon(boundingBox, exteriorRing, interiorPolygon?.InteriorRings.First().Points.FirstOrDefault()))
                    {
                        exteriorPolygon.InteriorRings = exteriorPolygon.InteriorRings ?? new List<LinearRing>();
                        exteriorPolygon.InteriorRings.AddRange(interiorPolygon.InteriorRings);

                        polygons.Remove(interiorPolygon);
                    }
                }
            }
        }

        private bool IsPointInPolygon(BoundingBox boundingBox, LinearRing ring, Point testPoint)
        {
            if (testPoint.X < boundingBox.MinX.Value.Value || testPoint.X > boundingBox.MaxX.Value.Value ||
                testPoint.Y < boundingBox.MinY.Value.Value || testPoint.Y > boundingBox.MaxY.Value.Value)
            {
                return false;
            }

            // Following code adapted from https://wrf.ecse.rpi.edu/Research/Short_Notes/pnpoly.html
            bool inside = false;
            for (int i = 0, j = ring.Points.Count - 1; i < ring.Points.Count; j = i++)
            {
                var pointI = ring.Points[i];
                var pointJ = ring.Points[j];
                if ((pointI.Y > testPoint.Y) != (pointJ.Y > testPoint.Y) &&
                    testPoint.X < (pointJ.X - pointI.X) * (testPoint.Y - pointI.Y) / (pointJ.Y - pointI.Y) + pointI.X)
                {
                    inside = !inside;
                }
            }
            return inside;
        }
    }
}
