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
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

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
        public ProductMapper(TaskDataMapper taskDataMapper, ProductGroupMapper productGroupMapper) : base(taskDataMapper, "PDT")
        {
            _productGroupMapper = productGroupMapper;
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
                        Ingredient ingredient = DataModel.Catalog.Ingredients.FirstOrDefault(i => i.Id.ReferenceId == component.IngredientId);
                        ISOProduct componentProduct = isoProducts.FirstOrDefault(p => p.ProductDesignator == ingredient.Description); //Matches on name; assumes all ingredients are also products
                        if (componentProduct != null)
                        {
                            //Create PRNs if we can match to pre-existing products
                            ISOProductRelation relation = new ISOProductRelation();
                            relation.ProductIdRef = componentProduct.ProductId;
                            relation.QuantityValue = component.Quantity.AsLongViaMappedDDI(RepresentationMapper); 
                            isoMixProduct.ProductRelations.Add(relation);
                        }
                    }

                    //Total Quantity
                    isoMixProduct.MixtureRecipeQuantity = adaptMixProduct.TotalQuantity.AsLongViaMappedDDI(RepresentationMapper);

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

        public ISOProduct ExportProduct(Product adaptProduct)
        {
            ISOProduct isoProduct = new ISOProduct();

            //ID
            string id = adaptProduct.Id.FindIsoId(XmlPrefix) ?? GenerateId();
            isoProduct.ProductId = id;
            if (!ExportIDs(adaptProduct.Id, id))
            {
                //CVT > PDT in the mapping
                TaskDataMapper.InstanceIDMap.ReplaceISOID(adaptProduct.Id.ReferenceId, id);
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
                long value = (long)adaptProduct.Density.Value.Value; //Assumes values are in appropriate units already
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
            Product product = DataModel.Catalog.Products.FirstOrDefault(p => p.ProductType == ProductTypeEnum.Variety && p.Description == isoProduct.ProductDesignator);

            //If not, create a new product
            if (product == null)
            {
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
            }

            //ID
            if (!ImportIDs(product.Id, isoProduct.ProductId))
            {
                //Replace the CVT id with the PDT id in the mapping            
                TaskDataMapper.InstanceIDMap.ReplaceISOID(product.Id.ReferenceId, isoProduct.ProductId);
            }

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

                    //Find or create the active ingredient to match the component
                    Ingredient ingredient = DataModel.Catalog.Ingredients.FirstOrDefault(i => i.Id.FindIsoId() == isoComponent.ProductId);
                    if (ingredient == null)
                    {
                        ingredient = new ActiveIngredient();
                        ingredient.Description = isoComponent.ProductDesignator;
                        DataModel.Catalog.Ingredients.Add(ingredient);
                    }
                    
                    //Create a component for this ingredient
                    ProductComponent component = new ProductComponent() { IngredientId = ingredient.Id.ReferenceId };
                    if (!string.IsNullOrEmpty(isoComponent.QuantityDDI))
                    {
                        component.Quantity = prn.QuantityValue.AsNumericRepresentationValue(isoComponent.QuantityDDI, RepresentationMapper);
                    }
                    product.ProductComponents.Add(component);
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

        #endregion Import
    }
}
