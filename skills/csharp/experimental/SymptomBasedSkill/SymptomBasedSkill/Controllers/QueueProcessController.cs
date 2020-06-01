using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SymptomBasedSkill.Controllers
{
    [Route("api/processQueueEvents")]
    [ApiController]
    public class QueueProcessController : ControllerBase
    {
        [HttpPost]
        public IActionResult Score([FromBody] string request)
        {
            var notiication = JsonConvert.DeserializeObject<string>(request);

            return Ok();
        }
    }
}
