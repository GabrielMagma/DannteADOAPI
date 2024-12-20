
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class Test2Services : ITest2Services
    {

        public bool TestCall()
        {            
            Console.WriteLine("llegó bien");
            return true;
        }

    }
}
