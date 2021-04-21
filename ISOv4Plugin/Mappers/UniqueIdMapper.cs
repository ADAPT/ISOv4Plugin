/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public class UniqueIdMapper
    {
        private const string AgGatewayGLN = "1100046503738";
        private const string UnclassifiedLinkGroupDesignator = "ADAPT_UniqueIds";
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
                else if (uid.IdType == IdTypeEnum.URI)
                {
                    //Add to global URI list
                    linkGroup = LinkList.LinkGroups.FirstOrDefault(lg => lg.LinkGroupType == ISOEnumerations.ISOLinkGroupType.UniqueResolvable);
                    if (linkGroup == null)
                    {
                        linkGroup = new ISOLinkGroup() { LinkGroupType = ISOEnumerations.ISOLinkGroupType.UniqueResolvable, LinkGroupDesignator = "URIs", Links = new List<ISOLink>() };
                        linkGroup.LinkGroupId = BaseMapper.GenerateId(0, "LGP", _lgpID++); //Special ID invocation here due to class relationships
                        LinkList.LinkGroups.Add(linkGroup);
                    }
                }
                else
                {
                    //Save to the AgGateway GLN.    The link source is the designator
                    linkGroup = LinkList.LinkGroups.FirstOrDefault(lg => lg.LinkGroupType == ISOEnumerations.ISOLinkGroupType.ManufacturerProprietary && lg.ManufacturerGLN == AgGatewayGLN);
                    if (linkGroup == null)
                    {
                        linkGroup = new ISOLinkGroup() { LinkGroupType = ISOEnumerations.ISOLinkGroupType.ManufacturerProprietary, ManufacturerGLN = AgGatewayGLN, LinkGroupDesignator = UnclassifiedLinkGroupDesignator, Links = new List<ISOLink>() };
                        linkGroup.LinkGroupId = BaseMapper.GenerateId(0, "LGP", _lgpID++); //Special ID invocation here due to class relationships
                        LinkList.LinkGroups.Add(linkGroup);
                    }
                }

                //Add the link
                ISOLink link = new ISOLink();
                link.ObjectIdRef = isoIDRef;
                link.LinkValue = uid.Id;
                link.LinkDesignator = uid.Source;
                linkGroup.Links.Add(link);
            }
        }

        public List<string> ExportContextItems(List<ContextItem> contextItems, string isoIDRef, string groupName, string prefix)
        {
            List<string> errors = new List<string>();
            ISOLinkGroup linkGroup = LinkList.LinkGroups.FirstOrDefault(lg => lg.LinkGroupType == ISOEnumerations.ISOLinkGroupType.ManufacturerProprietary && lg.ManufacturerGLN == AgGatewayGLN && lg.LinkGroupDesignator == groupName);
            if (linkGroup == null)
            {
                linkGroup = new ISOLinkGroup() { LinkGroupType = ISOEnumerations.ISOLinkGroupType.ManufacturerProprietary, ManufacturerGLN = AgGatewayGLN, LinkGroupDesignator = groupName, Links = new List<ISOLink>() };
                linkGroup.LinkGroupId = BaseMapper.GenerateId(0, "LGP", _lgpID++); //Special ID invocation here due to class relationships
                LinkList.LinkGroups.Add(linkGroup);
            }

            foreach (ContextItem rootContextItem in contextItems)
            {
                Tuple<List<ISOLink>, List<string>> linksWithErrors = GetLinksForContextItem(isoIDRef, rootContextItem, prefix);
                linkGroup.Links.AddRange(linksWithErrors.Item1);
                errors.AddRange(linksWithErrors.Item2);
            }
            return errors;
        }

        private Tuple<List<ISOLink>, List<string>> GetLinksForContextItem(string isoIDRef, ContextItem item, string prefix)
        {
            List<ISOLink> output = new List<ISOLink>();
            List<string> errors = new List<string>();
            prefix = string.Concat(prefix, item.Code);
            if (item.NestedItems != null && item.NestedItems.Any())
            {
                prefix = string.Concat(prefix, ".");
                foreach (ContextItem nestedItem in item.NestedItems)
                {
                    output.AddRange(GetLinksForContextItem(isoIDRef, nestedItem, prefix).Item1);
                }
            }
            else
            {
                string value = $"{prefix}|{item.Value}";
                if (value.Length <= 255) //We can only export links up to 255 characters
                {
                    output.Add(new ISOLink() { LinkValue = value, ObjectIdRef = isoIDRef });
                }
                else
                {
                    errors.Add($"Value too long.  Context item skipped. {value}");
                }
            }
            return new Tuple<List<ISOLink>, List<string>>(output, errors);
        }
        #endregion Export

        #region Import

        public IEnumerable<UniqueId> ImportUniqueIDs(string isoObjectIdRef)
        {
            List<UniqueId> uniqueIDs = new List<UniqueId>();

            //1. Add any matching link in the LinkList.xml
            if (LinkList != null)
            {
                foreach (ISOLinkGroup isoLinkGroup in LinkList.LinkGroups.Where(lg => lg.LinkGroupDesignator != null && !lg.LinkGroupDesignator.Contains("ADAPT_Context_Items")))
                {
                    ISOLink link = isoLinkGroup.Links.FirstOrDefault(l => l.ObjectIdRef == isoObjectIdRef);
                    if (link != null)
                    {
                        UniqueId adaptID = new UniqueId();
                        adaptID.Id = link.LinkValue;
                        adaptID.Source = link.LinkDesignator;

                        if (isoLinkGroup.LinkGroupType == ISOEnumerations.ISOLinkGroupType.UUID)
                        {
                            adaptID.IdType = IdTypeEnum.UUID;
                        }
                        else if (isoLinkGroup.LinkGroupType == ISOEnumerations.ISOLinkGroupType.UniqueResolvable)
                        {
                            adaptID.IdType = IdTypeEnum.URI;
                        }
                        else if (isoLinkGroup.LinkGroupType == ISOEnumerations.ISOLinkGroupType.ManufacturerProprietary &&
                                 (isoLinkGroup.LinkGroupDesignator != UnclassifiedLinkGroupDesignator ||
                                 isoLinkGroup.ManufacturerGLN != AgGatewayGLN)) //We'll handle the "unclassified" links under AgGateway's GLN below in the else
                        {
                            adaptID.SourceType = IdSourceTypeEnum.GLN;
                            if (!string.IsNullOrEmpty(isoLinkGroup.ManufacturerGLN))
                            {
                                adaptID.Source = isoLinkGroup.ManufacturerGLN; //We'll overwrite the source with the GLN if supplied
                            }
                        }
                        else
                        {
                            adaptID.IdType = IdTypeEnum.String;
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

        /// <summary>
        /// Returns a list of ContextItems from the LinkGroup matching the object ID
        /// </summary>
        /// <param name="isoObjectIdRef"></param>
        /// <param name="linkGroupDescription">If supplied, the link grup description must match</param>
        /// <returns></returns>
        public List<ContextItem> ImportContextItems(string isoObjectIdRef, string linkGroupDescription)
        {
            List<ContextItem> outputItems = new List<ContextItem>();
            if (LinkList != null)
            {
                foreach (ISOLinkGroup isoLinkGroup in LinkList.LinkGroups.Where(lg => lg.LinkGroupDesignator != null && lg.LinkGroupDesignator.Equals(linkGroupDescription)))
                {
                    foreach (ISOLink link in isoLinkGroup.Links.Where(link => link.ObjectIdRef == isoObjectIdRef))
                    {
                        string[] codesVsValue = link.LinkValue.Split('|');
                        string code = codesVsValue[0];
                        string value = codesVsValue[1];
                        List<string> codeHierarchy = code.Split('.').ToList();

                        ContextItem parent = null;
                        ContextItem item = FindContextItem(outputItems, new List<string>() { codeHierarchy[0] });
                        if (item == null)
                        {
                            item = new ContextItem();
                            outputItems.Add(item); //This is the root item that gets exported from the method
                        }

                        while (codeHierarchy.Any())
                        {
                            //Remove the first code
                            item.Code = codeHierarchy[0];
                            codeHierarchy.RemoveAt(0);

                            if (codeHierarchy.Any())
                            {
                                //There is another level after this.   Create a nesting and reset the item.
                                if (item.NestedItems == null)
                                {
                                    item.NestedItems = new List<ContextItem>();
                                }
                                parent = item;
                                item = FindContextItem(parent.NestedItems, new List<string>() { codeHierarchy[0] });
                                if (item == null)
                                {
                                    item = new ContextItem();
                                    item.Code = codeHierarchy[0];
                                    parent.NestedItems.Add(item);
                                }
                            }
                            else
                            {
                                //This is the final level of the code hierarchy.
                                //Add the actual value and to the output list
                                item.Value = value;
                                parent = null;
                            }
                        }
                    }
                }
            }
            return outputItems;
        }

        private ContextItem FindContextItem(List<ContextItem> createdItems, List<string> codeHierarchy)
        {
            foreach (ContextItem item in createdItems)
            {
                for (int i = 0; i < codeHierarchy.Count; i++)
                {
                    string code = codeHierarchy[i];
                    if (item.Code != code)
                    {
                        break;
                    }
                    return item;
                }
            }
            return null;
        }
        #endregion Import
    }
}
