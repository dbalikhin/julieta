using System.Linq;
using julieta.Data;
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

            var account = _context.Accounts
                .FromSqlRaw("SELECT * FROM ACCOUNTS WHERE login={0}", user)
                .FirstOrDefault();

            var sql = "SELECT * FROM ACCOUNTS WHERE login = '" + user + "'";
            var account2 = _context.Accounts
                .FromSqlRaw(sql)
                .FirstOrDefault();

            var account3 = _context.Accounts
                .FromSqlRaw($"SELECT * FROM ACCOUNTS WHERE login = '{user}'")
                .FirstOrDefault();

            var account4 = _context.Accounts
                .FromSqlRaw("SELECT * FROM ACCOUNTS WHERE login = '" + user + "'")
                .FirstOrDefault();

            var account5 = _context.Accounts
                .FromSqlRaw("SELECT * FROM ACCOUNTS WHERE login = {0}", user)
                .FirstOrDefault();

            return new OkResult();
        }
    }


}