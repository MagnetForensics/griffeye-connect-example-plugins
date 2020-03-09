using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Griffeye;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ConnectDebugPlugin.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly ILogger<DebugController> _logger;

        public DebugController(ILogger<DebugController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("dashboardhome")]
        [Route("dashboardview")]
        [Route("workspaceview")]
        [Route("workspacenew")]
        public string Post(UserActionPayload data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }

        [HttpPost]
        [Route("filemenu")]
        [Route("filesubmenu")]
        public string Post(FileUserActionPayload data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }

        [HttpPost]
        [Route("entitymenu")]
        [Route("entityfilemenu")]
        public string Post(EntityUserActionPayload data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }

        [HttpGet]
        public string Get()
        {
            return "Hello world!";
        }
    }
}
