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
        private readonly DayAheadMarketService __dayAheadMarketService;

        public BatteryManagementController(DayAheadMarketService DayAheadMarketService,
                                           SessyService sessyService,
                                           ILogger<BatteryManagementController> logger)
        {
            _logger = logger;
            _sessyService = sessyService;
            __dayAheadMarketService = DayAheadMarketService;
        }

        /// <summary>
        /// Gets the prices fetched by the background service.
        /// </summary>
        [HttpGet("DayAheadMarketService", Name = "GetPrizes")]
        public SortedDictionary<DateTime, double> GetPrizes()
        {
            return __dayAheadMarketService.GetPrices();
        }

        /// <summary>
        /// Set the curren power used by your home in watts.
        /// </summary>
        [HttpPut("DayAheadMarketService", Name = "SetCurrentPower")]
        public void SetCurrentPower(double watt)
        {
            _sessyService.SetCurrentPower(watt);
        }
    }
}
