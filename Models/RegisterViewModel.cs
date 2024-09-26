using System.ComponentModel.DataAnnotations;

namespace BMHCSDL.Models
{
    public class RegisterViewModel 
    {
        [Required(ErrorMessage = "Username không được để trống.")]
        public required string UserId { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
        public required string ConfirmPassword { get; set; }
    }   

}