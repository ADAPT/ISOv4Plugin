/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IPolygonMapper
    {
        IEnumerable<ISOPolygon> ExportPolygons(IEnumerable<Polygon> adaptPolygons, ISOPolygonType PolygonType);
        ISOPolygon ExportPolygon(Polygon adaptPolygon, ISOPolygonType PolygonType);
        IEnumerable<Polygon> ImportPolygons(IEnumerable<ISOPolygon> isoPolygons);
        Polygon ImportPolygon(ISOPolygon isoPolygon);
    }

    public class PolygonMapper : BaseMapper, IPolygonMapper
    {
        public PolygonMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "PLN")
        {
        }

        #region Export
        public IEnumerable<ISOPolygon> ExportPolygons(IEnumerable<Polygon> adaptPolygons, ISOPolygonType polygonType)
        {
            List <ISOPolygon> polygons = new List<ISOPolygon>();
            foreach (Polygon polygon in adaptPolygons)
            {
                ISOPolygon ISOPolygon = ExportPolygon(polygon, polygonType);
                polygons.Add(ISOPolygon);
            }
            return polygons;
        }

        public ISOPolygon ExportPolygon(Polygon adaptPolygon, ISOPolygonType polygonType)
        {
            ISOPolygon ISOPolygon = new ISOPolygon();

            ISOPolygon.PolygonType = polygonType;
            ISOPolygon.PolygonDesignator = adaptPolygon.Id.ToString();

            LineStringMapper lsgMapper = new LineStringMapper(TaskDataMapper);
            ISOPolygon.LineStrings = new List<ISOLineString>();
            ISOPolygon.LineStrings.Add(lsgMapper.ExportLinearRing(adaptPolygon.ExteriorRing, ISOLineStringType.PolygonExterior));
            foreach(LinearRing interiorRing in adaptPolygon.InteriorRings)
            {
                ISOPolygon.LineStrings.Add(lsgMapper.ExportLinearRing(interiorRing, ISOLineStringType.PolygonInterior));
            }

            return ISOPolygon;
        }

        #endregion Export 

        #region Import

        public IEnumerable<Polygon> ImportPolygons(IEnumerable<ISOPolygon> isoPolygons)
        {
            List<Polygon> adaptPolygons = new List<Polygon>();
            foreach (ISOPolygon isoPolygon in isoPolygons)
            {
                Polygon adaptPolygon = ImportPolygon(isoPolygon);
                adaptPolygons.Add(adaptPolygon);
            }
            return adaptPolygons;
        }

        public Polygon ImportPolygon(ISOPolygon isoPolygon)
        {
            Polygon polygon = new Polygon();
            LineStringMapper lsgMapper = new LineStringMapper(TaskDataMapper);
            ISOLineString exterior = isoPolygon.LineStrings.FirstOrDefault(l => l.LineStringType == ISOLineStringType.PolygonExterior);
            if (exterior != null)
            {
                polygon.ExteriorRing = lsgMapper.ImportLinearRing(exterior);
            }
            polygon.InteriorRings = new List<LinearRing>();
            polygon.InteriorRings.AddRange(lsgMapper.ImportLinearRings(isoPolygon.LineStrings.Where(l => l.LineStringType == ISOLineStringType.PolygonInterior)));
            return polygon;
        }
        #endregion Import
    }
}
