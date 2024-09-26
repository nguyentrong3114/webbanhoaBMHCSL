using System.ComponentModel.DataAnnotations;

namespace BMHCSDL.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "UserID không được để trống.")]
        public required string userId { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
