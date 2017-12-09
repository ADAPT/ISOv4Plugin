using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IProductGroupMapper
    {
        ISOProductGroup ExportProductGroup(string productGroupName, bool isCropType);
    }

    public class ProductGroupMapper : BaseMapper, IProductGroupMapper
    {
        public ProductGroupMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "PGP") { }

        public ISOProductGroup ExportProductGroup(string productGroupName, bool isCropType)
        {
            if (productGroupName == "Variety")
            {
                return this.ExportProductGroup("CropType", true);
            }
            else if (ISOTaskData.ChildElements.OfType<ISOProductGroup>().Any(g => g.ProductGroupDesignator == productGroupName))
            {
                //return the prexisting value
                return ISOTaskData.ChildElements.OfType<ISOProductGroup>().Single(g => g.ProductGroupDesignator == productGroupName);
            }
            else
            {
                ISOProductGroup pgp = new ISOProductGroup();

                //ID
                pgp.ProductGroupId = GenerateId();

                //Designator
                pgp.ProductGroupDesignator = productGroupName;

                //Type
                if (isCropType)
                {
                    pgp.ProductGroupType = ISOEnumerations.ISOProductGroupType.CropType;
                }
                else
                {
                    pgp.ProductGroupType = ISOEnumerations.ISOProductGroupType.ProductGroup;
                }

                //Add to the TaskData
                ISOTaskData.ChildElements.Add(pgp);

                return pgp;
            }
        }
    }
}
