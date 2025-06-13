using System.ComponentModel.DataAnnotations;

namespace DIP.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "請輸入帳號")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
