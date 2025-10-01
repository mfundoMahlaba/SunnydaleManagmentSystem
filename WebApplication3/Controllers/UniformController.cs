using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using WebApplication3.Models;
using WebApplication3.ViewModels;
using WebApplication3.Services;
using Stripe;
using Stripe.Checkout;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Diagnostics;

namespace WebApplication3.Controllers
{
    public class UniformController : Controller
    {
        private readonly SchoolDbContext _context;
        private const string CartSessionKey = "Cart";
       

        public UniformController()
        {
            _context = new SchoolDbContext();
        }

        // Helper methods for cart access
        private List<CartItemVm> GetCart()
        {
            return Session[CartSessionKey] as List<CartItemVm> ?? new List<CartItemVm>();
        }

        private void SaveCart(List<CartItemVm> cart)
        {
            Session[CartSessionKey] = cart;
        }

        // Show available uniforms
        public ActionResult Index()
        {
            var items = _context.UniformItems.ToList();

            // Get cart from session using consistent key
            var cart = GetCart();
            ViewBag.CartCount = cart?.Sum(c => c.Quantity) ?? 0;

            return View(items);
        }

        // Add item to cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int id, int quantity = 1, string size = "M")
        {
            if (quantity < 1) quantity = 1;

            var item = _context.UniformItems.Find(id);
            if (item == null) return HttpNotFound();

            var cart = GetCart();

            if (item.Stock < quantity)
            {
                TempData["Message"] = $"There are only {item.Stock} units of {item.Name} available.";
                return RedirectToAction("Index");
            }

            var existing = cart.FirstOrDefault(c => c.ItemId == id && c.Size == size);
            if (existing != null)
            {
                int requestedTotal = existing.Quantity + quantity;
                if (requestedTotal > item.Stock)
                {
                    TempData["Message"] = $"Cannot add {quantity} more. Only {item.Stock - existing.Quantity} left.";
                    return RedirectToAction("Index");
                }
                existing.Quantity = requestedTotal;
            }
            else
            {
                cart.Add(new CartItemVm
                {
                    ItemId = item.Id,
                    Name = item.Name,
                    UnitPrice = item.Price,
                    Quantity = quantity,
                    ImagePath = item.ImagePath,
                    Size = size
                });
            }

            SaveCart(cart);
            TempData["Message"] = $"{quantity} × {item.Name} ({size}) added to cart.";
            return RedirectToAction("ViewCart");
        }

        // View cart
        public ActionResult ViewCart()
        {
            var cart = GetCart();
            return View(cart);
        }

        // Remove item from cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int id, string size)
        {
            var cart = GetCart();
            var itemToRemove = cart.FirstOrDefault(c => c.ItemId == id && c.Size == size);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCart(cart);
                TempData["Message"] = "Item removed from cart.";
            }

            return RedirectToAction("ViewCart");
        }

        // Update cart quantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCartQuantity(int id, string size, int quantity)
        {
            if (quantity < 1)
                return RedirectToAction("ViewCart");

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ItemId == id && c.Size == size);

            if (item != null)
            {
                // Check stock availability
                var dbItem = _context.UniformItems.Find(id);
                if (dbItem != null && quantity > dbItem.Stock)
                {
                    TempData["Message"] = $"Only {dbItem.Stock} units available. Cannot update to {quantity}.";
                    return RedirectToAction("ViewCart");
                }

                item.Quantity = quantity;
                SaveCart(cart);
                TempData["Message"] = "Cart updated.";
            }

