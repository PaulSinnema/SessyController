using Microsoft.AspNetCore.Mvc;
using SessyController.Services;

namespace SessyController.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BatteryManagementController : ControllerBase
    {
        private readonly ILogger<BatteryManagementController> _logger;
        private readonly BatteryManagementService _epexHourlyPricesService;

        public BatteryManagementController(BatteryManagementService epexHourlyPricesService, ILogger<BatteryManagementController> logger)
        {
            _logger = logger;
            _epexHourlyPricesService = epexHourlyPricesService;
        }

        /// <summary>
        /// Gets the prices fetched by the background service.
        /// </summary>
        [HttpGet(Name = "GetPrizes")]
        public Dictionary<DateTime, double> GetPrizes()
        {
            return _epexHourlyPricesService?.GetPrices() ?? new Dictionary<DateTime, double>();
        }

        /// <summary>
        /// Set the curren power used by your home in watts.
        /// </summary>
        /// <param name="watt"></param>
        [HttpPut(Name = "SetCurrentPower")]
        public void SetCurrentPower(double watt)
        {
            _epexHourlyPricesService.SetCurrentPower(watt);
        }
    }
}
