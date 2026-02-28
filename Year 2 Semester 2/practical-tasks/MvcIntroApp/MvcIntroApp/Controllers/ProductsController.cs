using Microsoft.AspNetCore.Mvc;

namespace MvcIntroApp.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View(products);
        }
        public IActionResult Create()
        {
            return View(new Product());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            // 1) Серверна валідація на основі DataAnnotations
            if (!ModelState.IsValid)
                return View(product);

            // 2) Присвоюємо Id (бо БД поки немає)
            product.Id = products.Count == 0 ? 1 : products.Max(p => p.Id) + 1;

            // 3) Додаємо в список
            products.Add(product);

            // 4) PRG-патерн (Post/Redirect/Get): щоб не дублювати POST при F5
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (!ModelState.IsValid)
                return View(product);

            var existing = products.FirstOrDefault(p => p.Id == product.Id);
            if (existing == null)
                return NotFound();

            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.Category = product.Category;

            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var existing = products.FirstOrDefault(p => p.Id == id);
            if (existing != null)
                products.Remove(existing);

            return RedirectToAction("Index");
        }
        private static List<Product> products = new()
        {
            new Product { Id = 1, Name = "Laptop", Price = 35000, Category = "Electronics" },
            new Product { Id = 2, Name = "Phone", Price = 18000, Category = "Electronics" }
        };
    }
}
