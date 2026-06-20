using FlowerShop.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBHT.Models // Hãy sửa lại đúng tên Namespace đồ án của bạn nếu khác
{
    public class Manufacturer
    {
        [Key]
        public int ManufacturerId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = null!;

        [StringLength(100)]
        public string? Country { get; set; }

        // Một hãng sản xuất có thể cung cấp danh sách nhiều sản phẩm
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}