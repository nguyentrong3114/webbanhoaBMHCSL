
using System.Data;
using System.Numerics;
using BMHCSDL.Data;
using BMHCSDL.Filters;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace BMHCSDL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeAdmin]
    public class AdminDashBoardController : Controller
    {

        private readonly ILogger<AdminDashBoardController> _logger;
        private readonly IConfiguration _configuration;
        public AdminDashBoardController(IConfiguration configuration, ILogger<AdminDashBoardController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        [HttpPost]
        public IActionResult AddProducts(ManageProduct product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (!ModelState.IsValid)
            {
                return View("ProductDashBoard", product);
            }
            try
            {
                string? connectionString = _configuration.GetConnectionString("OracleAdmin");
                using (var connection = new OracleConnection(connectionString))
                {

                    connection.Open();
                    using (var command = new OracleCommand("ADD_PRODUCT", connection))
                    {

                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new OracleParameter("p_maHoa", product.MaHoa));
                        command.Parameters.Add(new OracleParameter("p_maDanhMuc", product.DanhMuc));
                        command.Parameters.Add(new OracleParameter("p_tenHoa", product.TenHoa));
                        command.Parameters.Add(new OracleParameter("p_soLuong", product.SoLuong));
                        command.Parameters.Add(new OracleParameter("p_giaBan", product.Price));
                        command.Parameters.Add(new OracleParameter("p_trangThai", product.TrangThai));
                        command.Parameters.Add(new OracleParameter("p_moTa", Encrypt(connectionString, product.MoTa, 3)));
                        command.Parameters.Add(new OracleParameter("p_hinhAnh", product.ImageUrl));

                        command.ExecuteNonQuery();
                    }
                }
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("AdminDashBoard", "ProductDashBoard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm sản phẩm: " + ex.Message);
                return View("ProductDashBoard", product);
            }
        }
        public IEnumerable<ManageProduct> GetProducts()
        {
            var products = new List<ManageProduct>();
            string connectionString = _configuration.GetConnectionString("OracleAdmin");

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                try
                {
                    using (OracleCommand command = new OracleCommand("select MaHoa, MaDanhMuc, TenHoa, SoLuong, GiaBan, TrangThai, MoTa, HinhAnh from Hoa", connection))
                    {
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var product = new ManageProduct
                                {
                                    MaHoa = reader.GetString(0),
                                    TenHoa = reader.IsDBNull(reader.GetOrdinal("TenHoa")) ? null : reader.GetString(reader.GetOrdinal("TenHoa")),
                                    MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa")),
                                    DanhMuc = reader.IsDBNull(reader.GetOrdinal("MaDanhMuc")) ? null : reader.GetString(reader.GetOrdinal("MaDanhMuc")),
                                    SoLuong = reader.IsDBNull(reader.GetOrdinal("SoLuong")) ? 0 : reader.GetInt32(reader.GetOrdinal("SoLuong")),
                                    Price = reader.IsDBNull(reader.GetOrdinal("GiaBan")) ? 0 : reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                                    ImageUrl = reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? null : reader.GetString(reader.GetOrdinal("HinhAnh")),
                                    TrangThai = reader.IsDBNull(reader.GetOrdinal("TrangThai")) ? null : reader.GetString(reader.GetOrdinal("TrangThai")),
                                };
                                products.Add(product);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Có lỗi xảy ra khi lấy danh sách sản phẩm.");
                }
            }
            return products;
        }
        public static string Encrypt(string connectString, string text, int shift)
        {
            string encryptedText = string.Empty;

            using (var connection = new OracleConnection(connectString))
            {
                connection.Open();

                using (var command = new OracleCommand("CaesarEncrypt", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;


                    command.Parameters.Add(new OracleParameter("p_text", text));
                    command.Parameters.Add(new OracleParameter("p_shift", shift));


                    OracleParameter outputParam = new OracleParameter("p_result", OracleDbType.Varchar2, 4000);
                    outputParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(outputParam);

                    command.ExecuteNonQuery();


                    encryptedText = outputParam.Value.ToString();
                }
            }

            return encryptedText;
        }
        public IActionResult OverviewDashBoard()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ProductDashBoard()
        {
            List<ManageProduct> products = GetProducts().ToList(); // Nếu GetProducts() là async, thì thêm await
            return View(products);
        }


        [HttpPost]
        public async Task<IActionResult> ProductDashBoard(ManageProduct product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (ModelState.IsValid)
            {
                try
                {
                    string? connectionString = _configuration.GetConnectionString("OracleAdmin");
                    using (var connection = new OracleConnection(connectionString))
                    {
                        connection.Open();
                        string mota = CallCaesarFromDatabase.Encrypt(connectionString, product.MoTa, 3);
                        using (var command = new OracleCommand("INSERT INTO Hoa (MaHoa, MaDanhMuc, TenHoa, SoLuong, GiaBan, TrangThai, MoTa, HinhAnh) VALUES (:maHoa, :maDanhMuc, :tenHoa, :soLuong, :giaBan, :trangThai, :moTa, :hinhAnh)", connection))
                        {
                            command.Parameters.Add(new OracleParameter("MaHoa", product.MaHoa));
                            command.Parameters.Add(new OracleParameter("MaDanhMuc", product.DanhMuc));

                            command.Parameters.Add(new OracleParameter("TenHoa", product.TenHoa));
                            command.Parameters.Add(new OracleParameter("SoLuong", product.SoLuong));
                            command.Parameters.Add(new OracleParameter("GiaBan", product.Price));
                            command.Parameters.Add(new OracleParameter("TrangThai", product.TrangThai));
                            command.Parameters.Add(new OracleParameter("MoTa", mota));
                            command.Parameters.Add(new OracleParameter("HinhAnh", product.ImageUrl));
                            command.ExecuteNonQuery();
                        }
                    }
                    TempData["SuccessMessage"] = "Sản phẩm đã được thêm thành công!";
                    return RedirectToAction("ProductDashBoard");
                }
                catch (Exception ex)
                {

                    ModelState.AddModelError("", "Có lỗi xảy ra khi thêm sản phẩm: " + ex.Message);
                }
            }
            var products = GetProducts();
            return View(products);
        }
        private string EncryptToString(Data.SimpleRSA rsa, string input)
        {
            BigInteger[] encryptedValues = rsa.Encrypt(input);
            return string.Join(",", encryptedValues.Select(b => b.ToString()));
        }


        private ManageProduct GetProductById(string maHoa)
        {
            ManageProduct product = null;
            string connectionString = _configuration.GetConnectionString("OracleSelect");

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                OracleCommand command = new OracleCommand("select MaHoa, MaDanhMuc, TenHoa, SoLuong, GiaBan, TrangThai, MoTa, HinhAnh from ShopHoa.Hoa where MaHoa = :maHoa", connection);
                command.Parameters.Add("maHoa", OracleDbType.Varchar2).Value = maHoa;

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        product = new ManageProduct
                        {
                            MaHoa = reader.GetString(0),
                            TenHoa = reader.IsDBNull(reader.GetOrdinal("TenHoa")) ? null : reader.GetString(reader.GetOrdinal("TenHoa")),
                            MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa")),
                            DanhMuc = reader.IsDBNull(reader.GetOrdinal("MaDanhMuc")) ? null : reader.GetString(reader.GetOrdinal("MaDanhMuc")),
                            SoLuong = reader.IsDBNull(reader.GetOrdinal("SoLuong")) ? 0 : reader.GetInt32(reader.GetOrdinal("SoLuong")),
                            Price = reader.IsDBNull(reader.GetOrdinal("GiaBan")) ? 0 : reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                            ImageUrl = reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? null : reader.GetString(reader.GetOrdinal("HinhAnh")),
                            TrangThai = reader.IsDBNull(reader.GetOrdinal("TrangThai")) ? null : reader.GetString(reader.GetOrdinal("TrangThai")),
                        };
                    }
                }
            }
            return product;
        }

        public IActionResult DeliveryDashBoard()
        {
            return View();
        }
        public IActionResult CustomerDashBoard()
        {
            return View();
        }
        public IActionResult RoleDashBoard()
        {
            List<User> users = GetAllUsers();
            return View(users);
        }
        [HttpGet("admin/update-flower/{productId}")]
        public IActionResult UpdateFlower(string productId)
        {
            var product = GetProductById(productId);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost("admin/update-flower/{productId}")]
        public async Task<IActionResult> UpdateFlower(string productId, ManageProduct model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string connectionString = _configuration.GetConnectionString("OracleAdmin");

            try
            {
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (OracleCommand command = new OracleCommand("F_UPDATE_PRODUCT", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;


                        command.Parameters.Add("p_maDanhMuc", OracleDbType.Varchar2).Value = model.DanhMuc;
                        command.Parameters.Add("p_tenHoa", OracleDbType.Varchar2).Value = model.TenHoa;
                        command.Parameters.Add("p_soLuong", OracleDbType.Int32).Value = model.SoLuong;
                        command.Parameters.Add("p_giaBan", OracleDbType.Decimal).Value = model.Price;
                        command.Parameters.Add("p_trangThai", OracleDbType.Varchar2).Value = model.TrangThai;
                        command.Parameters.Add("p_moTa", OracleDbType.Varchar2).Value = model.MoTa;
                        command.Parameters.Add("p_hinhAnh", OracleDbType.Varchar2).Value = model.ImageUrl;
                        command.Parameters.Add("p_maHoa", OracleDbType.Varchar2).Value = productId;
                        string resultMessage = await command.ExecuteScalarAsync() as string;

                        TempData["SuccessMessage"] = resultMessage;
                    }
                }
                return RedirectToAction("ProductDashBoard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật sản phẩm." + ex.Message);
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> DeleteFlower(string productId)
        {

            if (string.IsNullOrEmpty(productId))
            {
                _logger.LogWarning("productId is null or empty");
                return NotFound();
            }
            {
                string connectionStringTemplate = _configuration.GetConnectionString("OracleAdmin");

                try
                {
                    using (OracleConnection connection = new OracleConnection(connectionStringTemplate))
                    {
                        await connection.OpenAsync();

                        using (OracleCommand command = new OracleCommand("DELETE_PRODUCT", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.Add("p_maHoa", OracleDbType.Varchar2).Value = productId;

                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    ViewBag.Message = $"Sản phẩm với mã {productId} đã được xóa thành công.";
                }
                catch (Exception ex)
                {
                    ViewBag.Error = $"Lỗi xảy ra khi xóa sản phẩm: {ex.Message}";
                }


                return RedirectToAction("ProductDashBoard");
            }
        }
        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            string connectionString = _configuration.GetConnectionString("OracleAdmin");

            try
            {
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("SELECT * FROM v_all_users", conn))
                    {
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new User
                                {
                                    UserId = reader.GetString(reader.GetOrdinal("MAUSER")),
                                    FullName = reader.GetString(reader.GetOrdinal("HoTen")),
                                    Email = reader.GetString(reader.GetOrdinal("EMAIL")),
                                    Username = reader.GetString(reader.GetOrdinal("TENTAIKHOAN")),
                                    PhoneNumber = reader.GetString(reader.GetOrdinal("SODIENTHOAI")),
                                    DiaChi = reader.GetString(reader.GetOrdinal("DIACHI")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("NGAYDANGKY")),
                                    Status = reader.GetString(reader.GetOrdinal("TRANGTHAI"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (users.Count == 0)
            {

            }

            return users;
        }
        [HttpPost("lock-account")]
        public IActionResult LockAccount(string Username)
        {
            string connectionString = _configuration.GetConnectionString("OracleAdmin");
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();
                using (var command = new OracleCommand("lock_user_account", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_username", OracleDbType.NVarchar2).Value = Username;

                    try
                    {
                        command.ExecuteNonQuery();
                        TempData["Message"] = "Tài khoản đã được khóa thành công.";
                    }
                    catch (Exception ex)
                    {
                        TempData["Message"] = "Lỗi khi khóa tài khoản: " + ex.Message;
                    }
                }
            }

            return RedirectToAction("RoleDashboard");
        }

        [HttpPost("unlock-account")]
        public IActionResult UnlockAccount(string Username)
        {
            string connectionString = _configuration.GetConnectionString("OracleAdmin");
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();
                using (var command = new OracleCommand("unlock_user_account", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_username", OracleDbType.NVarchar2).Value = Username;

                    try
                    {
                        command.ExecuteNonQuery();
                        TempData["Message"] = "Tài khoản đã được mở khóa thành công.";
                    }
                    catch (Exception ex)
                    {
                        TempData["Message"] = "Lỗi khi mở khóa tài khoản: " + ex.Message;
                    }
                }
            }
            return RedirectToAction("RoleDashboard");
        }
    }

}
