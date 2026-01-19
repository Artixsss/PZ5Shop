using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using PZ5Shop.Models;

namespace PZ5Shop.Data
{
    public class DbService
    {
        public List<Products> GetProducts()
        {
            using (var db = new PZ5ShopDbEntities())
            {
                return db.Products.Include("Categories").ToList();
            }
        }

        public List<Categories> GetCategories()
        {
            using (var db = new PZ5ShopDbEntities())
            {
                return db.Categories.OrderBy(c => c.Name).ToList();
            }
        }

        public Users GetUserById(int id)
        {
            using (var db = new PZ5ShopDbEntities())
            {
                return db.Users.FirstOrDefault(u => u.Id == id);
            }
        }

        public Users GetUserByCredentials(string username, string password)
        {
            var hash = PasswordHasher.Hash(password);
            using (var db = new PZ5ShopDbEntities())
            {
                return db.Users.FirstOrDefault(u => u.Username == username && (u.PasswordHash == password || u.PasswordHash == hash));
            }
        }

        public bool UsernameExists(string username)
        {
            using (var db = new PZ5ShopDbEntities())
            {
                return db.Users.Any(u => u.Username == username);
            }
        }

        public bool EmailExists(string email)
        {
            using (var db = new PZ5ShopDbEntities())
            {
                return db.Users.Any(u => u.Email == email);
            }
        }

        public Users CreateUser(string lastName, string firstName, string middleName, string username, string email, string password)
        {
            var user = new Users
            {
                LastName = lastName,
                FirstName = firstName,
                MiddleName = middleName,
                Username = username,
                Email = email,
                PasswordHash = PasswordHasher.Hash(password),
                RegistrationDate = DateTime.Now
            };

            using (var db = new PZ5ShopDbEntities())
            {
                db.Users.Add(user);
                db.SaveChanges();
                return user;
            }
        }

        public List<Carts> GetCartItems(int userId)
        {
            using (var db = new PZ5ShopDbEntities())
            {
                return db.Carts.Include("Products").Where(c => c.UserId == userId).ToList();
            }
        }

        public void UpsertCartItem(int userId, int productId, int quantity)
        {
            using (var db = new PZ5ShopDbEntities())
            {
                var item = db.Carts.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);
                if (item == null)
                {
                    db.Carts.Add(new Carts { UserId = userId, ProductId = productId, Quantity = quantity });
                }
                else
                {
                    item.Quantity = quantity;
                }
                db.SaveChanges();
            }
        }

        public void RemoveCartItem(int userId, int productId)
        {
            using (var db = new PZ5ShopDbEntities())
            {
                var item = db.Carts.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);
                if (item != null)
                {
                    db.Carts.Remove(item);
                    db.SaveChanges();
                }
            }
        }

        public void ClearCart(int userId)
        {
            using (var db = new PZ5ShopDbEntities())
            {
                var items = db.Carts.Where(c => c.UserId == userId).ToList();
                if (items.Count > 0)
                {
                    db.Carts.RemoveRange(items);
                    db.SaveChanges();
                }
            }
        }

        public Orders CreateOrder(int userId, List<CartLine> lines)
        {
            using (var db = new PZ5ShopDbEntities())
            {
                var statusId = GetProcessingStatusId(db);
                var total = lines.Sum(l => l.UnitPrice * l.Quantity);

                var order = new Orders
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = total,
                    StatusId = statusId
                };

                db.Orders.Add(order);
                db.SaveChanges();

                foreach (var line in lines)
                {
                    var item = new OrderItems
                    {
                        OrderId = order.Id,
                        ProductId = line.ProductId,
                        Quantity = line.Quantity,
                        PriceAtOrder = line.UnitPrice
                    };
                    db.OrderItems.Add(item);
                }

                db.SaveChanges();
                return order;
            }
        }

        public List<Orders> GetOrdersForUser(int userId)
        {
            using (var db = new PZ5ShopDbEntities())
            {
                return db.Orders
                    .Include("OrderStatus")
                    .Include("OrderItems")
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();
            }
        }

        public List<OrderItems> GetOrderItems(int orderId)
        {
            using (var db = new PZ5ShopDbEntities())
            {
                return db.OrderItems.Include("Products").Where(i => i.OrderId == orderId).ToList();
            }
        }

        private int GetProcessingStatusId(PZ5ShopDbEntities db)
        {
            var status = db.OrderStatus.FirstOrDefault(s => s.Name == "В обработке");
            if (status != null)
            {
                return status.Id;
            }

            var fallback = db.OrderStatus.FirstOrDefault();
            return fallback != null ? fallback.Id : 1;
        }
    }
}
