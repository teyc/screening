using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace RinCalc.Db
{
    public partial class ShoppingCart
    {
        public static int GetCurrentCustomerId(HttpRequestBase req)
        {
            return Convert.ToInt32(req["customerId"]);
        }

        public static ShoppingCart Get(ModelsContainer ctx, int customerId)
        {
            var cart = (from c in ctx.ShoppingCarts
                        where c.CustomerId == customerId && c.IsCurrent
                        select c).SingleOrDefault();

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    IsCurrent = true,
                    CustomerId = customerId,
                };
                ctx.ShoppingCarts.Add(cart);
                ctx.SaveChanges();
            }

            return cart;
        }

        public static IEnumerable<dynamic> GetPurchaseHistory(string criteria)
        {
            ModelsContainer models = new ModelsContainer();
            using (var conn = models.Database.Connection)
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }

                var command = conn.CreateCommand();
                var stmt = @"SELECT CUSTOMERS.FULLNAME, SHOPPINGCARTS.TOTALPRICE FROM
CUSTOMERS INNER JOIN SHOPPINGCARTS ON CUSTOMERS.ID = SHOPPINGCARTS.CUSTOMERID
WHERE SHOPPINGCARTS.IsCurrent = 0";
                if (criteria.Length > 0)
                {
                    stmt += " AND " + criteria;
                }
                command.CommandText = stmt;

                var dr = command.ExecuteReader();

                dynamic row = new ExpandoObject();
                while (dr != null && dr.Read())
                {
                    row.FullName = dr["FULLNAME"];
                    row.TotalPrice = dr["TOTALPRICE"];
                    yield return row;
                }
            }
        }
    }
}