using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace julieta.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class WeakRandomGeneratorController : ControllerBase
    {
        public IActionResult Random1()
        {
            Random random = new Random();
            var sensitiveVariable = random.Next();
            return new OkResult();
        }

        public IActionResult Random2(int toExclusive)
        {
            var sensitiveVariable = RandomNumberGenerator.GetInt32(toExclusive);
            return new OkResult();
        }

        public IActionResult Random3()
        {
            var random = new Random(); // Sensitive use of Random
            byte[] data = new byte[16];
            random.NextBytes(data);
            BitConverter.ToString(data); // Check if this value is used for hashing or encryption
            return new OkResult();
        }

        public IActionResult Random4()
        {
            var randomGenerator = RandomNumberGenerator.Create(); // Compliant for security-sensitive use cases
            byte[] data = new byte[16];
            randomGenerator.GetBytes(data);
            BitConverter.ToString(data);
            return new OkResult();
        }

        public IActionResult Random5()
        {
            // crypto strong seed :)
            int seed;
            using (var generator = new RNGCryptoServiceProvider())
            {
                var intBytes = new byte[4];
                generator.GetBytes(intBytes);
                seed = BitConverter.ToInt32(intBytes, 0);
            }
            var random = new Random(seed);
            var r = random.Next();

            return new OkResult();
        }
    }
}