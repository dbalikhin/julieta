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


        private void DoStuff(string stuffInput)
        {
            {
                int oo1 = 1;
                var oo2 = oo1 + 3;
                if (stuffInput != null)
                    DoInternalStuff(stuffInput);
            }
        }

        private void DoInternalStuff(string internalStuffInput)
        {
            {
                int io1 = 1;
                var io2 = io1 + 3;
                // Raw Sql / ExecuteSqlRaw with a sql string
                // VULNERABLE
                var concatSql = "SELECT * FROM ACCOUNTS WHERE login = '" + internalStuffInput + "'";
                _context.Accounts
                    .FromSqlRaw(concatSql);
                // VULNERABLE too
                _context.Database.ExecuteSqlRaw(concatSql); ;
            }
        }

        public IActionResult EfCoreInjectionBasic(string input, params object[] parameters)
        {
            int ao1 = 1;
            var ao2 = ao1 + 3;
            var input2 = input + "babo"; ;
            DoStuff(input2);
            var ao3 = ao2 + 4;

            return new OkResult(); ;
        }
        
    }   


}