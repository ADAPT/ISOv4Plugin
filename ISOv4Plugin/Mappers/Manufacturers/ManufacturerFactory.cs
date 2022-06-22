using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers.Manufacturers
{
    internal interface IManufacturer
    {
        string GetCropName(ISOElement isoElement);
        ProductFormEnum? GetProductForm(ISOProduct isoProduct);
        ProductTypeEnum? GetProductType(ISOProduct isoProduct);
        CategoryEnum? GetProductCategory(ISOProduct isoProduct);
        string GetProductManufacturer(ISOProduct isoProduct);

        IEnumerable<OperationData> PostProcessOperationData(TaskDataMapper taskDataMapper, IEnumerable<OperationData> operationDatas);
    }

    internal static class ManufacturerFactory
    {
        private const string CNHManufacturer = "CNH Industrial N.V.";

        public static IManufacturer GetManufacturer(TaskDataMapper taskDataMapper)
        {
            if (taskDataMapper.ISOTaskData.TaskControllerManufacturer.EqualsIgnoreCase(CNHManufacturer))
            {
                return new CNH();
            }
            return null;
        }
    }
}
