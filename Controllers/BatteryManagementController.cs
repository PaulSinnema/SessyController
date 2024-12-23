using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SessyController.Common;
using SessyController.Services;

namespace SessyController.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BatteryManagementController : ControllerBase
    {
        private readonly ILogger<BatteryManagementController> _logger;
        private readonly BatteriesService _batteriesService;
        private readonly DayAheadMarketService _dayAheadMarketService;
        private readonly SessyService _sessyService;
        private readonly SessyBatteryConfig _batteryConfig;

        public BatteryManagementController(DayAheadMarketService DayAheadMarketService,
                                           BatteriesService batteriesService,
                                           SessyService sessyService,
                                           IOptions<SessyBatteryConfig> batteryConfig,
                                           ILogger<BatteryManagementController> logger)
        {
            _logger = logger;
            _batteriesService = batteriesService;
            _dayAheadMarketService = DayAheadMarketService;
            _sessyService = sessyService;
            _batteryConfig = batteryConfig.Value;
        }

        /// <summary>
        /// Gets the prices fetched by the background service.
        /// </summary>
        [HttpGet("DayAheadMarketService", Name = "GetPrizes")]
        public SortedDictionary<DateTime, double> GetPrizes()
        {
            return _dayAheadMarketService.GetPrices();
        }


        [HttpGet("SessyService", Name = "{id}/PowerStatus")]
        public async Task<IActionResult> GetPowerStatus(string id)
        {
            if (!_batteryConfig.Batteries.TryGetValue(id, out var battery))
            {
                return NotFound($"Battery with ID {id} not found.");
            }

            var status = await _sessyService.GetPowerStatusAsync(battery);

            return Ok(status);
        }
    }
}
