using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
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
            string expression = "/users/user[@name='" + user + "' and @pass='" + pass + "']"; // Unsafe

            // An attacker can bypass authentication by setting user to this special value
            // user = "' or 1=1 or ''='";

            return Content(doc.SelectSingleNode(expression) != null ? "success" : "fail"); // Noncompliant
        }

        public ActionResult SampleSonar2(string user, string pass)
        {
            // Restrict the username and password to letters only
            if (!ValidateInputParameters(user, pass)) 
                return NotFound();

            string expression = "/users/user[@name='" + user + "' and @pass='" + pass + "']"; // Compliant
            return Content(doc.SelectSingleNode(expression) != null ? "success" : "fail");
        }

        private bool ValidateInputParameters(string user, string pass)
        {
            return Regex.IsMatch(user, "^[a-zA-Z]+$") && Regex.IsMatch(pass, "^[a-zA-Z]+$");
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

        public ActionResult SampleCodeQL(string userName)
        {
        // BAD: Use user-provided data directly in an XPath expression
            string badXPathExpr = "//users/user[login/text()='" + userName + "']/home_dir/text()";
            XPathExpression.Compile(badXPathExpr);

            // GOOD: XPath expression uses variables to refer to parameters
            string xpathExpression = "//users/user[login/text()=$username]/home_dir/text()";
            XPathExpression xpath = XPathExpression.Compile(xpathExpression);

            // Arguments are provided as a XsltArgumentList()
            XsltArgumentList varList = new XsltArgumentList();
            varList.AddParam("userName", string.Empty, userName);

            // CustomContext is an application specific class, that looks up variables in the
            // expression from the varList.
            CustomContext context = new CustomContext(new NameTable(), varList);
            xpath.SetContext(context);
            return Ok();
        }


    }

    public class CustomContext : XsltContext
    {
        public CustomContext(NameTable nt, XsltArgumentList argsList) : base(nt)
        {
        }

        public override int CompareDocument(string baseUri, string nextbaseUri)
        {
            return 0;
        }

        public override bool PreserveWhitespace(System.Xml.XPath.XPathNavigator node)
        {
            return false;
        }

        public override IXsltContextFunction ResolveFunction(string prefix, string name, System.Xml.XPath.XPathResultType[] ArgTypes)
        {
            return name.Equals("current") ? new XPathContextFunction("current") : null;
        }

        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            return null;
        }

        public override bool Whitespace => false;

        private class XPathContextFunction : IXsltContextFunction
        {
            private string _functionName;

            public XPathContextFunction(string functionName)
            {
                _functionName = functionName;
            }

            public XPathResultType[] ArgTypes => null;

            public XPathResultType ReturnType => XPathResultType.Navigator;

            public int Minargs => 0;

            public int Maxargs => 0;

            public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
            {
                if (_functionName.Equals("current"))
                {
                    XmlNode currentNode = ((IHasXmlNode)docContext).GetNode();

                    return currentNode;
                }

                return null;
            }
        }
    }


}