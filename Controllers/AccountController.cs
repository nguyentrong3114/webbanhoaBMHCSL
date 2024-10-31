using BMHCSDL.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using BMHCSDL.Data;
using System.Text;
using System.Data;
namespace BMHCSDL.Controllers;
public class AccountController : Controller
{
    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
    }



    public int GetSessionCount(string username)
    {
        string connectionString = _configuration.GetConnectionString("OracleAdmin");
        string query = @"SELECT COUNT(*) FROM v$session WHERE username = :username";

        using (OracleConnection conn = new OracleConnection(connectionString))
        {
            conn.Open();
            using (OracleCommand cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add(new OracleParameter("username", username));

                int sessionCount = Convert.ToInt32(cmd.ExecuteScalar());
                return sessionCount;
            }
        }
    }

    public void TerminateOldSession(string username)
    {
        string query = @"SELECT sid, serial# FROM v$session WHERE username = :username ORDER BY logon_time ASC";
        string connectionString = _configuration.GetConnectionString("OracleAdmin");
        using (OracleConnection conn = new OracleConnection(connectionString))
        {
            conn.Open();
            using (OracleCommand cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add(new OracleParameter("username", username));

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string sid = reader["sid"].ToString();
                        string serial = reader["serial#"].ToString();
                        TerminateSession(sid, serial);
                    }
                }
            }
        }
    }
    private void TerminateSession(string sid, string serial)
    {
        string killSessionQuery = $"ALTER SYSTEM KILL SESSION '{sid},{serial}' IMMEDIATE";
        string connectionString = _configuration.GetConnectionString("OracleAdmin");
        using (OracleConnection conn = new OracleConnection(connectionString))
        {
            conn.Open();
            using (OracleCommand cmd = new OracleCommand(killSessionQuery, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }

    public class UserIdGenerator
    {
        private static Random random = new Random();

        public static string GenerateUserId(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder userId = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                userId.Append(chars[random.Next(chars.Length)]);
            }

            return userId.ToString();
        }
    }
    public void GenerateAndStoreKeys(string userId)
    {
        string connectionStringTemplate = _configuration.GetConnectionString("OracleAdmin");
        using (var connection = new OracleConnection(connectionStringTemplate))
        {
            connection.Open();

            // Tạo khóa RSA
            using (var command = new OracleCommand("CRYPTO.RSA_GENERATE_KEYS", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Khai báo tham số đầu ra
                var keys = command.Parameters.Add("p_keys", OracleDbType.Varchar2, 4000);
                keys.Direction = ParameterDirection.Output;

                command.ExecuteNonQuery();

                // Lấy khóa từ tham số đầu ra
                string privateKey = keys.Value.ToString();
                string publicKey = keys.Value.ToString(); // Bạn có thể chia tách nếu cần

                // Chèn khóa vào bảng
                using (var insertCommand = new OracleCommand("INSERT INTO user_keys (user_id, private_key, public_key) VALUES (:userId, :privateKey, :publicKey)", connection))
                {
                    insertCommand.Parameters.Add(":userId", OracleDbType.Varchar2).Value = userId;
                    insertCommand.Parameters.Add(":privateKey", OracleDbType.Varchar2).Value = privateKey;
                    insertCommand.Parameters.Add(":publicKey", OracleDbType.Varchar2).Value = publicKey;

                    insertCommand.ExecuteNonQuery();
                }
            }
        }
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
        string newUserId = string.Empty;
        string passwordEncryppt = CallCaesarFromDatabase.Encrypt(connectionStringTemplate, model.Password, 3);
        try
        {
            using (OracleConnection connection = new OracleConnection(connectionStringTemplate))
            {
                await connection.OpenAsync();

                bool isUnique = false;
                while (!isUnique)
                {
                    newUserId = UserIdGenerator.GenerateUserId();

                    using (OracleCommand checkCommand = new OracleCommand("KiemTraTrungTenUser", connection))
                    {
                        checkCommand.CommandType = System.Data.CommandType.StoredProcedure;

                        checkCommand.Parameters.Add("p_username", OracleDbType.Varchar2).Value = model.UserName;
                        checkCommand.Parameters.Add("p_exists", OracleDbType.Varchar2, 5).Direction = System.Data.ParameterDirection.Output;

                        await checkCommand.ExecuteNonQueryAsync();
                        string exists = checkCommand.Parameters["p_exists"].Value.ToString();

                        isUnique = (exists != "TRUE");
                    }
                }
                using (OracleCommand command = new OracleCommand("TaoTaiKhoan", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.Add("p_username", OracleDbType.Varchar2).Value = model.UserName;
                    command.Parameters.Add("p_password", OracleDbType.Varchar2).Value = passwordEncryppt;

                    await command.ExecuteNonQueryAsync();
                }
                string key = "privatekey";
                byte[] b = Des.EncryptDES(connectionStringTemplate, model.email, key);
                string emailEncypted = Convert.ToBase64String(b);
                using (OracleCommand insertCommand = new OracleCommand("INSERT INTO USERS (MAUSER,HOTEN,TENTAIKHOAN,EMAIL) VALUES (:userId, :fullname,:username,:email )", connection))
                {
                    insertCommand.Parameters.Add(":userId", OracleDbType.Varchar2).Value = newUserId;
                    insertCommand.Parameters.Add(":fullname", OracleDbType.Varchar2).Value = model.FullName;
                    insertCommand.Parameters.Add(":username", OracleDbType.Varchar2).Value = model.UserName;
                    insertCommand.Parameters.Add(":email", OracleDbType.Varchar2).Value = emailEncypted;
                    await insertCommand.ExecuteNonQueryAsync();
                }
                string alterUserSql = $"ALTER USER {model.UserName} PROFILE user_profile";
                using (OracleCommand grantProfile = new OracleCommand(alterUserSql, connection))
                {
                    await grantProfile.ExecuteNonQueryAsync();
                }
            }

            ViewBag.Message = $"Tạo tài khoản thành công cho user: {newUserId}";
            return View("Login");
        }
        catch (OracleException ex)
        {
            switch (ex.Number)
            {
                case 1:
                    ModelState.AddModelError(string.Empty, "Username hoặc email đã tồn tại");
                    break;
            }
            return View(model);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Lỗi khi tạo tài khoản: {ex.Message}";
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult LoginAnotherDB()
    {
        return View();
    }
    [HttpPost]
    public IActionResult LoginAnotherDB(LoginAnotherDBViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string connectionStringTemplate = _configuration.GetConnectionString("OracleDefault");
        string DecryptString = CaesarPlus.CaesarEncrypt(model.Password, 3);
        string connectionString = string.Format(connectionStringTemplate, model.userId, DecryptString, model.Host, model.Port, model.SID);
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

        string connectionStringTemplate2 = _configuration.GetConnectionString("OracleAdmin");
        string passwordEncryppt = CallCaesarFromDatabase.Encrypt(connectionStringTemplate2, model.Password, 3);
        string connectionString = string.Format(connectionStringTemplate, model.userId, passwordEncryppt);

        if (model.userId.Equals("SYS", StringComparison.OrdinalIgnoreCase))
        {
            connectionString += ";DBA Privilege=SYSDBA";
        }
        try
        {
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();

                string roleQuery = "SELECT * FROM USER_ROLE_PRIVS WHERE USERNAME = :userId";
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
            switch (ex.Number)
            {
                case 1017:
                    ModelState.AddModelError(string.Empty, "Sai tên tài khoản hoặc mật khẩu.");
                    break;
                case 28000:
                    ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa.");
                    break;
                case 02391:
                    ModelState.AddModelError(string.Empty, "Tài khoản của bạn đang đăng nhập ở thiết bị khác vui lòng đăng xuất.");
                    break;
                default:
                    ModelState.AddModelError(string.Empty, "Đăng nhập không thành công: " + ex.Message);
                    break;
            }
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
