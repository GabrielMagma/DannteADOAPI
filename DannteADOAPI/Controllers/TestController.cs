//using Microsoft.AspNetCore.Mvc;
//using ADO.BL.Responses;
//using ADO.BL.Interfaces;

//namespace DannteADOAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class TestController : Controller
//    {
//        readonly ITestServices testServices;

//        public TestController(ITestServices _testServices)
//        {
//            testServices = _testServices;
//        }

//        /// <summary>
//        /// prueba de llamado secuencial.
//        /// </summary>
//        /// <param></param>
//        /// <returns></returns>  
//        [HttpPost]
//        [Route(nameof(TestController.TestFunction))]
//        public async Task<IActionResult> TestFunction()
//        {
//            return await Task.Run(() =>
//            {
//                testServices.TestFunction();
//                return Ok();
//            });
//        }

//    }
//}
