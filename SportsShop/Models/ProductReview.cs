using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsShop.Models
{
    public class ProductReview
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string UserId { get; set; } // ID của người review

        [Required]
        [Range(1, 5, ErrorMessage = "Đánh giá từ 1 đến 5 sao")]
        public int Rating { get; set; } // 1-5 sao

        [StringLength(500, ErrorMessage = "Nội dung không quá 500 ký tự")]
        public string? Comment { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsVerifiedPurchase { get; set; } = false; // Đã mua hàng chưa?

        public bool IsApproved { get; set; } = true; // Admin duyệt chưa?

        // Navigation properties
        public Product? Product { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}