            return RedirectToAction("ViewCart");
        }

        //checkout page
        public ActionResult Checkout()
        {
            if (Session["ParentId"] == null)
                return RedirectToAction("Login", "Parent");

            var cart = GetCart();
            if (cart == null || !cart.Any())
                return RedirectToAction("ViewCart");

            int parentId = (int)Session["ParentId"];
            var parent = _context.Parents.Find(parentId);
            if (parent == null) return RedirectToAction("Login", "Parent");

            // Validate stock BEFORE creating order
            foreach (var c in cart)
            {
                var dbItem = _context.UniformItems.Find(c.ItemId);
                if (dbItem == null || dbItem.Stock < c.Quantity)
                {
                    TempData["Message"] = $"Not enough stock for {c.Name}. Available: {dbItem?.Stock ?? 0}";
                    return RedirectToAction("ViewCart");
                }
            }

            // Create order (Pending)
            var order = new UniformOrder
            {
                ParentId = parentId,
                OrderDate = DateTime.Now,
                Status = "Completed",
                TotalAmount = cart.Sum(c => c.UnitPrice * c.Quantity),
                RecipientName = parent.FullName,
                RecipientEmail = parent.Email
            };

            _context.UniformOrders.Add(order);
            _context.SaveChanges(); // get order.Id

            // Save order items
            foreach (var cartItem in cart)
            {
                var orderItem = new UniformOrderItem
                {
                    OrderId = order.Id,
                    UniformItemId = cartItem.ItemId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    Size = cartItem.Size
                };
                _context.UniformOrderItems.Add(orderItem);
            }
            _context.SaveChanges();

            // Stripe payment session creation
            StripeConfiguration.ApiKey = ConfigurationManager.AppSettings["StripeSecretKey"];
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = cart.Select(c => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "zar",
                        UnitAmount = (long)(c.UnitPrice * 100), // Stripe expects cents
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"{c.Name} ({c.Size})"
                        }
                    },
                    Quantity = c.Quantity
                }).ToList(),
                Mode = "payment",
                SuccessUrl = Url.Action("OrderSuccess", "Uniform", new { id = order.Id }, protocol: Request.Url.Scheme),
                CancelUrl = Url.Action("ViewCart", "Uniform", null, protocol: Request.Url.Scheme),
                CustomerEmail = parent.Email
            };

            var service = new SessionService();
            Session session = service.Create(options);

            
            return Redirect(session.Url);
        }


        public ActionResult OrderSuccess(int id)
        {
            var order = _context.UniformOrders
                .Include(o => o.Items.Select(i => i.UniformItem))
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return HttpNotFound();

            // ====== Deduct Stock AFTER payment ======
            foreach (var orderItem in order.Items)
            {
                var dbItem = _context.UniformItems.Find(orderItem.UniformItemId);
                if (dbItem != null)
                {
                    dbItem.Stock -= orderItem.Quantity;
                    if (dbItem.Stock < 0)
                        dbItem.Stock = 0;
                    _context.Entry(dbItem).State = EntityState.Modified;
                }
            }
            _context.SaveChanges();

            // ====== Parent Email (Invoice) ======
            try
            {
                var parent = _context.Parents.Find(order.ParentId);
                if (parent != null)
                {
                    string toEmail = parent.Email;
                    string parentFName = parent.FullName;

                    // Build invoice
                    StringBuilder invoice = new StringBuilder();
                    invoice.AppendLine("Order Summary:");
                    invoice.AppendLine("---------------------------");

                    decimal totalAmount = 0;
                    foreach (var item in order.Items)
                    {
                        decimal itemTotal = item.Quantity * item.UniformItem.Price;
                        totalAmount += itemTotal;
                        invoice.AppendLine($"{item.UniformItem.Name} - Quantity: {item.Quantity} - Price: R{itemTotal:F2}");
                    }

                    invoice.AppendLine("---------------------------");
                    invoice.AppendLine($"Total Amount: R{totalAmount:F2}\n");

                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress("mfundo.mahlabarh@gmail.com");
                    mail.To.Add(toEmail);
                    mail.Subject = "Sunnydale High School - Uniform Order Confirmation";
                    mail.Body = $"Dear {parentFName},\n\n" +
                                $"Your order was successful.\n" +
                                $"Order Number: {order.Id}\n\n" +
                                invoice.ToString() +
                                "You can log in to track your order status.\n\n" +
                                "Warm regards,\nSunnydale High School Team";

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential("mfundo.mahlabarh@gmail.com", "yzzw vzjo staj osru");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parent email failed: " + ex.Message);
            }

            // ====== Admin Low Stock Notification ======
            try
            {
                var lowStockItems = order.Items
                    .Where(i => i.UniformItem.Stock <= 5) // threshold
                    .ToList();

                if (lowStockItems.Any())
                {
                    StringBuilder lowStockHtml = new StringBuilder();
                    lowStockHtml.AppendLine("<h3>Low Stock Alert</h3>");
                    lowStockHtml.AppendLine("<p>The following items are low on stock:</p><ul>");

                    foreach (var item in lowStockItems)
                    {
                        lowStockHtml.AppendLine($"<li>{item.UniformItem.Name} - Remaining: {item.UniformItem.Stock}</li>");
                    }

                    lowStockHtml.AppendLine("</ul>");

                    var admins = _context.Admins.Select(a => a.Email).ToList();
                    if (admins.Any())
                    {
                        MailMessage adminMail = new MailMessage();
                        adminMail.From = new MailAddress("mfundo.mahlabarh@gmail.com");
                        foreach (var adminEmail in admins)
                        {
                            adminMail.To.Add(adminEmail);
                        }
                        adminMail.Subject = "Sunnydale High School - Low Stock Alert";
                        adminMail.Body = lowStockHtml.ToString();
                        adminMail.IsBodyHtml = true;

                        using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                        {
                            smtp.UseDefaultCredentials = false;
                            smtp.Credentials = new NetworkCredential("mfundo.mahlabarh@gmail.com", "yzzw vzjo staj osru");
                            smtp.EnableSsl = true;
                            smtp.Send(adminMail);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Admin notification failed: " + ex.Message);
            }

            return View(order);
        }



        // Dispose pattern to properly handle DbContext
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}