/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;


namespace AgGateway.ADAPT.ISOv4Plugin
{
    public class TaskDocumentWriter : IDisposable
    {
        public XmlWriter RootWriter { get; private set; }
        public MemoryStream XmlStream { get; private set; }
        public string BaseFolder { get; private set; }

        public TaskDocumentWriter()
        {
        }

        public XmlWriter WriteTaskData(string taskDataPath, ISO11783_TaskData taskData)
        {
            BaseFolder = taskDataPath;

            CreateFolderStructure();

            XmlStream = new MemoryStream();
            RootWriter = CreateWriter(XmlStream);
            RootWriter.WriteStartDocument();
            taskData.WriteXML(RootWriter);
            RootWriter.WriteEndDocument();
            RootWriter.Flush();

            var xml = Encoding.UTF8.GetString(XmlStream.ToArray());
            File.WriteAllText(Path.Combine(BaseFolder, "TASKDATA.XML"), xml);

            return RootWriter;
        }

        public XmlWriter WriteLinkList(string exportPath, ISO11783_LinkList linkList)
        {
            BaseFolder = exportPath;
            CreateFolderStructure();

            XmlStream = new MemoryStream();
            RootWriter = CreateWriter(XmlStream);
            RootWriter.WriteStartDocument();
            linkList.WriteXML(RootWriter);
            RootWriter.WriteEndDocument();
            RootWriter.Flush();

            var xml = Encoding.UTF8.GetString(XmlStream.ToArray());
            File.WriteAllText(Path.Combine(BaseFolder, "LINKLIST.XML"), xml);

            return RootWriter;
        }

        public XmlWriter WriteTimeLog(string exportPath, ISOTimeLog timeLog, ISOTime time)
        {
            BaseFolder = exportPath;
            CreateFolderStructure();

            XmlStream = new MemoryStream();
            RootWriter = CreateWriter(XmlStream);
            RootWriter.WriteStartDocument();
            time.WriteXML(RootWriter);
            RootWriter.WriteEndDocument();
            RootWriter.Flush();

            var xml = Encoding.UTF8.GetString(XmlStream.ToArray());
            File.WriteAllText(Path.Combine(BaseFolder, timeLog.Filename + ".xml"), xml);

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

        public XmlWriter CreateWriter(MemoryStream xmlString)
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
            if(RootWriter != null)
                RootWriter.Dispose();

            if(XmlStream != null)
                XmlStream.Dispose();
        }
    }
}
