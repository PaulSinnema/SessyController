using System.Xml;

namespace Services
{
    public class EPEXHourlyPricesService
    {
        private static readonly HttpClient client = new HttpClient();
        private const string ApiUrl = "https://web-api.tp.entsoe.eu/api";
        private const string SecurityTokenKey = "EPEX:SecurityToken";
        private const string InDomain = "10YNL----------L"; // EIC-code voor Nederland
        private const string Ns = "urn:iec62325.351:tc57wg16:451-3:publicationdocument:7:3";
        private const string TimeSeries = "//ns:TimeSeries";
        private const string IntervalStart = "ns:timeInterval/ns:start";
        private const string Resolution = "ns:resolution";
        private const string DateFormat = "yyyyMMdd";
        private const string PriceAmount = "ns:price.amount";
        private const string Position = "ns:position";
        private const string ResolutionFormat = "PT60M";
        private const string Point = "ns:Point";
        private const string Period = "ns:Period";
        private const string Time = "0000";
        private Timer? _timer;
        private static string? _securityToken;
        private volatile Dictionary<DateTime, double> _prices;


        public EPEXHourlyPricesService(IConfiguration configuration)
        {
            _securityToken = configuration[SecurityTokenKey];
        }

        public void Start()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            _timer = new Timer(Process, null, 1, 1000 * 3600 * 24);
        }

        /// <summary>
        /// This routine is called periodicly through a timer.
        /// </summary>
        public async void Process(object? state)
        {
            // Stap 1: Fetch day-ahead market prices
            _prices = await FetchDayAheadPricesAsync(DateTime.UtcNow);

            // Stap 2: Voorspellen van energieverbruik en -opwekking
            //var predictedConsumption = PredictEnergyConsumption();
            //var predictedGeneration = PredictEnergyGeneration();

            // Stap 3: Optimaliseren van batterijgebruik
            //var batterySchedule = OptimizeBatteryUsage(prices, predictedConsumption, predictedGeneration);

            // Uitvoeren van het laad- en ontlaadschema
            //ExecuteBatterySchedule(batterySchedule);
        }

        /// <summary>
        /// Get the fetched prices for today and tomorrow (if present).
        /// </summary>
        public Dictionary<DateTime, double> GetPrices() 
        {
            return _prices;
        }

        private static async Task<Dictionary<DateTime, double>> FetchDayAheadPricesAsync(DateTime date)
        {
            date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            var prices = new Dictionary<DateTime, double>();
            string periodStart = date.ToString(DateFormat) + Time;
            string periodEnd = date.AddDays(2).ToString(DateFormat) + Time;
            string url = $"{ApiUrl}?documentType=A44&in_Domain={InDomain}&out_Domain={InDomain}&periodStart={periodStart}&periodEnd={periodEnd}&securityToken={_securityToken}";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(responseBody);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("ns", Ns);
            GetPrizes(prices, xmlDoc, nsmgr);

            // Detecteer en vul ontbrekende punten in
            FillMissingPoints(prices, date, date.AddDays(1), TimeSpan.FromHours(1));

            return prices.OrderBy(point => point.Key).ToDictionary();
        }

        private static void GetPrizes(Dictionary<DateTime, double> prices, XmlDocument xmlDoc, XmlNamespaceManager nsmgr)
        {
            foreach (XmlNode timeSeries in xmlDoc.SelectNodes(TimeSeries, nsmgr))
            {
                var period = timeSeries.SelectSingleNode(Period, nsmgr);

                var startTime = DateTime.Parse(period.SelectSingleNode(IntervalStart, nsmgr).InnerText);
                var resolution = period.SelectSingleNode(Resolution, nsmgr).InnerText;
                var interval = resolution == ResolutionFormat ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(15);
                var nodes = period.SelectNodes(Point, nsmgr);

                foreach (XmlNode point in nodes)
                {
                    int position = int.Parse(point.SelectSingleNode(Position, nsmgr).InnerText);
                    double price = double.Parse(point.SelectSingleNode(PriceAmount, nsmgr).InnerText);
                    DateTime timestamp = startTime.Add(interval * (position));
                    prices.Add(timestamp, price / 1000);
                }
            }
        }

