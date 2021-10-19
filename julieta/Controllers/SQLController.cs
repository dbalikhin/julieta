using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using julieta.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace julieta.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class SQLController : ControllerBase
    {

        private readonly DataContext _context;

        public SQLController(DataContext context)
        {
            _context = context;
        }
        public IActionResult EfCoreInjectionTest(string user)
        {
            // Raw Sql Same Line
            // VULNERABLE
            _context.Accounts
                .FromSqlRaw("SELECT * FROM ACCOUNTS WHERE login={0}", user)
                .FirstOrDefault();

            // Raw SQL multi-lines lines
            // VULNERABLE
            _context.Accounts
                .FromSqlRaw(
                    "SELECT * FROM ACCOUNTS WHERE login={0}",
                    user
                )
                .FirstOrDefault();

            // Raw Sql with a sql string
            // VULNERABLE
            var concatSql = "SELECT * FROM ACCOUNTS WHERE login = '" + user + "'";
            _context.Accounts
                .FromSqlRaw(concatSql)
                .FirstOrDefault();

            // Raw Sql with a format sql str
            // VULNERABLE
            var formatStringSql = string.Format("SELECT * FROM ACCOUNTS WHERE login = '{0}'", user);
            _context.Accounts
                .FromSqlRaw(formatStringSql)
                .FirstOrDefault();

            // Raw Sql with an interpolated sql str
            // VULNERABLE?
            var interpolatedStringSql = $"SELECT * FROM ACCOUNTS WHERE login = '{user}'";
            _context.Accounts
                .FromSqlRaw(interpolatedStringSql)
                .FirstOrDefault();

            // Raw Sql with an interpolated sql str - one-liner
            // VULNERABLE?
            _context.Accounts
                .FromSqlRaw($"SELECT * FROM ACCOUNTS WHERE login = '{user}'")
                .FirstOrDefault();

            
            // Interpolated Sql with an interpolated sql str
            // SAFE
            _context.Accounts
                .FromSqlInterpolated($"SELECT * FROM ACCOUNTS WHERE login = '{user}'")
                .FirstOrDefault();


            return new OkResult();
        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3001:Review code for SQL injection vulnerabilities", Justification = "<Pending>")]
        public IActionResult SqlTest(string productName, string productCategory)
        {
            int productNameId = int.Parse(productName);
            using (SqlConnection connection = new SqlConnection("dummyconnectionstring"))
            {
                SqlCommand sqlCommand = new SqlCommand()
                {
                    CommandText = "SELECT ProductId FROM Products WHERE ProductName = '" + productNameId + "' AND ProductCategory ='" + productCategory + "'",
                    CommandType = CommandType.Text,
                };

                SqlDataReader reader = sqlCommand.ExecuteReader();

                SqlCommand sqlCommand2 = new SqlCommand()
                {
                    CommandText = "SELECT ProductId FROM Products WHERE ProductCategory ='" + productCategory + "'",
                    CommandType = CommandType.Text,
                };

                SqlDataReader reader2 = sqlCommand2.ExecuteReader();

                SqlCommand sqlCommand3 = new SqlCommand()
                {
                    CommandText = $"SELECT ProductId FROM Products WHERE ProductCategory ='{productCategory}'",
                    CommandType = CommandType.Text,
                };

                SqlDataReader reader3 = sqlCommand2.ExecuteReader();
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