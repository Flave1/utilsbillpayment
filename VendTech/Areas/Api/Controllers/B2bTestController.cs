using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using VendTech.BLL.Interfaces;
using VendTech.Framework.Api;

namespace VendTech.Areas.Api.Controllers.b2b
{
    public class b2btestController: BaseB2bAPIController
    {
        private readonly ICalculatorService _calculatorService;

        public b2btestController(ICalculatorService calculatorService,
            IErrorLogManager errorLogManager) : base(errorLogManager)
        {
            _calculatorService = calculatorService;
        }

        [HttpGet]
        [ActionName("get")]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage Get(int id)
        {
            var result = _calculatorService.Add(id, id);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }


        //[HttpPost]
        //[Route("")]
        //public IHttpActionResult CreateUser(User user)
        //{
        //    if (user == null || !ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var createdUser = _userService.CreateUser(user);
        //    return CreatedAtRoute("GetUserById", new { id = createdUser.Id }, createdUser);
        //}
    }
}