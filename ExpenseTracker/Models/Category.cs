using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        [Required]
        public string Title { get; set; }
        [Column(TypeName = "nvarchar(5)")]

        public string Icon { get; set; } = "";
        [Column(TypeName = "nvarchar(10)")]

        public string Type { get; set; } = "Expense";

        //ta có một annotation mới ở đây, [NotMapped] định nghĩa thuộc tính này ko liên quan, ko cần tạo thành 1 cột trong database
        [NotMapped]
        public string? IconWithTitle { 
            get
            {
                return Icon + " " + Title;
            }
        }
    }
}
