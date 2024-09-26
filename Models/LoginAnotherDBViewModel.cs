using System.ComponentModel.DataAnnotations;

namespace BMHCSDL.Models
{
    public class LoginAnotherDBViewModel
    {
        [Required(ErrorMessage = "Host không được để trống.")]
        public required string Host { get; set; }
        [Required(ErrorMessage = "Port không được để trống.")]
        public required string Port { get; set; }
        [Required(ErrorMessage = "UserID không được để trống.")]
        public required string SID { get; set; }
        [Required(ErrorMessage = "SID không được để trống.")]
        public required string userId { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
