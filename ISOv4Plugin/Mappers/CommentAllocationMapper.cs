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
    public interface ICommentAllocationMapper
    {
        IEnumerable<ISOCommentAllocation> ExportCommentAllocations(IEnumerable<Note> adaptCommentAllocations);
        ISOCommentAllocation ExportCommentAllocation(Note note);

        IEnumerable<Note> ImportCommentAllocations(IEnumerable<ISOCommentAllocation> isoCommentAllocations);
        Note ImportCommentAllocation(ISOCommentAllocation isoCommentAllocation);
    }

    public class CommentAllocationMapper : BaseMapper, ICommentAllocationMapper
    {
        public CommentAllocationMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "CAN")
        {
        }

        #region Export
        public IEnumerable<ISOCommentAllocation> ExportCommentAllocations(IEnumerable<Note> adaptCommentAllocations)
        {
            List<ISOCommentAllocation> CommentAllocations = new List<ISOCommentAllocation>();
            foreach (Note note in adaptCommentAllocations)
            {
                ISOCommentAllocation isoCommentAllocation = ExportCommentAllocation(note);
                CommentAllocations.Add(isoCommentAllocation);
            }
            return CommentAllocations;
        }

        public ISOCommentAllocation ExportCommentAllocation(Note note)
        {
            ISOCommentAllocation commentAllocation = new ISOCommentAllocation();

            if (note.Value == null)
            {
                commentAllocation.FreeCommentText = note.Description;
            }
            else
            {
                if (note.Value.Representation != null)
                {
                    ISOCodedComment comment = TaskDataMapper.CommentMapper.ExportCodedComment(note.Value);
                    ISOCodedCommentListValue value = comment.CodedCommentListValues.FirstOrDefault(c => c.CodedCommentListValueDesignator == note.Value.Value.Value);
                    if (value != null)
                    {
                        commentAllocation.CodedCommentListValueIdRef = value.CodedCommentListValueId;
                    }
                }
            }

            //Allocation Stamps
            if (note.TimeStamps.Any())
            {
                commentAllocation.AllocationStamp = AllocationStampMapper.ExportAllocationStamps(note.TimeStamps).FirstOrDefault();
            }

            return commentAllocation;
        }

        #endregion Export 

        #region Import

        public IEnumerable<Note> ImportCommentAllocations(IEnumerable<ISOCommentAllocation> isoCommentAllocations)
        {
            //Import Notes
            List<Note> adaptNotes = new List<Note>();
            foreach (ISOCommentAllocation ISOCommentAllocation in isoCommentAllocations)
            {
                Note adaptNote = ImportCommentAllocation(ISOCommentAllocation);
                adaptNotes.Add(adaptNote);
            }

            return adaptNotes;
        }


        public Note ImportCommentAllocation(ISOCommentAllocation isoCommentAllocation)
        {
            Note adaptNote = new Note();

            //Allocation Stamps
            if (isoCommentAllocation.AllocationStamp != null)
            {
                adaptNote.TimeStamps = AllocationStampMapper.ImportAllocationStamps(new List<ISOAllocationStamp>() {isoCommentAllocation.AllocationStamp }).ToList();
            }

            //Coded Comment
            if (!string.IsNullOrEmpty(isoCommentAllocation.CodedCommentIdRef) && !string.IsNullOrEmpty(isoCommentAllocation.CodedCommentListValueIdRef))
            {
                ISOCodedComment comment = ISOTaskData.ChildElements.OfType<ISOCodedComment>().FirstOrDefault(c => c.CodedCommentID == isoCommentAllocation.CodedCommentIdRef);
                ISOCodedCommentListValue value = comment.CodedCommentListValues.FirstOrDefault(l => l.CodedCommentListValueId == isoCommentAllocation.CodedCommentListValueIdRef);
                if (comment != null)
                {
                    adaptNote.Description = comment.CodedCommentDesignator;
                    adaptNote.Value = TaskDataMapper.CommentMapper.ImportCodedComment(comment);
                    adaptNote.Value.Value = adaptNote.Value.Representation.EnumeratedMembers.FirstOrDefault(m => m.Value == value.CodedCommentListValueDesignator);
                }
            }
            else
            {
                adaptNote.Description = isoCommentAllocation.FreeCommentText;
            }


            return adaptNote;
        }

        #endregion Import
    }
}
