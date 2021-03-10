using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AspNetCoreRateLimit.Demo.Controllers
{
    [Route("api/[controller]/{uname_id}")]
    public class UserController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(string id)
        {
            return "value is " + id;
        }

        [HttpGet()]
        [Route("email/{id}")]
        public string Email(string id)
        {
            return id + "@email.com";
        }

        [HttpGet()]
        [Route("location/{id}")]
        public string Location(string id)
        {
            return id + " lives in seattle";
        }
    }
}
