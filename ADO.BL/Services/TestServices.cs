
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class TestServices : ITestServices
    {
        private readonly ITest2Services test2Services;

        public TestServices(ITest2Services _ITest2Services)
        {
            test2Services = _ITest2Services;            
        }

        public void TestFunction()
        {
            var test = test2Services.TestCall();
            Console.WriteLine(test);            
            Console.WriteLine("Completado");
        }

    }
}
