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
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.Notes;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Common;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface ICodedCommentMapper
    {
        IEnumerable<ISOCodedComment> ExportCodedComments(IEnumerable<EnumeratedValue> enumeratedValues);
        ISOCodedComment ExportCodedComment(EnumeratedValue value);
        IEnumerable<EnumeratedValue> ImportCodedComments(IEnumerable<ISOCodedComment> isoComments);
        EnumeratedValue ImportCodedComment(ISOCodedComment comment);
    }

    public class CodedCommentMapper : BaseMapper, ICodedCommentMapper
    {
        CodedCommentListMapper _listMapper;
        public CodedCommentMapper(TaskDataMapper taskDataMapper, CodedCommentListMapper listMapper) : base(taskDataMapper, "CCT")
        {
            _listMapper = listMapper;
            ExportedComments = new List<ISOCodedComment>();
        }

        public List<ISOCodedComment> ExportedComments { get; set; }

        #region Export
        public IEnumerable<ISOCodedComment> ExportCodedComments(IEnumerable<EnumeratedValue> enumeratedValues)
        {
            List<ISOCodedComment> comments = new List<ISOCodedComment>();
            foreach (EnumeratedValue value in enumeratedValues)
            {
                ISOCodedComment isoCommentAllocation = ExportCodedComment(value);
                comments.Add(isoCommentAllocation);
            }

            return comments;
        }

        public ISOCodedComment ExportCodedComment(EnumeratedValue value)
        {
            ISOCodedComment comment = new ISOCodedComment();

            comment.CodedCommentID = GenerateId();
            comment.CodedCommentDesignator = value.Representation.Description;
            comment.CodedCommentListValues = _listMapper.ExportCodedCommentList(value.Representation.EnumeratedMembers).ToList();
            ExportedComments.Add(comment);
            return comment;
        }

        #endregion Export 

        #region Import

        public IEnumerable<EnumeratedValue> ImportCodedComments(IEnumerable<ISOCodedComment> isoComments)
        {
            List<EnumeratedValue> enumerations = new List<EnumeratedValue>();
            foreach (ISOCodedComment isoComment in isoComments)
            {
                EnumeratedValue enumeration = ImportCodedComment(isoComment);
                enumerations.Add(enumeration);
            }

            return enumerations;
        }


        public EnumeratedValue ImportCodedComment(ISOCodedComment comment)
        {
            EnumeratedValue value = new EnumeratedValue();
            value.Designator = comment.CodedCommentDesignator;
            value.Representation = new EnumeratedRepresentation();
            value.Representation.EnumeratedMembers = _listMapper.ImportCodedCommentListValues(comment.CodedCommentListValues).ToList();
            return value;
        }

        #endregion Import
    }
}
