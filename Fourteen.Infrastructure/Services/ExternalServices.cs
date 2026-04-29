using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Infrastructure.Services
{
    public class ExternalServices : IServices
    {

        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<ExternalServices> _logger;

        public ExternalServices(IHttpClientFactory httpFactory, ILogger<ExternalServices> logger)
        {
            _httpFactory = httpFactory;
            _logger = logger;
        }

        public async Task<ExternalAPIDto> GetByName(string name, CancellationToken ct)
        {
            var genderClient = _httpFactory.CreateClient("genderize");
            HttpResponseMessage response;

            try
            {
                response = await genderClient.GetAsync(
                    $"?name={Uri.EscapeDataString(name)}", ct);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error calling external API");
                throw new UpstreamApiException("Upstream service is unreachable.", 502);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "External API timed out");
                throw new UpstreamApiException("Upstream service timed out.", 502);
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("External API returned {Status}", response.StatusCode);
                throw new UpstreamApiException(
                    $"Upstream returned {(int)response.StatusCode}.",
                    (int)response.StatusCode);
            }

            return await response.Content.ReadFromJsonAsync<ExternalAPIDto>(ct)
                   ?? throw new UpstreamApiException("Empty response from upstream.");
        }

        public async Task<TransformedExternalResponse> FetchAll(string name, CancellationToken ct)
        {
            var genderClient = _httpFactory.CreateClient("genderize");
            var agifyClient = _httpFactory.CreateClient("agify");
            var natClient = _httpFactory.CreateClient("nationalize");

            var encodedName = Uri.EscapeDataString(name);

            var (genderJson, agifyJson, natJson) = await FetchAllRaw(
          genderClient, agifyClient, natClient, encodedName, ct);

            //_logger.LogInformation("Genderize raw: {Json}", genderJson);
            //_logger.LogInformation("Agify raw:     {Json}", agifyJson);
            // _logger.LogInformation("Nationalize raw: {Json}", natJson);

            var genderData = System.Text.Json.JsonSerializer
                .Deserialize<GenderizeResponse>(genderJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new Exception("Failed to deserialize Genderize response");

            var agifyData = System.Text.Json.JsonSerializer
                .Deserialize<AgifyResponse>(agifyJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new Exception("Failed to deserialize Agify response");

            var natData = System.Text.Json.JsonSerializer
                .Deserialize<NationalizeResponse>(natJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new Exception("Failed to deserialize Nationalize response");

            if (genderData?.Gender == null || genderData.Count == 0)
                throw new UpstreamApiException("Genderize returned an invalid response");

            if (agifyData?.Age == null)
                throw new UpstreamApiException("Agify returned an invalid response");

            if (natData?.Country == null || !natData.Country.Any())
                throw new UpstreamApiException("Nationalize returned an invalid response");

            return new TransformedExternalResponse
            {
                Gender = genderData.Gender,
                GenderProbability = genderData.Probability,
                Name = genderData.Name,
                Age = agifyData.Age,
                Country_Id = natData.Country.First().Country_Id,
                CountryProbability = natData.Country.First().Probability,
                Count = genderData.Count
            };

        }

        private static async Task<(string gender, string agify, string nat)> FetchAllRaw(
          HttpClient genderClient,
          HttpClient agifyClient,
          HttpClient natClient,
          string encodedName,
          CancellationToken ct)
        {
            var genderTask = genderClient.GetStringAsync($"?name={encodedName}", ct);
            var agifyTask = agifyClient.GetStringAsync($"?name={encodedName}", ct);
            var natTask = natClient.GetStringAsync($"?name={encodedName}", ct);

            await Task.WhenAll(genderTask, agifyTask, natTask);

            return (await genderTask, await agifyTask, await natTask);
        }



    }
}
