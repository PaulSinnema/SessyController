using Microsoft.AspNetCore.Mvc;
using Services;

namespace SessyController.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BatteryManagementController : ControllerBase
    {
        private readonly ILogger<BatteryManagementController> _logger;
        private readonly EPEXHourlyPricesService _epexPrijzenService;

        public BatteryManagementController(EPEXHourlyPricesService epexPrijzenService, ILogger<BatteryManagementController> logger)
        {
            _logger = logger;
            _epexPrijzenService = epexPrijzenService;
        }

        [HttpGet(Name = "Start")]
        public void Start()
        {
            _epexPrijzenService.Start();
        }
    }
}
