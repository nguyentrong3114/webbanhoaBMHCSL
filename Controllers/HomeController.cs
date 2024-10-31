using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BMHCSDL.Models;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using BMHCSDL.Data;

namespace BMHCSDL.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
    public IActionResult About()
    {
        return View();
    }

    public IActionResult Clothes()
    {
        var products = GetProducts();
        return View(products);
    }
    private IEnumerable<ProductsViewModel> GetProducts()
    {
        var products = new List<ProductsViewModel>();
        string connectionString = _configuration.GetConnectionString("OracleSelect");

        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            connection.Open();

            OracleCommand command = new OracleCommand("select MaHoa,TenHoa,giaban,HinhAnh from ShopHoa.Hoa", connection);

            using (OracleDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var product = new ProductsViewModel
                    {
                        MaHoa = reader.GetString(0),
                        TenHoa = reader.GetString(1),
                        Price = reader.GetDecimal(2),
                        ImageUrl = reader.GetString(3)
                    };
                    products.Add(product);
                }
            }
        }

        return products;
    }
    [HttpPost]
    public IActionResult BuyNow(string productId)
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            HttpContext.Session.SetString("ProductId", productId);
            return RedirectToAction("Login", "Account");
        }

        return RedirectToAction("DetailsFlower", "Home", new { productId = productId });
    }
    [HttpGet("{productId}")]
    public IActionResult DetailsFlower(string productId)
    {
        ProductsViewModel product = null;
        if (string.IsNullOrEmpty(productId))
        {
            _logger.LogWarning("productId is null or empty");
            return NotFound();
        }

        string connectionString = _configuration.GetConnectionString("OracleSelect");
        string ConnectStringAdmin = _configuration.GetConnectionString("OracleAdmin");
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            connection.Open();
            using (OracleCommand command = new OracleCommand("SELECT MaHoa,TenHoa,HinhAnh,GiaBan,TenDanhMuc,SoLuong,Desription_hoa FROM ShopHoa.ChitietHoa WHERE MaHoa = :productId", connection))
            {
                command.Parameters.Add(new OracleParameter("productId", productId));
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        product = new ProductsViewModel
                        {
                            MaHoa = reader.GetString(0),
                            TenHoa = reader.GetString(1),
                            ImageUrl = reader.GetString(2),
                            Price = reader.GetDecimal(3),
                            DanhMuc = reader.GetString(4),
                            SoLuong = reader.GetInt32(5),
                            // MoTa = reader.GetString(6),
                            MoTa = CallCaesarFromDatabase.Decrypt(ConnectStringAdmin,reader.GetString(6),3)
                        };
                    }
                }
            }
        }
        if (product == null)
        {
            return NotFound();
        }

        return View("DetailsFlower", product);
    }
    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
