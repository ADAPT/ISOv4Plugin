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
    public interface ICodedCommentListMapper
    {
        IEnumerable<ISOCodedCommentListValue> ExportCodedCommentList(IEnumerable<EnumerationMember> members);
        IEnumerable<EnumerationMember> ImportCodedCommentListValues(IEnumerable<ISOCodedCommentListValue> listValues);
    }

    public class CodedCommentListMapper : BaseMapper, ICodedCommentListMapper
    {
        public CodedCommentListMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "CCL")
        {
        }

        #region Export
        public IEnumerable<ISOCodedCommentListValue> ExportCodedCommentList(IEnumerable<EnumerationMember> members)
        {
            List<ISOCodedCommentListValue> listValues = new List<ISOCodedCommentListValue>();
            foreach (EnumerationMember member in members)
            {
                ISOCodedCommentListValue listValue = ExportCodedCommentListValue(member);
                listValues.Add(listValue);
            }
            return listValues;
        }

        private ISOCodedCommentListValue ExportCodedCommentListValue(EnumerationMember member)
        {
            ISOCodedCommentListValue value = new ISOCodedCommentListValue();
            value.CodedCommentListValueId = GenerateId();
            value.CodedCommentListValueDesignator = member.Value;
            return value;
        }

        #endregion Export 

        #region Import

        public IEnumerable<EnumerationMember> ImportCodedCommentListValues(IEnumerable<ISOCodedCommentListValue> listValues)
        {
            List<EnumerationMember> enumerations = new List<EnumerationMember>();
            foreach (ISOCodedCommentListValue listValue in listValues)
            {
                EnumerationMember member = ImportCodedCommentListValue(listValue);
                enumerations.Add(member);
            }

            return enumerations;
        }


        private EnumerationMember ImportCodedCommentListValue(ISOCodedCommentListValue listValue)
        {
            EnumerationMember member = new EnumerationMember();
            member.Value = listValue.CodedCommentListValueDesignator;
            return member;
        }

        #endregion Import
    }
}
