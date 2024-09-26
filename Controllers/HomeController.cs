using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BMHCSDL.Models;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

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
                        Price =  reader.GetDecimal(2),
                        ImageUrl = reader.GetString(3)
                    };
                    products.Add(product);
                }
            }
        }

        return products;
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
