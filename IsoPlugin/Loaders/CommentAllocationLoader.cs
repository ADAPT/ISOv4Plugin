using AgGateway.ADAPT.ApplicationDataModel;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal class CommentAllocationLoader
    {
        private TaskDataDocument _taskDocument;
        private List<LoggedNote> _allocations;

        private CommentAllocationLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _allocations = new List<LoggedNote>();
        }

        internal static List<LoggedNote> Load(XmlNode inputNode, TaskDataDocument taskDocument)
        {
            var loader = new CommentAllocationLoader(taskDocument);
            return loader.Load(inputNode.SelectNodes("CAN"));
        }

        private List<LoggedNote> Load(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                LoadCommentAllocations(inputNode);
            }

            return _allocations;
        }

        private void LoadCommentAllocations(XmlNode inputNode)
        {
            LoggedNote loggedNote = null;

            var commentId = inputNode.GetXmlNodeValue("@A");
            if (!string.IsNullOrEmpty(commentId))
                loggedNote = LoadCodedComment(inputNode, commentId);
            else
                loggedNote = new LoggedNote { Description = inputNode.GetXmlNodeValue("@C") };

            if (loggedNote == null)
                return;

            Point location;
            loggedNote.TimeStamp = AllocationTimestampLoader.Load(inputNode, out location);
            loggedNote.SpatialContext = location;

            _allocations.Add(loggedNote);
        }

        private LoggedNote LoadCodedComment(XmlNode inputNode, string commentId)
        {
            var comment = _taskDocument.Comments.FindById(commentId);
            if (comment == null)
                return null;

            var commentValueId = inputNode.GetXmlNodeValue("@B");
            if (string.IsNullOrEmpty(commentValueId))
                return null;

            var commentValue = comment.Values.FindById(commentValueId);
            if (commentValue == null)
                return null;

            return new LoggedNote
            {
                Value = new EnumeratedValue
                {
                    Value = commentValue,
                    Representation = comment.Comment
                }
            };
        }
    }
}
