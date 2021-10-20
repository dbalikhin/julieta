using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using julieta.Data;
using Microsoft.AspNetCore.Authorization;
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

        public IActionResult EfCoreInjectionAdvanced(string userInput, int userIdInput, Guid guidInput, string ageInput)
        {
            int age = 0;
            int.TryParse(ageInput, out age);
            string guidStr = guidInput.ToString();

            var concatSql = "SELECT * FROM ACCOUNTS WHERE " +
                            "login='" + userInput + "' AND " +
                            "userId='" + userIdInput + " AND " + // integer (not vulnerable)
                            "guid='" + guidStr + "' AND " +      // string from GUID (not vulnerable)
                            "age=" + age + "";                   // parsed int age from string (not vulnerable) 
            // VULNERABLE: userInput
            _context.Accounts
                .FromSqlRaw(concatSql);
            // VULNERABLE: userInput
            _context.Database
                .ExecuteSqlRaw(concatSql);

            // use ageInput (string), adds a new vuln (ability to detect both and provide a proper source)
            var formatStringSql = string.Format("SELECT * FROM ACCOUNTS WHERE login = '{0}' AND userId = {1} AND guid='{2}' AND age='{3}'", userInput, userIdInput, guidInput, ageInput); // userIdInput - int, guidInput - Guid (not vulnerable)
            // VULNERABLE: userInput, ageInput
            _context.Accounts
                .FromSqlRaw(formatStringSql);
            // VULNERABLE: userInput, guidStrInput
            _context.Database.ExecuteSqlRaw(formatStringSql);

            // validation case for age - accept only valid ageInput, default to "18"
            // attempt to confuse a scanner by introducing a new sanitizer/validator
            var goodAge = int.TryParse(ageInput, out var validAge);
            var myValidAgeStr = goodAge ? ageInput : "18"; // validated integer age as a string (not vulnerable)

            var interpolatedStringSql = $"SELECT * FROM ACCOUNTS WHERE userId = { userIdInput } AND guid = '{guidInput}' AND age = '{ myValidAgeStr }'"; // userIdInput - int, guidInput - Guid(not vulnerable)
            // SAFE
            _context.Accounts
                .FromSqlRaw(interpolatedStringSql);
            // SAFE
            _context.Database.ExecuteSqlRaw(interpolatedStringSql);

            return new OkResult();
        }

        
        
        public IActionResult SqlClientTest(int productCategoryIdInput, string productCategoryIdStrInput, string productCategoryNameInput, string spNameInput)
        {
            int productCategoryIdParsed = int.Parse(productCategoryIdStrInput);
            string productCategory = productCategoryIdParsed.ToString();

            string concatSql = "SELECT * FROM Products WHERE ProductCategoryId = '" + productCategoryIdInput +
                               "' AND ProductCategoryName ='" + productCategoryNameInput + "'";

            string formattedSql =
                string.Format("SELECT * FROM Products WHERE ProductCategoryId = '{0}' AND ProductCategoryName ='{1}'",
                    productCategoryIdInput, productCategoryNameInput);

            string interpolatedSql = $"SELECT * FROM Products WHERE ProductCategoryId = '{productCategoryIdInput}' AND ProductCategoryName ='{productCategoryNameInput}'";

            string intStringConversionSql = $"SELECT * FROM Products WHERE ProductCategoryId = '{productCategory}'";

            // in real life, vulnerable fields should be declared as "varchar" in the db, not int
            string twoVulnsSql = $"SELECT * FROM Products WHERE ProductCategoryId = '{productCategoryIdStrInput}' AND ProductCategoryName ='{productCategoryNameInput}'";

            using (SqlConnection connection = new SqlConnection("dummyconnectionstring"))
            {
                SqlCommand concatSqlCommand = new SqlCommand()
                {
                    CommandText = concatSql,
                    CommandType = CommandType.Text,
                };
                // VULNERABLE: productCategoryNameInput
                concatSqlCommand.ExecuteReader();

                SqlCommand formattedSqlCommand = new SqlCommand()
                {
                    CommandText = formattedSql,
                    CommandType = CommandType.Text,
                };
                // VULNERABLE: productCategoryNameInput
                formattedSqlCommand.ExecuteReader();

                
                SqlCommand interpolatedSqlCommand = new SqlCommand()
                {
                    CommandText = interpolatedSql,
                    CommandType = CommandType.Text,
                };
                // VULNERABLE: productCategoryNameInput
                interpolatedSqlCommand.ExecuteReader();

                SqlCommand intStringConversionSqlCommand = new SqlCommand()
                {
                    CommandText = intStringConversionSql,
                    CommandType = CommandType.Text,
                };
                // SAFE
                intStringConversionSqlCommand.ExecuteReader();

                SqlCommand twoVulnsSqlCommand = new SqlCommand()
                {
                    CommandText = twoVulnsSql,
                    CommandType = CommandType.Text,
                };
                // VULNERABLE: productCategoryIdStrInput and productCategoryNameInput
                twoVulnsSqlCommand.ExecuteReader();

                // SAFE
                // Should trigger Roslyn CA2100
                var spName = "sp_NormalStuff";
                var r1 =GetDatatable(connection, spName, null);

                // VULNERABLE - allow to use any sp
                var r2= GetDatatable(connection, spNameInput, null);

                if (spNameInput == "sp_NormalStuff" || spNameInput == "sp_AlsoNormalStuff")
                {
                    // SAFE
                    var r3 = GetDatatable(connection, spNameInput, null);
                }

            }

            return new OkResult();
        }

        private DataTable GetDatatable(SqlConnection connection, string procName, Hashtable parms)
        {
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            cmd.CommandText = procName;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = connection;
            if (parms.Count > 0)
            {
                foreach (DictionaryEntry de in parms)
                {
                    cmd.Parameters.AddWithValue(de.Key.ToString(), de.Value);
                }
            }
            da.SelectCommand = cmd;
            da.Fill(dt);
            return dt;
        }

        private DataTable UnusedGetDatatable(SqlConnection connection, string procName, Hashtable parms)
        {
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            cmd.CommandText = procName;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = connection;
            if (parms.Count > 0)
            {
                foreach (DictionaryEntry de in parms)
                {
                    cmd.Parameters.AddWithValue(de.Key.ToString(), de.Value);
                }
            }
            da.SelectCommand = cmd;
            da.Fill(dt);
            return dt;
        }


        /*
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
        */
    }


}