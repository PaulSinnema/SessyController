using Microsoft.AspNetCore.Mvc;
using SessyController.Services;

namespace SessyController.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BatteryManagementController : ControllerBase
    {
        private readonly ILogger<BatteryManagementController> _logger;
        private readonly SessyService? _sessyService;
        private readonly DayAheadMarketService _epexHourlyPricesService;

        public BatteryManagementController(DayAheadMarketService epexHourlyPricesService,
                                           SessyService sessyService,
                                           ILogger<BatteryManagementController> logger)
        {
            _logger = logger;
            _sessyService = sessyService;
            _epexHourlyPricesService = epexHourlyPricesService;
        }

        /// <summary>
        /// Gets the prices fetched by the background service.
        /// </summary>
        [HttpGet("EpexHourlyPricesService", Name = "GetPrizes")]
        public SortedDictionary<DateTime, double> GetPrizes()
        {
            return _epexHourlyPricesService.GetPrices();
        }

        /// <summary>
        /// Set the curren power used by your home in watts.
        /// </summary>
        [HttpPut("EpexHourlyPricesService", Name = "SetCurrentPower")]
        public void SetCurrentPower(double watt)
        {
            _epexHourlyPricesService.SetCurrentPower(watt);
        }

        /// <summary>
        /// Get the status of the battery.
        /// </summary>
        [HttpGet("BatteryManagementService", Name = "GetSessySystemState")]
        public async Task<int> GetSessySystemState()
        {
            var powerStatus = await _sessyService.StatusAsync();

            return powerStatus.Sessy.Power;

        }
    }
}
