using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Notes;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public class CommentAllocationLoader
    {
        private TaskDataDocument _taskDocument;
        private List<Note> _allocations;

        private CommentAllocationLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _allocations = new List<Note>();
        }

        public static List<Note> Load(XmlNode inputNode, TaskDataDocument taskDocument)
        {
            var loader = new CommentAllocationLoader(taskDocument);
            return loader.Load(inputNode.SelectNodes("CAN"));
        }

        private List<Note> Load(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                LoadCommentAllocations(inputNode);
            }

            return _allocations;
        }

        private void LoadCommentAllocations(XmlNode inputNode)
        {
            Note note = null;

            var commentId = inputNode.GetXmlNodeValue("@A");
            if (!string.IsNullOrEmpty(commentId))
                note = LoadCodedComment(inputNode, commentId);
            else
                note = new Note { Description = inputNode.GetXmlNodeValue("@C") };

            if (note == null)
                return;

            var noteTimeStamp = AllocationTimestampLoader.Load(inputNode);
            note.TimeStamps.Add(noteTimeStamp);
            if (noteTimeStamp != null && noteTimeStamp.Location1 != null)
                note.SpatialContext = noteTimeStamp.Location1.Position;

            _allocations.Add(note);
        }

        private Note LoadCodedComment(XmlNode inputNode, string commentId)
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

            return new Note
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
