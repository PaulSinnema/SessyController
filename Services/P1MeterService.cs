using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace SessyController.Services
{

    public class P1MeterService
    {
        private readonly HttpClient _httpClient;

        public P1MeterService(string baseAddress, string username, string password)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            var authToken = Encoding.ASCII.GetBytes($"{username}:{password}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
        }

        public async Task<P1Details> GetP1DetailsAsync()
        {
            var response = await _httpClient.GetAsync("/api/v2/p1/details");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<P1Details>(content);
        }

        public async Task<GridTarget> GetGridTargetAsync()
        {
            var response = await _httpClient.GetAsync("/api/v1/meter/grid_target");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GridTarget>(content);
        }

        public async Task SetGridTargetAsync(GridTargetPost gridTarget)
        {
            var json = JsonConvert.SerializeObject(gridTarget);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/v1/meter/grid_target", content);
            response.EnsureSuccessStatusCode();
        }
    }

    public class P1Details
    {
        public string Status { get; set; }
        public string State { get; set; }
        public int DsmrVersion { get; set; }
        public long PowerConsumedTariff1 { get; set; }
        public long PowerProducedTariff1 { get; set; }
        public long PowerConsumedTariff2 { get; set; }
        public long PowerProducedTariff2 { get; set; }
        public int TariffIndicator { get; set; }
        public int PowerConsumed { get; set; }
        public int PowerProduced { get; set; }
        public int PowerTotal { get; set; }
        public int PowerFailureAnyPhase { get; set; }
        public int LongPowerFailureAnyPhase { get; set; }
        public int VoltageSagCountL1 { get; set; }
        public int VoltageSagCountL2 { get; set; }
        public int VoltageSagCountL3 { get; set; }
        public int VoltageSwellCountL1 { get; set; }
        public int VoltageSwellCountL2 { get; set; }
        public int VoltageSwellCountL3 { get; set; }
        public int VoltageL1 { get; set; }
        public int VoltageL2 { get; set; }
        public int VoltageL3 { get; set; }
        public int CurrentL1 { get; set; }
        public int CurrentL2 { get; set; }
        public int CurrentL3 { get; set; }
        public int PowerConsumedL1 { get; set; }
        public int PowerConsumedL2 { get; set; }
        public int PowerConsumedL3 { get; set; }
        public int PowerProducedL1 { get; set; }
        public int PowerProducedL2 { get; set; }
        public int PowerProducedL3 { get; set; }
        public DateTime GasMeterValueTime { get; set; }
        public double GasMeterValue { get; set; }
    }

    public class GridTarget
    {
        public int GridTargetValue { get; set; }
    }

    public class GridTargetPost
    {
        public int GridTargetValue { get; set; }
    }
}
