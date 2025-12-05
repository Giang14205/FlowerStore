using FlowerShop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Threading.Tasks; // Nhớ thêm thư viện này
namespace FlowerShop.Controllers
{
    public class ContactController : Controller
    {
        private readonly QlbhtContext _context;
        public ContactController(QlbhtContext context)
        {
            _context = context;
        }
        public IActionResult DetailContact()
        {
            return View();
        }
        [HttpPost]
        
        public async Task<IActionResult> Create(string name,string email, string message )
        {
            try {
                ContactMessage contact = new ContactMessage();
                contact.FullName = name;
                contact.Email = email;
                contact.Message = message;

                if(User.Identity.IsAuthenticated)
                {
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if(int.TryParse(userId, out int parsedId))
                    {
                        contact.UserId = parsedId;
                    }
                    else {
                        contact.UserId = null;
                            
                      }
                }
                 _context.Add(contact);
                await _context.SaveChangesAsync();
                return Json(new { status = true });
               
            
            }
            catch (Exception ex){

                return Json(new { status = false, msg=ex.Message });
                    
                    }

        }
    }
}
