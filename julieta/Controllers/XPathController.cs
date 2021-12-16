using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace julieta.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class XPathController : ControllerBase
    {
        public XmlDocument doc { get; set; }
        public XPathNavigator AuthorizedOperations { get; set; }

        public ActionResult SampleSonar1(string user, string pass)
        {
            String expression = "/users/user[@name='" + user + "' and @pass='" + pass + "']"; // Unsafe

            // An attacker can bypass authentication by setting user to this special value
            // user = "' or 1=1 or ''='";

            return Content(doc.SelectSingleNode(expression) != null ? "success" : "fail"); // Noncompliant
        }

        public ActionResult SampleSonar2(string user, string pass)
        {
            // Restrict the username and password to letters only
            if (!ValidateInputParameters(user, pass)) 
                return NotFound();

            String expression = "/users/user[@name='" + user + "' and @pass='" + pass + "']"; // Compliant
            return Content(doc.SelectSingleNode(expression) != null ? "success" : "fail");
        }

        private bool ValidateInputParameters(string user, string pass)
        {
            if (!Regex.IsMatch(user, "^[a-zA-Z]+$") || !Regex.IsMatch(pass, "^[a-zA-Z]+$"))
            {
                return false;
 
            }

            return true;
        }

        public ActionResult SampleRoslyn(string operation)
        {
            // If an attacker uses this for input:
            //     ' or 'a' = 'a
            // Then the XPath query will be:
            //     authorizedOperation[@username = 'anonymous' and @operationName = '' or 'a' = 'a']
            // and it will return any authorizedOperation node.
            XPathNavigator node = AuthorizedOperations.SelectSingleNode(
                "//authorizedOperation[@username = 'anonymous' and @operationName = '" + operation + "']");
            return Ok();
        }

        public ActionResult SampleSCS1(string input)
        {
            XmlDocument doc = new XmlDocument { XmlResolver = null };
            doc.Load("/config.xml");
            var results = doc.SelectNodes("/Config/Devices/Device[id='" + input + "']");

            return Ok();
        }
        public ActionResult SampleSCS2(string input)
        {
            Regex rgx = new Regex(@"^[a-zA-Z0-9]+$");
            if (rgx.IsMatch(input)) //Additional validation
            {
                XmlDocument doc = new XmlDocument { XmlResolver = null };
                doc.Load("/config.xml");
                var results = doc.SelectNodes("/Config/Devices/Device[id='" + input + "']");
            }
            return Ok();
        }

    }


}