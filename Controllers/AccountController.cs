using BMHCSDL.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace BMHCSDL.Controllers;
public class Account : Controller
{
    private readonly IConfiguration _configuration;

    public Account(IConfiguration configuration)
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
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await UserExists(model.UserId))
        {
            ModelState.AddModelError(string.Empty, "Người dùng đã tồn tại.");
            return View(model);
        }

        string connectionString = _configuration.GetConnectionString("OracleSys");

        using (var connection = new OracleConnection(connectionString))
        {
            await connection.OpenAsync();

            try
            {
                string createUserCommand = $"CREATE USER \"{model.UserId.ToUpper()}\" IDENTIFIED BY \"{model.Password}\"";

                using (var command = new OracleCommand(createUserCommand, connection))
                {
                    command.Parameters.Add(new OracleParameter("password", model.Password));
                    await command.ExecuteNonQueryAsync();
                }
                await GrantUserPermissions(model.UserId.ToUpper());
            }
            catch (OracleException ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi: " + ex.Message);
                return View(model);
            }
        }

        return RedirectToAction("Login");
    }


    private async Task<bool> UserExists(string userId)
    {
        string connectionString = _configuration.GetConnectionString("OracleSys");

        using (var connection = new OracleConnection(connectionString))
        {
            await connection.OpenAsync();

            using (var command = new OracleCommand($"SELECT COUNT(*) FROM all_users WHERE username = '{userId.ToUpper()}'", connection))
            {
                var count = await command.ExecuteScalarAsync();
                return Convert.ToInt32(count) > 0;
            }
        }
    }

    private async Task GrantUserPermissions(string userId)
    {
        string connectionString = _configuration.GetConnectionString("OracleSys");

        using (var connection = new OracleConnection(connectionString))
        {
            await connection.OpenAsync();

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var command = new OracleCommand($"GRANT CREATE SESSION TO \"{userId}\"", connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    using (var command = new OracleCommand($"GRANT user_role TO \"{userId}\"", connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    transaction.Commit();
                }
                catch (OracleException ex)
                {
                    throw new Exception("Đã xảy ra lỗi khi cấp quyền: " + ex.Message);
                }
            }
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
        string connectionString = string.Format(connectionStringTemplate, model.userId, model.Password);
        if (model.userId.Equals("SYS", StringComparison.OrdinalIgnoreCase))
        {
            connectionString += ";DBA Privilege=SYSDBA";
        }

        try
        {
            string OracleString = _configuration.GetConnectionString("OracleSys");
            using (var connection = new OracleConnection(OracleString))
            {
                connection.Open();

                string roleQuery = @"
                SELECT DISTINCT 
                    u.username,
                    r.granted_role
                FROM 
                    dba_users u
                LEFT JOIN 
                    dba_role_privs r ON u.username = r.grantee
                WHERE 
                    u.username = :userId";

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

                return RedirectToAction("Index", "Home");
            }
        }
        catch (OracleException ex)
        {
            ModelState.AddModelError(string.Empty, "Đăng nhập không thành công: " + ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi: " + ex.Message);
            return View(model);
        }
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
