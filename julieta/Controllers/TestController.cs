using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using julieta.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace julieta.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {

        private readonly DataContext _context;

        public TestController(DataContext context)
        {
            _context = context;
        }
        public IActionResult EfCoreSelectTest(string user)
        {

            var a1 = _context.Accounts
                .FromSqlRaw("SELECT * FROM ACCOUNTS WHERE login={0}", user)
                .FirstOrDefault();

            var sql = "SELECT * FROM ACCOUNTS WHERE login = '" + user + "'";
            var a2 = _context.Accounts
                .FromSqlRaw(sql)
                .FirstOrDefault();

            var a3 = _context.Accounts
                .FromSqlRaw($"SELECT * FROM ACCOUNTS WHERE login = '{user}'")
                .FirstOrDefault();

            var a4 = _context.Accounts
                .FromSqlRaw("SELECT * FROM ACCOUNTS WHERE login = '" + user + "'")
                .FirstOrDefault();

            var a5 = _context.Accounts
                .FromSqlRaw("SELECT * FROM ACCOUNTS WHERE login = {0}", user)
                .FirstOrDefault();

            return new OkResult();
        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3001:Review code for SQL injection vulnerabilities", Justification = "<Pending>")]
        public IActionResult SqlTest(string productName)
        {
            int productNameId = int.Parse(productName);
            using (SqlConnection connection = new SqlConnection("dummyconnectionstring"))
            {
                SqlCommand sqlCommand = new SqlCommand()
                {
                    CommandText = "SELECT ProductId FROM Products WHERE ProductName = '" + productNameId + "'",
                    CommandType = CommandType.Text,
                };

                SqlDataReader reader = sqlCommand.ExecuteReader();
            }

            return new OkResult();
        }

        public IActionResult CookieTest()
        {
            Response.Cookies.Append(
                "COOKIE_NAME",
                "COOKIE_VALUE",
                new CookieOptions()
                {
                    Path = "/",
                    HttpOnly = false,
                    Secure = false
                }
            );

            Cookie cookie = new Cookie("SecretCookie", "SecretValue");
            
            return new OkResult();
        }
    }


}