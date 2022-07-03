using System;
using System.Web.Services;

namespace CentralBankPublicWebService.WebServices
{
    /// <summary>
    /// Summary description for MathService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class MathService : WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public int Add(int a, int b)
        {
            return (a + b);
        }

        [WebMethod]
        public float Subtract(float a, float b)
        {
            return (a - b);
        }

        [WebMethod]
        public float Multiply(float a, float b)
        {
            return a * b;
        }

        [WebMethod]
        public float Divide(float a, float b)
        {
            if (b == 0) return -1;
            return Convert.ToSingle(a / b);
        }
    }
}
