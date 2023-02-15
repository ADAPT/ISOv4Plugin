/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.Mappers.Manufacturers;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IPolygonMapper
    {
        IEnumerable<ISOPolygon> ExportMultipolygon(MultiPolygon adaptMultiPolygon, ISOPolygonType PolygonType);
        ISOPolygon ExportPolygon(Polygon adaptPolygon, ISOPolygonType PolygonType);
        IEnumerable<Polygon> ImportBoundaryPolygons(IEnumerable<ISOPolygon> isoPolygons);
        IEnumerable<Polygon> ImportBoundaryPolygon(ISOPolygon isoPolygon, bool isVersion3Multipolygon);
        IEnumerable<AttributeShape> ImportAttributePolygons(IEnumerable<ISOPolygon> isoPolygons);
        AttributeShape ImportAttributePolygon(ISOPolygon isoPolygon);
    }

    public class PolygonMapper : BaseMapper, IPolygonMapper
    {
        private readonly IManufacturer _manufacturer;

        public PolygonMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "PLN")
        {
            _manufacturer = ManufacturerFactory.GetManufacturer(taskDataMapper);
        }

        #region Export
        public IEnumerable<ISOPolygon> ExportMultipolygon(MultiPolygon adaptMultiPolygon, ISOPolygonType polygonType)
        {
            List<ISOPolygon> polygons = new List<ISOPolygon>();
            if (TaskDataMapper.Version > 3)
            {
                //Version 4 supports multiple polygons to define a spatial entity
                foreach (Polygon polygon in adaptMultiPolygon.Polygons)
                {
                    ISOPolygon ISOPolygon = ExportPolygon(polygon, polygonType);
                    polygons.Add(ISOPolygon);
                }
            }
            else
            {
                //Version 3
                if (adaptMultiPolygon != null && adaptMultiPolygon.Polygons.Any())
                {
                    ISOPolygon polygon = ExportVersion3Polygon(adaptMultiPolygon.Polygons, polygonType);
                    polygons.Add(polygon);
                }
            }
            return polygons;
        }

        //Version 3 defines a multipolygon by a single PLN with multiple exterior linestrings and interior rings in order below their exterior
        private ISOPolygon ExportVersion3Polygon(List<Polygon> adaptPolygons, ISOPolygonType polygonType)
        {
            ISOPolygon isoPolygon = new ISOPolygon(TaskDataMapper.Version);
            if ((int)polygonType < 9)
            {
                isoPolygon.PolygonType = polygonType;
            }
            else
            {
                isoPolygon.PolygonType = ISOPolygonType.Other; //Version 3 stops at 8/Other
            }

            isoPolygon.PolygonDesignator = adaptPolygons.First().Id.ToString();
            LineStringMapper lsgMapper = new LineStringMapper(TaskDataMapper);
            isoPolygon.LineStrings = new List<ISOLineString>();
            foreach (Polygon adaptPolygon in adaptPolygons)
            {
                if (adaptPolygon.ExteriorRing != null)
                {
                    isoPolygon.LineStrings.Add(lsgMapper.ExportLinearRing(adaptPolygon.ExteriorRing, ISOLineStringType.PolygonExterior));
                }
                if (adaptPolygon.InteriorRings != null)
                {
                    foreach (LinearRing interiorRing in adaptPolygon.InteriorRings)
                    {
                        isoPolygon.LineStrings.Add(lsgMapper.ExportLinearRing(interiorRing, ISOLineStringType.PolygonInterior));
                    }
                }
            }
            return isoPolygon;
        }

        public ISOPolygon ExportPolygon(Polygon adaptPolygon, ISOPolygonType polygonType)
        {
            ISOPolygon isoPolygon = new ISOPolygon(TaskDataMapper.Version);

            isoPolygon.PolygonType = polygonType;
            isoPolygon.PolygonDesignator = adaptPolygon.Id.ToString();

            LineStringMapper lsgMapper = new LineStringMapper(TaskDataMapper);
            isoPolygon.LineStrings = new List<ISOLineString>();
            if (adaptPolygon.ExteriorRing != null)
            {
                isoPolygon.LineStrings.Add(lsgMapper.ExportLinearRing(adaptPolygon.ExteriorRing, ISOLineStringType.PolygonExterior));
            }
            if (adaptPolygon.InteriorRings != null)
            {
                foreach (LinearRing interiorRing in adaptPolygon.InteriorRings)
                {
                    isoPolygon.LineStrings.Add(lsgMapper.ExportLinearRing(interiorRing, ISOLineStringType.PolygonInterior));
                }
            }

            return isoPolygon;
        }

        #endregion Export

        #region Import

        public IEnumerable<Polygon> ImportBoundaryPolygons(IEnumerable<ISOPolygon> isoPolygons)
        {
            List<Polygon> adaptPolygons = new List<Polygon>();

            //Prior to version 4, ISOXML allowed multiple external linestrings in a single polygon, logically equating to a multipolygon
            //In version 4, multipolygons must be rendered as multiple polygons
            bool isVersion3Multipolygon = isoPolygons.Count() == 1 && isoPolygons.Single().LineStrings.Count(i => i.LineStringType == ISOLineStringType.PolygonExterior) > 1;
            foreach (ISOPolygon isoPolygon in isoPolygons)
            {
                IEnumerable<Polygon> polygonOutput = ImportBoundaryPolygon(isoPolygon, isVersion3Multipolygon);
                if (polygonOutput != null)
                {
                    adaptPolygons.AddRange(polygonOutput);
                }
            }

            _manufacturer?.PostProcessPolygons(adaptPolygons);

            return adaptPolygons;
        }

        public IEnumerable<AttributeShape> ImportAttributePolygons(IEnumerable<ISOPolygon> isoPolygons)
        {
            List<AttributeShape> attributePolygons = new List<AttributeShape>();
            foreach (ISOPolygon isoPolygon in isoPolygons)
            {
                AttributeShape attributePolygon = ImportAttributePolygon(isoPolygon);
                if (attributePolygon != null)
                {
                    attributePolygons.Add(attributePolygon);
                }
            }
            return attributePolygons;
        }

        /// <summary>
        /// Returns an ADAPT Polygon for ISOPolygons defined with Exterior/Interior rings.  Null otherwise
        /// </summary>
        /// <param name="isoPolygon"></param>
        /// <returns></returns>
        public IEnumerable<Polygon> ImportBoundaryPolygon(ISOPolygon isoPolygon, bool isVersion3Multipolygon)
        {
            if (IsFieldAttributeType(isoPolygon))
            {
                //Polygon is defined as an area of interest and not a PFD/TZN boundary, etc.
                return null;
            }

            LineStringMapper lsgMapper = new LineStringMapper(TaskDataMapper);
            List<Polygon> output = new List<Polygon>();
            if (isVersion3Multipolygon)
            {
                //Version 3 only allowed one polygon for the boundary with multiple external linestrings acting as individual polygons
                foreach (ISOLineString ls in isoPolygon.LineStrings)
                {
                    if (ls.LineStringType == ISOLineStringType.PolygonExterior)
                    {
                        Polygon polygon = new Polygon { ExteriorRing = lsgMapper.ImportLinearRing(ls) };
                        if (isoPolygon.PolygonDesignator != null)
                        {
                            polygon.ContextItems.Add(new ContextItem() { Code = "Pr_ISOXML_Attribute_Designator", Value = isoPolygon.PolygonDesignator });
                        }
                        output.Add(polygon);
                    }
                    else if (ls.LineStringType == ISOLineStringType.PolygonInterior)
                    {
                        //We will interpret any interior linestrings as belonging to the preceeding external linestring
                        output.Last().InteriorRings.Add(lsgMapper.ImportLinearRing(ls));
                    }
                }
            }
            else
            {
                //Normal Polygon behavior with only one possible exterior ring.
                ISOLineString exteriorRing = isoPolygon.LineStrings.FirstOrDefault(l => l.LineStringType == ISOLineStringType.PolygonExterior);
                IEnumerable<ISOLineString> interiorRings = isoPolygon.LineStrings.Where(l => l.LineStringType == ISOLineStringType.PolygonInterior);
                if (exteriorRing != null || interiorRings.Any())
                {
                    Polygon polygon = new Polygon();
                    if (exteriorRing != null)
                    {
                        polygon.ExteriorRing = lsgMapper.ImportLinearRing(exteriorRing);
                    }
                    polygon.InteriorRings = lsgMapper.ImportLinearRings(interiorRings).ToList();
                    if (isoPolygon.PolygonDesignator != null)
                    {
                        polygon.ContextItems.Add(new ContextItem() { Code = "Pr_ISOXML_Attribute_Designator", Value = isoPolygon.PolygonDesignator });
                    }
                    output.Add(polygon);
                }
            }
            return output;
        }

        /// <summary>
        /// Returns an ADAPT Polygon for ISOPolygons describing field points of interest.  Null otherwise
        /// </summary>
        /// <param name="isoPolygon"></param>
        /// <returns></returns>
        public AttributeShape ImportAttributePolygon(ISOPolygon isoPolygon)
        {
            Polygon boundaryPolygon = ImportBoundaryPolygon(isoPolygon, false)?.FirstOrDefault(); 
            if (boundaryPolygon != null && IsFieldAttributeType(isoPolygon))
            {
                //The data has defined an explicit PLN type that maps to an attribute type
                return new AttributeShape() {
                    Shape = boundaryPolygon,
                    TypeName = Enum.GetName(typeof(ISOPolygonType), isoPolygon.PolygonType),
                    Name = isoPolygon.PolygonDesignator };
            }
            else if (isoPolygon.LineStrings.Count == 1)
            {
                //If no linestrings defined as interior/exterior, we expect only 1 linestring
                ISOLineString attributeLsg = isoPolygon.LineStrings.FirstOrDefault(ls => LineStringMapper.IsFieldAttributeType(ls));
                if (attributeLsg != null)
                {
                    LineStringMapper lsgMapper = new LineStringMapper(TaskDataMapper);
                    Polygon polygon = new Polygon()
                    {
                        ExteriorRing = lsgMapper.ImportLinearRing(attributeLsg)
                    };
                    return new AttributeShape()
                    {
                        Shape = polygon,
                        TypeName = Enum.GetName(typeof(ISOPolygonType), isoPolygon.PolygonType),
                        Name = isoPolygon.PolygonDesignator
                    };
                }
            }
            return null;  //This polygon does not map to an InteriorBoundaryAttribute.
        }

        internal static bool IsFieldAttributeType(ISOPolygon isoPolygon)
        {
            return isoPolygon.PolygonType == ISOPolygonType.BufferZone ||
                    isoPolygon.PolygonType == ISOPolygonType.Building ||
                    isoPolygon.PolygonType == ISOPolygonType.Flag ||
                    isoPolygon.PolygonType == ISOPolygonType.Obstacle ||
                    isoPolygon.PolygonType == ISOPolygonType.Road ||
                    isoPolygon.PolygonType == ISOPolygonType.WaterSurface ||
                    isoPolygon.PolygonType == ISOPolygonType.Windbreak;
        }
        #endregion Import
    }

    /// <summary>
    /// Utility class to map to FieldBoundary.InteriorBoundaryAttribute
    /// </summary>
    public class AttributeShape
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public Shape Shape { get; set; }
    }
}
