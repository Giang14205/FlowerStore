//using FlowerShop.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace FlowerShop.Controllers
//{
//    public class BlogController : Controller
//    {
//        private readonly QlbhtContext _context;
//        public BlogController(QlbhtContext context)
//        {
//            _context = context;
//        }
//        public IActionResult DetailBlog()
//        {
//            return View();
//        }
//        [Route("/blog/{alias}-{id}.html")]
//        public async Task <IActionResult> Details(int? id)
//        {
//            if(id==null || _context.Blogs==null)
//            {
//                return NotFound();
//            }
//            var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.BlogId == id);
//            if(blog==null)
//            {
//                return NotFound();
//            }   
//            ViewBag.BlogTag = _context.Tags.Where(i=>i.Blogid)
//        }
//    }
//}
