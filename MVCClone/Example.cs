using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCClone
{

    public class HomeController : Controller
    {
        public string Index(string msg)
        {
            return "Hello World";
        }
    }

    public class TestController : Controller
    {

        public string Index()
        {
            return "Test";
        }
    }
}

void Main()
{
    var uri = new Uri("http://localhost/home/index");
    var container = new MVCContainer();
    container.Resolve(uri);
}