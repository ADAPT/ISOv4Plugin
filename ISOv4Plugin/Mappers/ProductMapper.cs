/*
* ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Mappers.Manufacturers;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IProductMapper
    {
        IEnumerable<ISOProduct> ExportProducts(IEnumerable<Product> adaptProducts);
        ISOProduct ExportProduct(Product adaptProduct);

        IEnumerable<Product> ImportProducts(IEnumerable<ISOProduct> isoProducts);
        Product ImportProduct(ISOProduct isoProduct);
    }

    public class ProductMapper : BaseMapper, IProductMapper
    {
        private readonly IManufacturer _manufacturer;

        public ProductMapper(TaskDataMapper taskDataMapper, ProductGroupMapper productGroupMapper) : base(taskDataMapper, "PDT")
        {
            _productGroupMapper = productGroupMapper;

            _manufacturer = ManufacturerFactory.GetManufacturer(taskDataMapper);
        }

        #region Export
        ProductGroupMapper _productGroupMapper;
        public IEnumerable<ISOProduct> ExportProducts(IEnumerable<Product> adaptProducts)
        {
            List <ISOProduct> isoProducts = new List<ISOProduct>();
            //Add all the products
            foreach (Product adaptProduct in adaptProducts)
            {
                ISOProduct product = ExportProduct(adaptProduct);
                isoProducts.Add(product);
            }

            //Fill in detail on the product mixes
            if (adaptProducts.OfType<MixProduct>().Any())
            {
                foreach (MixProduct adaptMixProduct in adaptProducts.OfType<MixProduct>())
                {
                    //Find the ISO Product
                    ISOProduct isoMixProduct = isoProducts.Single(p => p.ProductId == TaskDataMapper.InstanceIDMap.GetISOID(adaptMixProduct.Id.ReferenceId));

                    foreach (ProductComponent component in adaptMixProduct.ProductComponents)
                    {
                        //Components may map to either an ingredient or a product
                        //See comments at ProductComponent in ADAPT repo.
                        Product adaptProduct = DataModel.Catalog.Products.FirstOrDefault(i => i.Id.ReferenceId == component.IngredientId);
                        if (adaptProduct != null)
                        {
                            ExportProductRelation(isoProducts, adaptProduct.Description, component.Quantity, isoMixProduct);
                        }
                        else
                        {
                            Ingredient adaptIngredient = DataModel.Catalog.Ingredients.FirstOrDefault(i => i.Id.ReferenceId == component.IngredientId);
                            if (adaptIngredient != null)
                            {
                                ExportProductRelation(isoProducts, adaptIngredient.Description, component.Quantity, isoMixProduct);
                            }
                        }
                    }

                    //Total Quantity
                    isoMixProduct.MixtureRecipeQuantity = adaptMixProduct.TotalQuantity.AsIntViaMappedDDI(RepresentationMapper);

                    //Quantity DDI
                    int? ddi = RepresentationMapper.Map(adaptMixProduct.TotalQuantity.Representation);
                    if (ddi.HasValue)
                    {
                        isoMixProduct.QuantityDDI = ddi.Value.AsHexDDI();
                    }
                }

            }
            return isoProducts;
        }

        private void ExportProductRelation(List<ISOProduct> isoProducts,
                                           string adaptDescription,
                                           ApplicationDataModel.Representations.NumericRepresentationValue quantity,
                                           ISOProduct targetMixProduct)
        {

            ISOProduct componentProduct = isoProducts.FirstOrDefault(p => p.ProductDesignator == adaptDescription); //Matches on name; assumes all ingredients are also products
            if (componentProduct != null)
            {
                //Create PRNs if we can match to pre-existing products
                ISOProductRelation relation = new ISOProductRelation();
                relation.ProductIdRef = componentProduct.ProductId;
                relation.QuantityValue = quantity.AsIntViaMappedDDI(RepresentationMapper);
                targetMixProduct.ProductRelations.Add(relation);
            }
        }

        public ISOProduct ExportProduct(Product adaptProduct)
        {
            ISOProduct isoProduct = new ISOProduct();

            //ID
            string productID = adaptProduct.Id.FindIsoId(XmlPrefix) ?? GenerateId();
            isoProduct.ProductId = productID;
            if (!ExportIDs(adaptProduct.Id, productID))
            {
                string preExistingID = TaskDataMapper.InstanceIDMap.GetISOID(adaptProduct.Id.ReferenceId);
                if (preExistingID.StartsWith("CVT"))
                {
                    ISOCropVariety cvt = ISOTaskData.ChildElements.OfType<ISOCropType>().SelectMany(c => c.CropVarieties).FirstOrDefault(v => v.CropVarietyId == preExistingID);
                    if (cvt != null)
                    {
                        //Fill the product ID on the variety now that we have one
                        cvt.ProductIdRef = productID;
                    }
                }
                //CVT becomes PDT in the mapping
                TaskDataMapper.InstanceIDMap.ReplaceISOID(adaptProduct.Id.ReferenceId, productID);
            }

            //Context Items
            //The 1.1, etc. convention below for PackagedProduct and PackagedProductInstance is internal to this plugin.
            //As LinkList.xml items classified in ManufacturerGLN LGP's, the data carries a proprietary definition that,
            //in this case, is internal to the Import/Export methods in this class and serves
            //no purpose other than ensuring exported data is reimported in its same form.   If in future there is a need to expose
            //other product data in this way, we need simply alter these to methods to allow for the same.
            ExportContextItems(adaptProduct.ContextItems, productID, "ADAPT_Context_Items:Product");
            int packagedProductIndex = 0;
            foreach (var packagedProduct in DataModel.Catalog.PackagedProducts.Where(pp => pp.ProductId == adaptProduct.Id.ReferenceId))
            {
                ExportContextItems(packagedProduct.ContextItems, productID, "ADAPT_Context_Items:PackagedProduct", $"{(++packagedProductIndex).ToString()}.");
                int packagedProductInstanceIndex = 0;
                foreach (var packagedProductInstance in DataModel.Catalog.PackagedProductInstances.Where(ppi => ppi.PackagedProductId == packagedProduct.Id.ReferenceId))
                {
                    ExportContextItems(packagedProductInstance.ContextItems, productID, "ADAPT_Context_Items:PackagedProductInstance", $"{packagedProductIndex.ToString()}.{(++packagedProductInstanceIndex).ToString()}.");
                }
            }

            //Designator
            isoProduct.ProductDesignator = adaptProduct.Description;

            //Product Group
            string groupName = Enum.GetName(typeof(ProductTypeEnum), adaptProduct.ProductType);
            ISOProductGroup group = _productGroupMapper.ExportProductGroup(groupName, false);
            isoProduct.ProductGroupRef = group.ProductGroupId;

            //Type
            switch (adaptProduct.ProductType)
            {
                case ProductTypeEnum.Mix:
                    isoProduct.ProductType = ISOProductType.Mixture;
                    break;
                default:
                    isoProduct.ProductType = ISOProductType.Single;
                    break;
            }

            //Density
            if (adaptProduct.Density != null)
            {
                UnitOfMeasure uom = adaptProduct.Density.Value.UnitOfMeasure;
                int value = (int)adaptProduct.Density.Value.Value; //Assumes values are in appropriate units already
                if (uom.Code == "mg1l-1")
                {
                    isoProduct.DensityMassPerVolume = value;
                }
                else if (uom.Code == "g1count-1")
                {
                    isoProduct.DensityMassPerCount = value;
                }
                else if (uom.Code == "ml1count-1")
                {
                    isoProduct.DensityVolumePerCount = value;
                }
            }

            return isoProduct;
        }

        #endregion Export 

        #region Import

        public IEnumerable<Product> ImportProducts(IEnumerable<ISOProduct> isoProducts)
        {
            //Import products
            List<Product> adaptProducts = new List<Product>();
            foreach (ISOProduct isoProduct in isoProducts)
            {
                Product adaptProduct = ImportProduct(isoProduct);
                adaptProducts.Add(adaptProduct);
            }

            return adaptProducts;
        }

        public Product ImportProduct(ISOProduct isoProduct)
        {
            //First check if we've already created a matching seed product from the crop type
            Product product = DataModel.Catalog.Products.FirstOrDefault(p => p.ProductType == ProductTypeEnum.Variety && p.Description == isoProduct.ProductDesignator)
                ?? CreateNewProductInstance(isoProduct);

            //ID
            if (!ImportIDs(product.Id, isoProduct.ProductId))
            {
                //Replace the CVT id with the PDT id in the mapping            
                TaskDataMapper.InstanceIDMap.ReplaceISOID(product.Id.ReferenceId, isoProduct.ProductId);
            }

            //Context Items
            product.ContextItems = ImportContextItems(isoProduct.ProductId, "ADAPT_Context_Items:Product", isoProduct);
            ImportPackagedProductClasses(isoProduct, product);


            //Description
            product.Description = isoProduct.ProductDesignator;

            //Mixes
            if (isoProduct.ProductRelations.Any())
            {
                if (product.ProductComponents == null)
                {
                    product.ProductComponents = new List<ProductComponent>();
                }

                foreach (ISOProductRelation prn in isoProduct.ProductRelations)
                {
                    //Find the product referenced by the relation
                    ISOProduct isoComponent = ISOTaskData.ChildElements.OfType<ISOProduct>().FirstOrDefault(p => p.ProductId == prn.ProductIdRef);

                    if (isoComponent != null) //Skip PRN if PRN@A doesn't resolve to a product
                    {
                        //Find or create the product to match the component
                        Product adaptProduct = DataModel.Catalog.Products.FirstOrDefault(i => i.Id.FindIsoId() == isoComponent.ProductId);
                        if (adaptProduct == null)
                        {
                            adaptProduct = new GenericProduct();
                            adaptProduct.Description = isoComponent.ProductDesignator;
                            DataModel.Catalog.Products.Add(adaptProduct);
                        }

                        //Create a component for this ingredient
                        ProductComponent component = new ProductComponent() { IngredientId = adaptProduct.Id.ReferenceId, IsProduct = true };
                        if (!string.IsNullOrEmpty(isoComponent.QuantityDDI))
                        {
                            component.Quantity = prn.QuantityValue.AsNumericRepresentationValue(isoComponent.QuantityDDI, RepresentationMapper);
                        }
                        product.ProductComponents.Add(component);
                    }
                    else
                    {
                        TaskDataMapper.AddError($"Product relation with quantity {prn.QuantityValue} ommitted for product {isoProduct.ProductId} due to no ProductIdRef");
                    }
                }

                //Total Mix quantity
                if (isoProduct.MixtureRecipeQuantity.HasValue)
                {
                    MixProduct mixProduct = product as MixProduct;
                    mixProduct.TotalQuantity = isoProduct.MixtureRecipeQuantity.Value.AsNumericRepresentationValue(isoProduct.QuantityDDI, RepresentationMapper);
                }
            }

            //Density
            if (isoProduct.DensityMassPerCount.HasValue)
            {
                product.Density = isoProduct.DensityMassPerCount.Value.AsNumericRepresentationValue("007A", RepresentationMapper);
            }
            else if (isoProduct.DensityMassPerVolume.HasValue)
            {
                product.Density = isoProduct.DensityMassPerVolume.Value.AsNumericRepresentationValue("0079", RepresentationMapper);
            }
            else if (isoProduct.DensityVolumePerCount.HasValue)
            {
                product.Density = isoProduct.DensityVolumePerCount.Value.AsNumericRepresentationValue("007B", RepresentationMapper);
            }

            return product;
        }

        private Product CreateNewProductInstance(ISOProduct isoProduct)
        {
            // If there is a manufacturer defined attribute representing a crop name, use it
            string cropName = _manufacturer?.GetCropName(isoProduct);
            if (!string.IsNullOrWhiteSpace(cropName))
            {
                // New crop variety product
                var cropProduct = new CropVarietyProduct();
                cropProduct.ProductType = ProductTypeEnum.Variety;

                // Check if there is already Crop in ADAPT model
                Crop adaptCrop = TaskDataMapper.AdaptDataModel.Catalog.Crops.FirstOrDefault(x => x.Name.EqualsIgnoreCase(cropName));
                if (adaptCrop == null)
                {
                    // Create a new one
                    adaptCrop = new Crop();
                    adaptCrop.Name = cropName;
                    TaskDataMapper.AdaptDataModel.Catalog.Crops.Add(adaptCrop);
                }
                cropProduct.CropId = adaptCrop.Id.ReferenceId;
                return cropProduct;
            }

            Product product;
            //Type
            switch (isoProduct.ProductType)
            {
                case ISOProductType.Mixture:
                case ISOProductType.TemporaryMixture:
                    product = new MixProduct();
                    product.ProductType = ProductTypeEnum.Mix;
                    break;
                default:
                    product = new GenericProduct();
                    product.ProductType = ProductTypeEnum.Generic;
                    break;
            }
            return product;
        }

        /// <summary>
        /// Import any PackagedProduct & PackagedProductInstance classes as may be defined in the LinkList/ContextItems
        /// Any export process (above) will have named the context items with with integer prefixes so that we can identify
        /// the source object hierarchy.
        /// E.g., 1.1.Code|Value is the first PackagedProduct and the first PackagedProduct belonging to it.
        /// The 1.1, etc. convention is internal to this plugin.  As LinkList.xml items classified in ManufacturerGLN LGP's, the
        /// data carries a proprietary definition that, in this case, is internal to the Import/Export methods in this class and serves
        /// no purpose other than ensuring exported data is reimported in its same form.   If in future there is a need to expose
        /// other product data in this way, we need simply alter these to methods to allow for the same.
        /// </summary>
        /// <param name="isoProduct"></param>
        /// <param name="product"></param>
        private void ImportPackagedProductClasses(ISOProduct isoProduct, Product product)
        {
            List<ContextItem> ppContextItems = ImportContextItems(isoProduct.ProductId, "ADAPT_Context_Items:PackagedProduct");
            List<ContextItem> ppiContextItems = ImportContextItems(isoProduct.ProductId, "ADAPT_Context_Items:PackagedProductInstance");
            int packagedProductIndex = 1;
            ContextItem relevantPPContextItem = ppContextItems.FirstOrDefault(ci => ci.Code == packagedProductIndex.ToString());
            while (relevantPPContextItem != null)
            {
                //PackagedProduct
                PackagedProduct packagedProduct = new PackagedProduct();
                packagedProduct.ProductId = product.Id.ReferenceId;
                packagedProduct.ContextItems = relevantPPContextItem.NestedItems;
                packagedProduct.Description = packagedProduct.ContextItems.FirstOrDefault()?.Value;
                DataModel.Catalog.PackagedProducts.Add(packagedProduct);

                //PackagedProductInstance
                ContextItem containingContextItem = ppiContextItems.FirstOrDefault(ci => ci.Code == packagedProductIndex.ToString());
                if (containingContextItem != null)
                {
                    int packagedProductInstanceIndex = 1;
                    ContextItem relevantPPIContextItem = containingContextItem.NestedItems.FirstOrDefault(ci => ci.Code == packagedProductInstanceIndex.ToString());
                    while (relevantPPIContextItem != null)
                    {
                        PackagedProductInstance packagedProductInstance = new PackagedProductInstance();
                        packagedProductInstance.PackagedProductId = packagedProduct.Id.ReferenceId;
                        packagedProductInstance.ContextItems = relevantPPIContextItem.NestedItems;
                        packagedProductInstance.Description = packagedProductInstance.ContextItems.FirstOrDefault()?.Value;
                        DataModel.Catalog.PackagedProductInstances.Add(packagedProductInstance);

                        //Increment the counter and see if there is a PackagedProductInstance 1.2, etc.
                        int index = ++packagedProductInstanceIndex; 
                        relevantPPIContextItem = containingContextItem.NestedItems.FirstOrDefault(ci => ci.Code == index.ToString());
                    }
                }

                //Increment the counter and see if there is a PackagedProduct 2, etc.
                relevantPPContextItem = ppContextItems.FirstOrDefault(ci => ci.Code == (++packagedProductIndex).ToString());
            }
        }
        #endregion Import
    }
}
