using System;
using System.Collections.Generic;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Readers
{
    public interface ITsksReader
    {
        List<TSK> Read(XPathNodeIterator iterator);
    }

    public class TsksReader : ITsksReader
    {
        private readonly ITaskdataTimReader _taskdataTimReader;
        private readonly ITlgReader _tlgReader;
        private readonly IGrdReader _grdReader;

        public TsksReader() : this(new TaskdataTimReader(), new TlgReader(), new GrdReader())
        {
            
        }
        public TsksReader(ITaskdataTimReader taskdataTimReader, ITlgReader tlgReader, IGrdReader grdReader)
        {
            _taskdataTimReader = taskdataTimReader;
            _tlgReader = tlgReader;
            _grdReader = grdReader;
        }

        public List<TSK> Read(XPathNodeIterator iterator)
        {
            var tsks = new List<TSK>();
            if (iterator.Count == 0)
                return tsks;

            foreach (XPathNavigator node in iterator)
            {
                if (node.SelectChildren("TLG", node.NamespaceURI).Count == 0)
                    continue;

                var tsk = new TSK
                {
                    A = GetStringValue(node, "A"),
                    B = GetStringValue(node, "B"),
                    C = GetStringValue(node, "C"),
                    D = GetStringValue(node, "D"),
                    E = GetStringValue(node, "E"),
                    F = GetStringValue(node, "F"),
                    G = GetEnumValue(node, "G"),
                    H = GetByteValue(node, "H"),
                    I = GetByteValue(node, "I"),
                    J = GetByteValue(node, "J"),
                    Items = GetItems(node),
                };

                tsks.Add(tsk);
            }

            return tsks;
        }

        private byte GetByteValue(XPathNavigator node, string attributeName)
        {
            var value = node.GetAttribute(attributeName, node.NamespaceURI);
            return value != string.Empty ? byte.Parse(value) : (byte)0;
        }

        private TSKG GetEnumValue(XPathNavigator node, string attributeName)
        {
            var value = node.GetAttribute(attributeName, node.NamespaceURI);

            TSKG outValue;
            Enum.TryParse(value, out outValue);
            return outValue;
        }

        private IWriter[] GetItems(XPathNavigator node)
        {
            var children = node.SelectChildren(XPathNodeType.Element);
            var tims = _taskdataTimReader.Read(children.Current.Select("./" + "TIM"));
            var tlgs = _tlgReader.Read(children.Current.Select("./"+"TLG"));
            var grds = _grdReader.Read(children.Current.Select("./" + "GRD"));

            var items = new List<IWriter>();
            items.AddRange(tims);
            items.AddRange(tlgs);
            items.AddRange(grds);
            return items.ToArray();
        }

        private string GetStringValue(XPathNavigator node, string attributeName)
        {
            var value = node.GetAttribute(attributeName, node.NamespaceURI);
            return value != string.Empty ? value : null;
        }
    }
}
