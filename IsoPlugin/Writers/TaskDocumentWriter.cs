using AgGateway.ADAPT.ApplicationDataModel.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace AgGateway.ADAPT.Plugins.Writers
{
    internal class TaskDocumentWriter : IDisposable
    {
        internal XmlWriter RootWriter { get; private set; }
        internal string BaseFolder { get; private set; }
        internal ApplicationDataModel.ADM.ApplicationDataModel DataModel { get; private set; }


        internal Dictionary<int, string> Customers { get; private set; }
        internal Dictionary<int, string> Farms { get; private set; }
        internal Dictionary<int, string> Fields { get; private set; }
        internal Dictionary<int, string> Crops { get; private set; }
        internal Dictionary<int, string> Products { get; private set; }
        internal Dictionary<int, string> Workers { get; private set; }
        internal Dictionary<int, IsoUnit> UserUnits { get; private set; }


        internal TaskDocumentWriter()
        {
            Customers = new Dictionary<int, string>();
            Farms = new Dictionary<int, string>();
            Fields = new Dictionary<int, string>();
            Crops = new Dictionary<int, string>();
            Products = new Dictionary<int, string>();
            Workers = new Dictionary<int, string>();
            UserUnits = new Dictionary<int, IsoUnit>();
        }

        public void Write(string exportPath, ApplicationDataModel.ADM.ApplicationDataModel dataModel)
        {
            BaseFolder = exportPath;
            DataModel = dataModel;

            CreateFolderStructure();

            RootWriter = CreateWriter("TASKDATA.XML");

            IsoRootWriter.Write(this);
        }

        private void CreateFolderStructure()
        {
            var pathParts = BaseFolder.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var lastPathPart = pathParts.LastOrDefault();
            if (!string.Equals(lastPathPart, "taskdata", StringComparison.OrdinalIgnoreCase))
                BaseFolder = Path.Combine(BaseFolder, "TASKDATA");

            Directory.CreateDirectory(BaseFolder);
        }

        internal XmlWriter CreateWriter(string fileName)
        {
            return XmlWriter.Create(Path.Combine(BaseFolder, fileName), new XmlWriterSettings { Indent = true });
        }

        public void Dispose()
        {
            using (RootWriter) { }
        }
    }
}
