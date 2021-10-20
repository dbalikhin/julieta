using System;
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
        public IActionResult EfCoreInjectionBasic(string user)
        {
            // Raw Sql Same - one line
            // SAFE
            _context.Accounts
                .FromSqlRaw("SELECT * FROM ACCOUNTS WHERE login={0}", user);

            // ExecuteSqlRaw - one line
            // SAFE
            _context.Database.ExecuteSqlRaw("SELECT * FROM ACCOUNTS WHERE login={0}", user);

            // Raw SQL multi line
            // SAFE
            _context.Accounts
                .FromSqlRaw(
                    "SELECT * FROM ACCOUNTS WHERE login={0}",
                    user
                );

            // ExecuteSqlRaw - multi line
            // SAFE
            _context.Database.ExecuteSqlRaw(
                "SELECT * FROM ACCOUNTS WHERE login={0}",
                user
            );

            // Raw Sql / ExecuteSqlRaw with a sql string
            // VULNERABLE
            var concatSql = "SELECT * FROM ACCOUNTS WHERE login = '" + user + "'";
            _context.Accounts
                .FromSqlRaw(concatSql);
            // VULNERABLE too
            _context.Database.ExecuteSqlRaw(concatSql);

            // Raw Sql / ExecuteSqlRaw with a format sql str
            // VULNERABLE
            var formatStringSql = string.Format("SELECT * FROM ACCOUNTS WHERE login = '{0}'", user);
            _context.Accounts
                .FromSqlRaw(formatStringSql);
            // VULNERABLE
            _context.Database.ExecuteSqlRaw(formatStringSql);

            // Raw Sql / ExecuteSqlRaw with an interpolated sql str
            // VULNERABLE
            var interpolatedStringSql = $"SELECT * FROM ACCOUNTS WHERE login = '{user}'";
            _context.Accounts
                .FromSqlRaw(interpolatedStringSql);
            // VULNERABLE
            _context.Database.ExecuteSqlRaw(interpolatedStringSql);

            // Raw Sql / ExecuteSqlRaw with an interpolated sql str - one-liner
            // VULNERABLE
            _context.Accounts
                .FromSqlRaw($"SELECT * FROM ACCOUNTS WHERE login = '{user}'");
            // VULNERABLE
            _context.Database.ExecuteSqlRaw(interpolatedStringSql);


            // Interpolated Sql / ExecuteSqlInterpolated with an interpolated sql str
            // SAFE
            _context.Accounts
                .FromSqlInterpolated($"SELECT * FROM ACCOUNTS WHERE login = '{user}'");
            // SAFE
            _context.Database.ExecuteSqlInterpolated($"SELECT * FROM ACCOUNTS WHERE login = '{user}'");

            // Sql Raw / ExecuteSqlRaw - Implicit Db Parameter
            // SAFE
            _context.Accounts
                .FromSqlRaw("SELECT * FROM ACCOUNTS WHERE login = {0}", user);
            // SAFE
            _context.Database.ExecuteSqlRaw("SELECT * FROM ACCOUNTS WHERE login = {0}", user);

            // Interpolated Sql - Implicit Db Parameter
            // SAFE
            _context.Accounts
                .FromSqlInterpolated($"SELECT * FROM ACCOUNTS WHERE login = {user}");
            // SAFE
            _context.Database.ExecuteSqlInterpolated($"SELECT * FROM ACCOUNTS WHERE login = {user}");


            return new OkResult();
        }

        public IActionResult EfCoreInjectionAdvanced(string userInput, int userIdInput, Guid guidInput, string guidStrInput, string ageInput)
        {
            int age = 0;
            int.TryParse(ageInput, out age);
            string guidStr = guidInput.ToString();

            var concatSql = "SELECT * FROM ACCOUNTS WHERE " +
                            "login='" + userInput + "' AND " +
                            "userId='" + userIdInput + " AND " +
                            "guid='" + guidStr + "' AND " +
                            "age=" + age + "";
            // VULNERABLE: userInput
            _context.Accounts
                .FromSqlRaw(concatSql);
            // VULNERABLE: userInput
            _context.Database
                .ExecuteSqlRaw(concatSql);

            // use GUID (string), adds a new vuln
            var formatStringSql = string.Format("SELECT * FROM ACCOUNTS WHERE login = '{0}' AND userId = {1} AND guid='{2}' AND age={3}", userInput, userIdInput, guidStrInput, age);
            // VULNERABLE: userInput, guidStrInput
            _context.Accounts
                .FromSqlRaw(formatStringSql);
            // VULNERABLE: userInput, guidStrInput
            _context.Database.ExecuteSqlRaw(formatStringSql);

            // guid validation case
            // check if it is a valid guid
            var goodGuid = Guid.TryParse(guidStrInput, out var validGuid);
            var myNewGuidStr = goodGuid ? guidStrInput : "";

            var interpolatedStringSql = $"SELECT * FROM ACCOUNTS WHERE login = '{userInput}' AND userId = { userIdInput } AND guid = '{myNewGuidStr}' AND age = { age }";
            // VULNERABLE: userInput
            _context.Accounts
                .FromSqlRaw(interpolatedStringSql);
            // VULNERABLE: userInput
            _context.Database.ExecuteSqlRaw(interpolatedStringSql);

            return new OkResult();
        }

        //private void Do(DbSet<Account> account, )

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