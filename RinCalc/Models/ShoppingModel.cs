using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RinCalc.Db;

namespace RinCalc.Models
{
    public class ShoppingModel
    {
        public ShoppingCart ShoppingCart { get; set; }

        public IEnumerable<Product> Products { get; set; }

        public Customer CurrentCustomer { get; set; }

        internal void AddProducts(ModelsContainer ctx, IEnumerable<string> addProductKeys)
        {
            foreach (var key in addProductKeys)
            {
                string productPrice = key.Substring("addProduct-".Length);
                int productId = Convert.ToInt32(productPrice.Split('-')[0]);
                float price = Convert.ToSingle(productPrice.Split('-')[1]);
                var product = ctx.Products.SingleOrDefault(p => p.Id == productId);
                if (product != null)
                {
                    ShoppingCart.Items.Add(new ShoppingCartItem 
                    {
                        ProductId = productId,
                        ShoppingCartId = this.ShoppingCart.Id,                        
                    });
                    ShoppingCart.TotalPrice += price;
                }
            }
        }

        internal void DeleteCartItems(ModelsContainer ctx, IEnumerable<string> deleteCartKeys)
        {
            foreach (var key in deleteCartKeys)
            {
                string itemPrice = key.Substring("deleteCart-".Length);
                int itemId = Convert.ToInt32(itemPrice.Split('-')[0]);
                float price = Convert.ToSingle(itemPrice.Split('-')[1]);

                var item = ShoppingCart.Items.SingleOrDefault(p => p.Id == itemId);
                if (item != null)
                {
                    ShoppingCart.Items.Remove(item);
                    ctx.ShoppingCartItems.Remove(item);
                }
                ShoppingCart.TotalPrice -= price;
            }
        }

        internal void Save(ModelsContainer ctx)
        {
            ctx.SaveChanges();
        }

        internal void MakePurchase(ModelsContainer ctx)
        {
            this.ShoppingCart.IsCurrent = false;            
        }
    }
}