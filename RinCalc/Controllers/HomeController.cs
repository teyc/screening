using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RinCalc.Controllers
{
    using RinCalc.Db;
    using RinCalc.Models;

    [Route("{action=index}")]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Route("Invoice/{invoiceId}")]
        public ActionResult Invoice(int invoiceId)
        {
            ModelsContainer ctx = new ModelsContainer();
            var cart = ctx.ShoppingCarts.SingleOrDefault(i => i.Id == invoiceId);
            this.ViewBag.ShoppingCart = cart;
            return View();
        }

        public ActionResult ShoppingCart()
        {
            ModelsContainer ctx = new ModelsContainer();
            ShoppingModel model = GetShoppingModel(ctx);

            return View(model);
        }

        public ActionResult UpdateShoppingCart()
        {
            ModelsContainer ctx = new ModelsContainer();

            ShoppingModel model = GetShoppingModel(ctx);

            var addProductKeys = this.Request.Form.AllKeys.Where(key => key.StartsWith("addProduct-"));
            var deleteCartKeys = this.Request.Form.AllKeys.Where(key => key.StartsWith("deleteCart-"));
            model.AddProducts(ctx, addProductKeys);
            model.DeleteCartItems(ctx, deleteCartKeys);

            if (this.Request.Form["makePurchase"] != null)
            {
                model.MakePurchase(ctx);
                model.Save(ctx);
                return Redirect("Invoice/" + model.ShoppingCart.Id);
            }
            else
            {
                model.Save(ctx);
                return Redirect("ShoppingCart?customerId=" + model.ShoppingCart.CustomerId);
            }
        }

        public ActionResult PurchaseHistory()
        {
            string criteria = Request["criteria"];
            ViewBag.History = Db.ShoppingCart.GetPurchaseHistory(criteria).ToList();
            return View();
        }

        public ActionResult RevertDatabase()
        {
            System.IO.File.Copy(
                Server.MapPath("~/App_Data/RinCalc.sample.sdf"),
                Server.MapPath("~/App_Data/RinCalc.sdf"), 
                overwrite:true);
            return Redirect("/?reverted=1");
        }

        public ActionResult Welcome()
        {
            return View();
        }

        private ShoppingModel GetShoppingModel(ModelsContainer ctx)
        {
            ShoppingModel model = new ShoppingModel();

            var customerId = Db.ShoppingCart.GetCurrentCustomerId(Request);
            model.ShoppingCart = Db.ShoppingCart.Get(ctx, customerId);
            model.Products = ctx.Products;
            model.CurrentCustomer = ctx.Customers.Single(c => c.Id == customerId);
            return model;
        }

    }

}