using FlowerShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.ViewComponents
{
   
    public class ProductViewComponent : ViewComponent
    {
        private readonly QlbhtContext _context;
        public ProductViewComponent(QlbhtContext context)
        {
            _context = context;
        }
        public async Task <IViewComponentResult> InvokeAsync()
        {
            var items = _context.Products.Include(m => m.ProductCategory)
                .Where(m => m.IsActive==true).Where(m => m.IsNew==true);
           
            return await Task.FromResult<IViewComponentResult>(View(items.OrderByDescending(m => m.ProductId).ToList()));
            

        }
    }
}
