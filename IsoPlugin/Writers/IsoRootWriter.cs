using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel;

namespace AgGateway.ADAPT.Plugins.Writers
{
    internal class IsoRootWriter
    {
        internal static void Write(TaskDocumentWriter writer)
        {
            writer.RootWriter.WriteStartElement("ISO11783_TaskData");
            writer.RootWriter.WriteAttributeString("VersionMajor", "4");
            writer.RootWriter.WriteAttributeString("VersionMinor", "0");
            writer.RootWriter.WriteAttributeString("ManagementSoftwareManufacturer", "AgGateway");
            writer.RootWriter.WriteAttributeString("ManagementSoftwareVersion", "1.0");

            if (writer.DataModel != null)
            {
                if (writer.DataModel.Catalog != null)
                    WriteMetaItems(writer);
            }

            writer.RootWriter.WriteEndElement();
        }

        private static void WriteMetaItems(TaskDocumentWriter writer)
        {
            CropWriter.Write(writer);
            CustomerWriter.Write(writer);
            FarmWriter.Write(writer);
            FieldWriter.Write(writer);
            ProductWriter.Write(writer);
            WorkerWriter.Write(writer);
            CommentWriter.Write(writer);
            PrescriptionWriter.Write(writer);
        }
    }
}
