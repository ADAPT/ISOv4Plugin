//While ISO1173-10 (and this plugin) supports vector prescriptions, industry adoption is limited.
//Raster prescriptions are commonly used.

using AgGateway.ADAPT.ApplicationDataModel;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ApplicationDataModel.Documents;

class RasterPrescriptions
{
    public static void ExportExamplePrescription()
    {
        var adm = new ApplicationDataModel
        {
            Catalog = new Catalog
            {
                Products = new List<Product>
                {
                    new GenericProduct { Description = "Product 1", Category = CategoryEnum.Fertilizer, ProductType = ProductTypeEnum.Fertilizer, Form = ProductFormEnum.Solid, Status = ProductStatusEnum.Active },
                    new GenericProduct { Description = "Product 2", Category = CategoryEnum.Fungicide, ProductType = ProductTypeEnum.Chemical, Form = ProductFormEnum.Liquid, Status = ProductStatusEnum.Active },
                    new GenericProduct { Description = "Product 3", Category = CategoryEnum.Herbicide, ProductType = ProductTypeEnum.Chemical, Form = ProductFormEnum.Liquid, Status = ProductStatusEnum.Active },
                    new GenericProduct { Description = "Product 4", Category = CategoryEnum.Insecticide, ProductType = ProductTypeEnum.Chemical, Form = ProductFormEnum.Liquid, Status = ProductStatusEnum.Active }
                },
            },
            Documents = new Documents()
        };

        var rx = new RasterGridPrescription
        {
            CellHeight = new NumericRepresentationValue { Value = new NumericValue(new UnitOfMeasure(), 0.001) }, //In ISO1173-10, cell size is listed in degrees of lat/lon
            CellWidth = new NumericRepresentationValue { Value = new NumericValue(new UnitOfMeasure(), 0.002) },
            ColumnCount = 10,
            RowCount = 5,
            OperationType = OperationTypeEnum.SowingAndPlanting,
            Origin = new Point { X = -87.60, Y = 41.88 }, //SW Corner
            RxProductLookups = adm.Catalog.Products.Select(x => new RxProductLookup { ProductId = x.Id.ReferenceId, UnitOfMeasure = new UnitOfMeasure { } }).ToList(),
            ProductIds = adm.Catalog.Products.Select(x => x.Id.ReferenceId).ToList(),
            Rates = new List<RxCellLookup>(),
            Description = "RasterRx"
        };

        for (int h = 0; h < rx.RowCount; h++) //Rates in ADAPT/ISOXML Raster prescriptions are in a simple list, starting at the SW cell and going row-by-row left to right
        {
            for (int w = 0; w < rx.ColumnCount; w++)
            {
                var rates = rx.RxProductLookups.Select((x, i) => new RxRate
                {
                    Rate = h * rx.ColumnCount + w + i * 100, //Dummy rates for the example for illustrative purposes
                    RxProductLookupId = x.Id.ReferenceId
                }).ToList();
                rx.Rates.Add(new RxCellLookup { RxRates = rates });
            }
        }

        adm.Catalog.Prescriptions = new List<Prescription> { rx };

        //The plugin is currently only recognizes Prescriptions mapped through Work Items and Work Item Operations
        //See TaskDataMapper.cs at "Tasks" export
        var workItemOperation = new WorkItemOperation
        {
            PrescriptionId = rx.Id.ReferenceId
        };
        adm.Documents.WorkItemOperations = new List<WorkItemOperation> { workItemOperation };

        var workItem = new WorkItem
        {
            WorkItemOperationIds = new List<int>() { workItemOperation.Id.ReferenceId }
        };
        adm.Documents.WorkItems = new List<WorkItem> { workItem };


        var plugin = new AgGateway.ADAPT.ISOv4Plugin.Plugin();
        var properties = new AgGateway.ADAPT.ApplicationDataModel.ADM.Properties();
        properties.SetProperty("GridType", "2"); //ISOXML has 2 formats of grid prescriptions. 2 is the default if this property is omitted.
        plugin.Export(adm, System.IO.Directory.GetCurrentDirectory(), properties); //Exports to location of the Examples binary
    }
}