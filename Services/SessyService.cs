using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using SessyController.Common;

namespace SessyController.Services
{
    public class SessyService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SessyService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateHttpClient(SessyBattery battery)
        {
            if (battery == null) throw new ArgumentNullException(nameof(battery));
            if (battery.BaseUrl == null) throw new ArgumentNullException(nameof(battery.BaseUrl));

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(battery.BaseUrl);
            var authToken = Encoding.ASCII.GetBytes($"{battery.UserId}:{battery.Password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
            return client;
        }

        public async Task<PowerStatus?> GetPowerStatusAsync(SessyBattery battery)
        {
            using var client = CreateHttpClient(battery);
            var response = await client.GetAsync("/api/v1/power/status");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var powerStatus = JsonConvert.DeserializeObject<PowerStatus>(content);
            return powerStatus;
        }

        public async Task<ActivePowerStrategy?> GetActivePowerStrategyAsync(SessyBattery battery)
        {
            using var client = CreateHttpClient(battery);
            var response = await client.GetAsync("/api/v1/power/active_strategy");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ActivePowerStrategy>(content);
        }

        public async Task SetActivePowerStrategyAsync(SessyBattery battery, ActivePowerStrategy strategy)
        {
            using var client = CreateHttpClient(battery);
            var json = JsonConvert.SerializeObject(strategy);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/v1/power/active_strategy", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task SetPowerSetpointAsync(SessyBattery battery, PowerSetpoint setpoint)
        {
            using var client = CreateHttpClient(battery);
            var json = JsonConvert.SerializeObject(setpoint);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/v1/power/setpoint", content);
            response.EnsureSuccessStatusCode();
        }
    }


    public class PowerStatus
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("sessy")]
        public Sessy Sessy { get; set; }

        [JsonProperty("renewable_energy_phase1")]
        public Phase RenewableEnergyPhase1 { get; set; }

        [JsonProperty("renewable_energy_phase2")]
        public Phase RenewableEnergyPhase2 { get; set; }

        [JsonProperty("renewable_energy_phase3")]
        public Phase RenewableEnergyPhase3 { get; set; }
    }

    public class Sessy
    {
        [JsonProperty("state_of_charge")]
        public double StateOfCharge { get; set; }

        [JsonProperty("power")]
        public int Power { get; set; }

        [JsonProperty("power_setpoint")]
        public int PowerSetpoint { get; set; }

        [JsonProperty("system_state")]
        public string SystemState { get; set; }

        [JsonProperty("system_state_details")]
        public string SystemStateDetails { get; set; }

        [JsonProperty("frequency")]
        public int Frequency { get; set; }

        [JsonProperty("inverter_current_ma")]
        public int InverterCurrentMa { get; set; }
    }

    public class Phase
    {
        [JsonProperty("voltage_rms")]
        public int VoltageRms { get; set; }

        [JsonProperty("current_rms")]
        public int CurrentRms { get; set; }

        [JsonProperty("power")]
        public int Power { get; set; }
    }
    public class ActivePowerStrategy
    {
        public string Strategy { get; set; }
        public string Status { get; set; }
    }

    public class PowerSetpoint
    {
        public int Setpoint { get; set; }
    }
}