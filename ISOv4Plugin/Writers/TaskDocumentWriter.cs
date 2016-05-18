using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class TaskDocumentWriter : IDisposable
    {
        public XmlWriter RootWriter { get; private set; }
        public MemoryStream XmlStream { get; private set; }
        public string BaseFolder { get; private set; }
        public ApplicationDataModel.ADM.ApplicationDataModel DataModel { get; private set; }


        public Dictionary<int, string> Customers { get; private set; }
        public Dictionary<int, string> Farms { get; private set; }
        public Dictionary<int, string> Fields { get; private set; }
        public Dictionary<int, string> Crops { get; private set; }
        public Dictionary<int, string> Products { get; private set; }
        public Dictionary<int, string> Workers { get; private set; }
        public Dictionary<int, IsoUnit> UserUnits { get; private set; }
        public Dictionary<string, CompoundIdentifier> Ids { get; set; } 

        public TaskDocumentWriter()
        {
            Customers = new Dictionary<int, string>();
            Farms = new Dictionary<int, string>();
            Fields = new Dictionary<int, string>();
            Crops = new Dictionary<int, string>();
            Products = new Dictionary<int, string>();
            Workers = new Dictionary<int, string>();
            UserUnits = new Dictionary<int, IsoUnit>();
            Ids = new Dictionary<string, CompoundIdentifier>();
        }

        public XmlWriter Write(string exportPath, ApplicationDataModel.ADM.ApplicationDataModel dataModel)
        {
            BaseFolder = exportPath;
            DataModel = dataModel;

            CreateFolderStructure();

            XmlStream = new MemoryStream();
            RootWriter = CreateWriter("TASKDATA.XML", XmlStream);
            RootWriter.WriteStartDocument();

            IsoRootWriter.Write(this);
            RootWriter.Flush();

            return RootWriter;
        }

        private void CreateFolderStructure()
        {
            var pathParts = BaseFolder.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var lastPathPart = pathParts.LastOrDefault();
            if (!string.Equals(lastPathPart, "taskdata", StringComparison.OrdinalIgnoreCase))
                BaseFolder = Path.Combine(BaseFolder, "TASKDATA");

            Directory.CreateDirectory(BaseFolder);
        }

        public XmlWriter CreateWriter(string fileName, MemoryStream xmlString)
        {
            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                Indent = true
            };
            return XmlWriter.Create(xmlString, settings);
        }

        public void Dispose()
        {
            using (RootWriter)
            {
            }
        }
    }
}
