using BMHCSDL.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using BMHCSDL.Data;
namespace BMHCSDL.Controllers;
public class AccountController : Controller
{
    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

[HttpPost]
public async Task<IActionResult> Register(RegisterViewModel model)
{
    string connectionStringTemplate = _configuration.GetConnectionString("OracleAdmin");
    
    try
    {
        using (OracleConnection connection = new OracleConnection(connectionStringTemplate))
        {
            await connection.OpenAsync();
            using (OracleCommand checkCommand = new OracleCommand("KiemTraTrungTenUser", connection))
            {
                checkCommand.CommandType = System.Data.CommandType.StoredProcedure;

                checkCommand.Parameters.Add("p_username", OracleDbType.Varchar2).Value = model.UserId;
                checkCommand.Parameters.Add("p_exists", OracleDbType.Varchar2, 5).Direction = System.Data.ParameterDirection.Output;

                await checkCommand.ExecuteNonQueryAsync();


                string exists = checkCommand.Parameters["p_exists"].Value.ToString();

                if (exists == "TRUE")
                {
                    ViewBag.Error = "Tên người dùng đã tồn tại. Vui lòng chọn tên khác.";
                    return View("Register"); 
                }
            }
            using (OracleCommand command = new OracleCommand("TaoTaiKhoan", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add("p_username", OracleDbType.Varchar2).Value = model.UserId;
                command.Parameters.Add("p_password", OracleDbType.Varchar2).Value = CaesarPlus.CaesarEncrypt(model.Password,3);

                await command.ExecuteNonQueryAsync();
            }
        }

        ViewBag.Message = $"Tạo tài khoản thành công cho user: {model.UserId}";
        return View("Login");
    }
    catch (Exception ex)
    {
        ViewBag.Error = $"Lỗi khi tạo tài khoản: {ex.Message}";
        return View("Error");
    }
}

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string connectionStringTemplate = _configuration.GetConnectionString("Oracle");
        string DecryptString = CaesarPlus.CaesarEncrypt(model.Password, 3);
        string connectionString = string.Format(connectionStringTemplate, model.userId, DecryptString);
        if (model.userId.Equals("SYS", StringComparison.OrdinalIgnoreCase))
        {
            connectionString += ";DBA Privilege=SYSDBA";
        }
        try
        {
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();

                string roleQuery = @"SELECT *
                                    FROM USER_ROLE_PRIVS";
                using (var command = new OracleCommand(roleQuery, connection))
                {
                    command.Parameters.Add(new OracleParameter("userId", model.userId.ToUpper()));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read() && !reader.IsDBNull(1))
                        {
                            string role = reader.GetString(1);
                            HttpContext.Session.SetString("UserName", reader.GetString(0));
                            HttpContext.Session.SetString("UserRole", role);
                        }
                    }
                }

                var commandDate = new OracleCommand("SELECT SYSDATE FROM dual", connection);
                commandDate.ExecuteScalar();
            }
        }

        catch (OracleException ex)
        {
            ModelState.AddModelError(string.Empty, "Đăng nhập không thành công");
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi: " + ex.Message);
            return View(model);
        }
        return RedirectToAction("Index", "Home");
    }




    public IActionResult Logout()
    {
        HttpContext.Session.Remove("UserName");
        return RedirectToAction("Index", "Home");
    }
    [Route("Denied")]
    [HttpGet("Denied")]
    public IActionResult Denied()
    {
        var model = new DeniedViewModel
        {
            Message = "You do not have permission to view this page."
        };
        return View(model);
    }
}
