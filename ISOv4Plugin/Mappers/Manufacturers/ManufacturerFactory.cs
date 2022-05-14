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
