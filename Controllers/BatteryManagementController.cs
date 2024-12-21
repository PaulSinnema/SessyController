using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using SessyController.Services;

namespace SessyController.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BatteryManagementController : ControllerBase
    {
        private readonly ILogger<BatteryManagementController> _logger;
        private readonly EpexHourlyPricesService _epexHourlyPricesService;
        private readonly SessyService? _sessyService;

        public BatteryManagementController(EpexHourlyPricesService epexHourlyPricesService,
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
        public ConcurrentDictionary<DateTime, double> GetPrizes()
        {
            return _epexHourlyPricesService?.GetPrices() ?? new ConcurrentDictionary<DateTime, double>();
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