        private static void FillMissingPoints(Dictionary<DateTime, double> prices, DateTime periodStart, DateTime periodEnd, TimeSpan interval)
        {
            DateTime currentTime = periodStart;

            while (currentTime < periodEnd)
            {
                if (!prices.ContainsKey(currentTime))
                {
                    // Zoek de vorige en volgende bekende prijzen
                    var previousPrice = GetPreviousPrice(prices, currentTime);
                    var nextPrice = GetNextPrice(prices, currentTime);

                    if (previousPrice.HasValue && nextPrice.HasValue)
                    {
                        // Bereken het gemiddelde van de vorige en volgende prijs
                        prices[currentTime] = (previousPrice.Value + nextPrice.Value) / 2;
                    }
                    else if (previousPrice.HasValue)
                    {
                        // Als er geen volgende prijs is, gebruik de vorige prijs
                        prices[currentTime] = previousPrice.Value;
                    }
                    else if (nextPrice.HasValue)
                    {
                        // Als er geen vorige prijs is, gebruik de volgende prijs
                        prices[currentTime] = nextPrice.Value;
                    }
                    else
                    {
                        // Geen vorige of volgende prijs beschikbaar; log een waarschuwing
                        Console.WriteLine($"Waarschuwing: Geen prijsinformatie beschikbaar voor {currentTime}");
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

        private static Dictionary<DateTime, double> PredictEnergyConsumption()
        {
            // Implementeer hier uw voorspellingsmodel voor energieverbruik
            // Voorbeeld: retourneer een dummy-voorspelling
            return Enumerable.Range(0, 24).ToDictionary(
                i => DateTime.UtcNow.Date.AddHours(i),
                i => 1.0 // Dummy-waarde in kWh
            );
        }

        private static Dictionary<DateTime, double> PredictEnergyGeneration()
        {
            // Implementeer hier uw voorspellingsmodel voor energieopwekking
            // Voorbeeld: retourneer een dummy-voorspelling
            return Enumerable.Range(0, 24).ToDictionary(
                i => DateTime.UtcNow.Date.AddHours(i),
                i => 0.5 // Dummy-waarde in kWh
            );
        }

        private static Dictionary<DateTime, double> OptimizeBatteryUsage(
            Dictionary<DateTime, double> prices,
            Dictionary<DateTime, double> predictedConsumption,
            Dictionary<DateTime, double> predictedGeneration)
        {
            var schedule = new Dictionary<DateTime, double>();
            double batteryCapacity = 10.0; // Totale batterijcapaciteit in kWh
            double currentCharge = 5.0; // Huidige lading in kWh

            foreach (var hour in prices.Keys.OrderBy(p => p))
            {
                double netConsumption = predictedConsumption[hour] - predictedGeneration[hour];
                if (netConsumption > 0)
                {
                    // Verbruik is hoger dan opwekking
                    if (currentCharge >= netConsumption)
                    {
                        // Gebruik batterij om aan vraag te voldoen
                        schedule[hour] = -netConsumption;
                        currentCharge -= netConsumption;
                    }
                    else
                    {
                        // Batterij is niet voldoende; koop resterende energie in
                        schedule[hour] = -currentCharge;
                        currentCharge = 0;
                    }
                }
                else
                {
                    // Opwekking is hoger dan verbruik
                    double surplus = -netConsumption;
                    if (currentCharge + surplus <= batteryCapacity)
                    {
                        // Laad batterij op met overschot
                        schedule[hour] = surplus;
                        currentCharge += surplus;
                    }
                    else
                    {
                        // Batterij is vol; verkoop overschot
                        schedule[hour] = batteryCapacity - currentCharge;
                        currentCharge = batteryCapacity;
                    }
                }
            }

            return schedule;
        }

        private static void ExecuteBatterySchedule(Dictionary<DateTime, double> schedule)
        {
            foreach (var entry in schedule)
            {
                DateTime time = entry.Key;
                double power = entry.Value; // Positief voor opladen, negatief voor ontladen

                // Implementeer hier de aansturing van uw batterij op het aangegeven tijdstip
                Console.WriteLine($"{time}: {(power >= 0 ? "Opladen" : "Ontladen")} met {Math.Abs(power)} kWh");
            }
        }
    }
}
