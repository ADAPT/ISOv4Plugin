using AgGateway.ADAPT.ApplicationDataModel.Representations;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal class CommentLoader
    {
        private XmlNode _rootNode;
        private string _baseFolder;
        private TaskDataDocument _taskDocument;
        private Dictionary<string, CodedComment> _comments;

        private CommentLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _comments = new Dictionary<string, CodedComment>();
        }

        internal static Dictionary<string, CodedComment> Load(TaskDataDocument taskDocument)
        {
            var loader = new CommentLoader(taskDocument);

            return loader.Load();
        }

        private Dictionary<string, CodedComment> Load()
        {
            LoadComments(_rootNode.SelectNodes("CCT"));
            ProcessExternalNodes();

            return _comments;
        }

        private void ProcessExternalNodes()
        {
            var externalNodes = _rootNode.SelectNodes("XFR[starts-with(@A, 'CCT')]");
            for (int i = 0; i < externalNodes.Count; i++)
            {
                var inputNodes = externalNodes[i].LoadActualNodes("XFR", _baseFolder);
                if (inputNodes == null)
                    continue;
                LoadComments(inputNodes);
            }
        }

        private void LoadComments(XmlNodeList inputNodes)
        {
            for (int i = 0; i < inputNodes.Count; i++)
            {
                string commentId;
                var comment = LoadComment(inputNodes[i], out commentId);
                if (comment != null)
                    _comments.Add(commentId, comment);
            }
        }

        private CodedComment LoadComment(XmlNode inputNode, out string commentId)
        {
            var comment = new CodedComment
            {
                Comment = new EnumeratedRepresentation()
            };

            // Required fields. Do not proceed if they are missing
            commentId = inputNode.GetXmlNodeValue("@A");
            comment.Comment.Description = inputNode.GetXmlNodeValue("@B");
            if (commentId == null || comment.Comment.Description == null)
                return null;

            if (!LoadScope(inputNode.GetXmlNodeValue("@C"), comment))
                return null;

            // Optional fields
            LoadListValues(inputNode.SelectNodes("CCL"), comment);

            _taskDocument.LoadLinkedIds(commentId, comment.Comment.Id);
            return comment;
        }

        private static bool LoadScope(string scopeValue, CodedComment comment)
        {
            if (string.IsNullOrEmpty(scopeValue))
                return false;

            byte scope;
            if (!byte.TryParse(scopeValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out scope)
                || scope < 1 || scope > 3)
                return false;

            comment.Scope = scope;
            return true;
        }

        private static void LoadListValues(XmlNodeList inputNodes, CodedComment comment)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                var valueId = inputNode.GetXmlNodeValue("@A");
                var description = inputNode.GetXmlNodeValue("@B");

                if (string.IsNullOrEmpty(valueId) || string.IsNullOrEmpty(description))
                    continue;

                if (comment.Values == null)
                {
                    comment.Values = new Dictionary<string, EnumerationMember>();
                    comment.Comment.EnumeratedMembers = new List<EnumerationMember>();
                }

                var listValue = new EnumerationMember
                {
                    Value = description
                };

                comment.Comment.EnumeratedMembers.Add(listValue);
                comment.Values[valueId] = listValue;
            }
        }
    }
}
