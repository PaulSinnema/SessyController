using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

/// <summary>
/// API client for interacting with the P1 Meter.
/// </summary>
public class P1MeterService
{
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="P1MeterService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
    public P1MeterService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Creates a configured HTTP client for interacting with the P1 Meter API.
    /// </summary>
    /// <param name="baseAddress">The base address of the P1 Meter API.</param>
    /// <param name="username">The username for authentication.</param>
    /// <param name="password">The password for authentication.</param>
    /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
    private HttpClient CreateHttpClient(string baseAddress, string username, string password)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseAddress);
        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        return client;
    }

    /// <summary>
    /// Retrieves the details of the P1 Meter.
    /// </summary>
    /// <param name="baseAddress">The base address of the P1 Meter API.</param>
    /// <param name="username">The username for authentication.</param>
    /// <param name="password">The password for authentication.</param>
    /// <returns>A <see cref="P1Details"/> object representing the details of the P1 Meter.</returns>
    public async Task<P1Details> GetP1DetailsAsync(string baseAddress, string username, string password)
    {
        using var client = CreateHttpClient(baseAddress, username, password);
        var response = await client.GetAsync("/api/v2/p1/details");

        // Ensure the response is successful
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<P1Details>(content);
    }

    /// <summary>
    /// Retrieves the current grid target from the P1 Meter.
    /// </summary>
    /// <param name="baseAddress">The base address of the P1 Meter API.</param>
    /// <param name="username">The username for authentication.</param>
    /// <param name="password">The password for authentication.</param>
    /// <returns>A <see cref="GridTarget"/> object representing the current grid target.</returns>
    public async Task<GridTarget> GetGridTargetAsync(string baseAddress, string username, string password)
    {
        using var client = CreateHttpClient(baseAddress, username, password);
        var response = await client.GetAsync("/api/v1/meter/grid_target");

        // Ensure the response is successful
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<GridTarget>(content);
    }

    /// <summary>
    /// Sets a new grid target on the P1 Meter.
    /// </summary>
    /// <param name="baseAddress">The base address of the P1 Meter API.</param>
    /// <param name="username">The username for authentication.</param>
    /// <param name="password">The password for authentication.</param>
    /// <param name="gridTarget">The new grid target to set.</param>
    /// <returns>An awaitable task representing the asynchronous operation.</returns>
    public async Task SetGridTargetAsync(string baseAddress, string username, string password, GridTargetPost gridTarget)
    {
        using var client = CreateHttpClient(baseAddress, username, password);
        var json = JsonConvert.SerializeObject(gridTarget);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/v1/meter/grid_target", content);

        // Ensure the response is successful
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Represents detailed information retrieved from the P1 Meter.
    /// </summary>
    public class P1Details
    {
        /// <summary>
        /// Status of the P1 Meter (e.g., "ok").
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Current state of the P1 Meter.
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }

        /// <summary>
        /// DSMR version used by the meter.
        /// </summary>
        [JsonProperty("dsmr_version")]
        public int DsmrVersion { get; set; }

        /// <summary>
        /// Power consumption for tariff 1 in watts.
        /// </summary>
        [JsonProperty("power_consumed_tariff1")]
        public long PowerConsumedTariff1 { get; set; }

        /// <summary>
        /// Power production for tariff 1 in watts.
        /// </summary>
        [JsonProperty("power_produced_tariff1")]
        public long PowerProducedTariff1 { get; set; }

        /// <summary>
        /// Power consumption for tariff 2 in watts.
        /// </summary>
        [JsonProperty("power_consumed_tariff2")]
        public long PowerConsumedTariff2 { get; set; }

        /// <summary>
        /// Power production for tariff 2 in watts.
        /// </summary>
        [JsonProperty("power_produced_tariff2")]
        public long PowerProducedTariff2 { get; set; }

        /// <summary>
        /// The current tariff indicator (e.g., 1 or 2).
        /// </summary>
        [JsonProperty("tariff_indicator")]
        public int TariffIndicator { get; set; }

        /// <summary>
        /// Current power consumption in watts.
        /// </summary>
        [JsonProperty("power_consumed")]
        public int PowerConsumed { get; set; }

        /// <summary>
        /// Current power production in watts.
        /// </summary>
        [JsonProperty("power_produced")]
        public int PowerProduced { get; set; }

        /// <summary>
        /// Total power in watts.
        /// </summary>
        [JsonProperty("power_total")]
        public int PowerTotal { get; set; }

        // Add additional fields similarly with appropriate JsonProperty attributes.
    }

    /// <summary>
    /// Represents the current grid target settings of the P1 Meter.
    /// </summary>
    public class GridTarget
    {
        /// <summary>
        /// The current grid target value in watts.
        /// </summary>
        [JsonProperty("grid_target_value")]
        public int GridTargetValue { get; set; }
    }

    /// <summary>
    /// Represents the payload for setting a new grid target on the P1 Meter.
    /// </summary>
    public class GridTargetPost
    {
        /// <summary>
        /// The desired grid target value in watts.
        /// </summary>
        [JsonProperty("grid_target_value")]
        public int GridTargetValue { get; set; }
    }
}
