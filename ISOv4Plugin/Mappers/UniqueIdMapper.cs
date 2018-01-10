/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using System.Collections.Generic;
using System.Linq;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public class UniqueIdMapper 
    {
        public UniqueIdMapper(ISO11783_LinkList linkList)
        {
            LinkList = linkList;
        }

        public ISO11783_LinkList LinkList { get; private set; }

        private int _lgpID = 1;

        public const string IsoSource = "http://dictionary.isobus.net/isobus/";

        #region Export
        public void ExportUniqueIDs(CompoundIdentifier id, string isoIDRef)
        {
            foreach (UniqueId uid in id.UniqueIds)
            {
                ISOLink link = new ISOLink();
                link.ObjectIdRef = isoIDRef;
                link.LinkValue = uid.Id;
                //link.LinkDesignator = 

                //Find or create the right LinkGroup
                ISOLinkGroup linkGroup = null;
                if (uid.Source == IsoSource)
                {
                    //No need to export temporary ISO ids
                    continue;
                }
                if (uid.IdType == IdTypeEnum.UUID)
                {
                    //Add to global UUID list
                    linkGroup = LinkList.LinkGroups.FirstOrDefault(lg => lg.LinkGroupType == ISOEnumerations.ISOLinkGroupType.UUID);
                    if (linkGroup == null)
                    {
                        linkGroup = new ISOLinkGroup() { LinkGroupType = ISOEnumerations.ISOLinkGroupType.UUID, LinkGroupDesignator = "UUIDs", Links = new List<ISOLink>() };
                        linkGroup.LinkGroupId = BaseMapper.GenerateId(0, "LGP", _lgpID++); //Special ID invocation here due to class relationships
                        LinkList.LinkGroups.Add(linkGroup);
                    }
                }
                else if (uid.SourceType == IdSourceTypeEnum.GLN)
                {
                    //Find GLN group for specfic GLN
                    linkGroup = LinkList.LinkGroups.FirstOrDefault(lg => lg.LinkGroupType == ISOEnumerations.ISOLinkGroupType.ManufacturerProprietary && lg.ManufacturerGLN == uid.Source);
                    if (linkGroup == null)
                    {
                        linkGroup = new ISOLinkGroup() { LinkGroupType = ISOEnumerations.ISOLinkGroupType.ManufacturerProprietary, ManufacturerGLN = uid.Source, LinkGroupDesignator = "GLNs", Links = new List<ISOLink>() };
                        LinkList.LinkGroups.Add(linkGroup);
                    }
                }
                else
                {
                    //Add to global URI list
                    linkGroup = LinkList.LinkGroups.FirstOrDefault(lg => lg.LinkGroupType == ISOEnumerations.ISOLinkGroupType.InformationalResolvable);
                    if (linkGroup == null)
                    {
                        linkGroup = new ISOLinkGroup() { LinkGroupType = ISOEnumerations.ISOLinkGroupType.InformationalResolvable, LinkGroupDesignator = "URIs", Links = new List<ISOLink>() };
                        LinkList.LinkGroups.Add(linkGroup);
                    }
                }

                //Add the link
                linkGroup.Links.Add(link);
            }
        }
        #endregion Export

        #region Import
        public IEnumerable<UniqueId> ImportUniqueIDs(string isoObjectIdRef)
        {
            List<UniqueId> uniqueIDs = new List<UniqueId>();

            //1. Add any matching link in the LinkList.xml
            if (LinkList != null)
            {
                foreach (ISOLinkGroup isoLinkGroup in LinkList.LinkGroups)
                {
                    ISOLink link = isoLinkGroup.Links.FirstOrDefault(l => l.ObjectIdRef == isoObjectIdRef);
                    if (link != null)
                    {
                        UniqueId adaptID = new UniqueId();
                        adaptID.Id = link.LinkValue;

                        if (isoLinkGroup.LinkGroupType == ISOEnumerations.ISOLinkGroupType.UUID)
                        {
                            adaptID.IdType = IdTypeEnum.UUID;
                        }
                        else if (isoLinkGroup.LinkGroupType == ISOEnumerations.ISOLinkGroupType.ManufacturerProprietary)
                        {
                            adaptID.SourceType = IdSourceTypeEnum.GLN;
                            adaptID.Source = isoLinkGroup.ManufacturerGLN;
                        }
                        uniqueIDs.Add(adaptID);
                    }
                }
            }

            //2. Add a link for the current ISOXML import
            //This temporary reference serves as a mapping back to the imported file
            //The values are omitted from the LinkList.xml during any export.
            uniqueIDs.Add(ImportCurrentISOLink(isoObjectIdRef));

            return uniqueIDs;
        }

        public static UniqueId ImportCurrentISOLink(string id)
        {
            return new UniqueId
            {
                Id = id,
                Source = IsoSource,
                IdType = IdTypeEnum.String,
                SourceType = IdSourceTypeEnum.URI
            };
        }
        #endregion Import
    }
}
