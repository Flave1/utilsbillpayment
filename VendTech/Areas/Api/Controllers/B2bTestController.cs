using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using VendTech.DAL;

namespace VendTech.Areas.Api.Controllers.b2b
{
    public class b2btestController: ApiController
    {
        private readonly ICalculatorService _calculatorService;

        public b2btestController(ICalculatorService calculatorService)
        {
            _calculatorService = calculatorService;
        }

        [HttpGet]
        [ActionName("get")]
        public IHttpActionResult Get(int id)
        {
            var result = _calculatorService.Add(id, id);
            if (result == 0)
                return NotFound();

            return Ok(result);
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