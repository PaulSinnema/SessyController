using System.Xml;

namespace SessyController.Services
{
    public class BatteryManagementService : BackgroundService
    {
        private const string ApiUrl = "https://web-api.tp.entsoe.eu/api";
        private const string SecurityTokenKey = "EPEX:SecurityToken";
        private const string DateFormat = "yyyyMMdd";
        private const string Time = "0000";

        private const string Ns = "urn:iec62325.351:tc57wg16:451-3:publicationdocument:7:3";
        private const string IntervalStart = "ns:timeInterval/ns:start";
        private const string Period = "ns:Period";
        private const string Point = "ns:Point";
        private const string Position = "ns:position";
        private const string PriceAmount = "ns:price.amount";
        private const string Resolution = "ns:resolution";
        private const string TimeSeries = "//ns:TimeSeries";

        private const string InDomain = "InDomain"; // EIC-code
        private const string ResolutionFormat = "ResolutionFormat";

        private static string? _securityToken;
        private static string? _inDomain;
        private static string? _resolutionFormat;
        private volatile Dictionary<DateTime, double>? _prices;
        private static IHttpClientFactory? _httpClientFactory;
        private static LoggingService<BatteryManagementService>? _logger;

        public BatteryManagementService(IConfiguration configuration, IHttpClientFactory httpClientFactory, LoggingService<BatteryManagementService> logger)
        {
            _securityToken = configuration[SecurityTokenKey];
            _inDomain = configuration[InDomain];
            _resolutionFormat = configuration[ResolutionFormat];
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public void SetCurrentPower(double watt)
        {

        }

        /// <summary>
        /// Executes the background service, fetching prices periodically.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken cancelationToken)
        {
            _logger.LogInformation("EPEXHourlyPricesService started.");

            // Loop to fetch prices every 24 hours
            while (!cancelationToken.IsCancellationRequested)
            {
                try
                {
                    await Process(cancelationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, "An error occurred while processing EPEX prices.");
                }

                // Wait for 24 hours or until cancellation
                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), cancelationToken);
                }
                catch (TaskCanceledException)
                {
                    // Ignore cancellation exception during delay
                }
            }

            _logger.LogInformation("EPEXHourlyPricesService stopped.");

        }

        /// <summary>
        /// This routine is called periodicly as a background task.
        /// </summary>
        public async Task Process(CancellationToken cancellationToken)
        {
            // Fetch day-ahead market prices
            _prices = await FetchDayAheadPricesAsync(DateTime.UtcNow, cancellationToken);
        }

        /// <summary>
        /// Get the fetched prices for today and tomorrow (if present).
        /// </summary>
        public Dictionary<DateTime, double>? GetPrices() 
        {
            return _prices;
        }

        /// <summary>
        /// Get the day-ahead-prices from ENTSO-E Api.
        /// </summary>
        private static async Task<Dictionary<DateTime, double>> FetchDayAheadPricesAsync(DateTime date, CancellationToken cancellationToken)
        {
            date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            string periodStart = date.ToString(DateFormat) + Time;
            string periodEnd = date.AddDays(2).ToString(DateFormat) + Time;
            string url = $"{ApiUrl}?documentType=A44&in_Domain={_inDomain}&out_Domain={_inDomain}&periodStart={periodStart}&periodEnd={periodEnd}&securityToken={_securityToken}";

            var client = _httpClientFactory?.CreateClient();

            if (client != null)
            {
                HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                var prices = GetPrizes(responseBody);

                // Detect and fill gaps in the prices with average prices.
                FillMissingPoints(prices, date, date.AddDays(1), TimeSpan.FromHours(1));

                return prices.OrderBy(point => point.Key).ToDictionary();
            }

            _logger.LogError("Unable to create HttpClient");

            return new Dictionary<DateTime, double>();
        }

        /// <summary>
        /// Get the prices and timestamps from the XML response.
        /// </summary>
        private static Dictionary<DateTime, double> GetPrizes(string responseBody)
        {
            var prices = new Dictionary<DateTime, double>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(responseBody);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("ns", Ns);

            var timeSeriesNodes = xmlDoc.SelectNodes(TimeSeries, nsmgr);

            if(timeSeriesNodes != null)
            {
                foreach (XmlNode timeSeries in timeSeriesNodes)
                {
                    if (TimeSeries != null)
                    {
                        XmlNode? period = timeSeries.SelectSingleNode(Period, nsmgr);

                        if (period != null)
                        {

                            var startTime = DateTime.Parse(GetSingleNode(period, IntervalStart, nsmgr));
                            var resolution = GetSingleNode(period, Resolution, nsmgr);
                            var interval = resolution == _resolutionFormat ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(15);
                            var pointNodes = period.SelectNodes(Point, nsmgr);

                            if (pointNodes != null)
                            {
                                foreach (XmlNode point in pointNodes)
                                {
                                    int position = int.Parse(GetSingleNode(point, Position, nsmgr));
                                    double price = double.Parse(GetSingleNode(point, PriceAmount, nsmgr));
                                    DateTime timestamp = startTime.Add(interval * (position));
                                    prices.Add(timestamp, price / 1000);
                                }
                            }
                        }
                    }
                }
            }

            return prices;
        }

        /// <summary>
        /// Get a single node from a node. Returns an empty string if node was notfound.
        /// </summary>
        private static string GetSingleNode(XmlNode? node, string key, XmlNamespaceManager nsmgr)
        {
            if (node != null)
            {
                var singleNode = node.SelectSingleNode(key, nsmgr);

                if (singleNode != null)
                {
                    return singleNode.InnerText;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Sometimes prices are missing. This routine fill the gaps with average prices.
        /// </summary>
        private static void FillMissingPoints(Dictionary<DateTime, double> prices, DateTime periodStart, DateTime periodEnd, TimeSpan interval)
        {
            DateTime currentTime = periodStart;

            while (currentTime < periodEnd)
            {
                if (!prices.ContainsKey(currentTime))
                {
                    // Search for the previous and future prices.
                    var previousPrice = GetPreviousPrice(prices, currentTime);
                    var nextPrice = GetNextPrice(prices, currentTime);

                    _logger.LogInformation($"Price missing for {currentTime}");

                    if (previousPrice.HasValue && nextPrice.HasValue)
                    {
                        // Calculate the average price.
                        prices[currentTime] = (previousPrice.Value + nextPrice.Value) / 2;
                    }
                    else if (previousPrice.HasValue)
                    {
                        // Use previous prices in case the next price is missing
                        prices[currentTime] = previousPrice.Value;
                    }
                    else if (nextPrice.HasValue)
                    {
                        // Use next prices in case the previous price is missing
                        prices[currentTime] = nextPrice.Value;
                    }
                    else
                    {
                        // Price information is missing. Write to log.
                        _logger.LogWarning($"No price information available for {currentTime}");
                    }
                }

                currentTime = currentTime.Add(interval);
            }
        }

        private static double? GetPreviousPrice(Dictionary<DateTime, double> prices, DateTime timestamp)
        {
            var previousTimes = prices.Keys.Where(t => t < timestamp).OrderByDescending(t => t);
            return previousTimes.Any() ? prices[previousTimes.First()] : (double?)null;
        }

        private static double? GetNextPrice(Dictionary<DateTime, double> prices, DateTime timestamp)
        {
            var nextTimes = prices.Keys.Where(t => t > timestamp).OrderBy(t => t);
            return nextTimes.Any() ? prices[nextTimes.First()] : (double?)null;
        }
    }
}
