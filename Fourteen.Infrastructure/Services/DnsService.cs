using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DnsClient;
using DnsClient.Protocol;
using Fourteen.Application.Interfaces;

namespace Fourteen.Infrastructure.Services
{
    public class DnsServices : IDnsService
    {
        private readonly ILogger<DnsServices> _logger;
        private readonly LookupClient _dnsClient;


        public DnsServices(ILogger<DnsServices> logger, LookupClient dnsClient)
        {
            _logger = logger;
            _dnsClient = dnsClient;
        }

        public async Task<bool> CheckTxtRecord(string host, string expectedValue, CancellationToken ct)
        {
            try
            {
                 if (string.IsNullOrWhiteSpace(host))
                    throw new ArgumentException("Host is required", nameof(host));

                var result = await _dnsClient.QueryAsync(host, QueryType.TXT, cancellationToken: ct);

                var txtRecords = result.Answers.TxtRecords();

                // _logger.LogInformation("TXT records found for {Host}: {Records}",
                //     host,
                //     string.Join(", ", txtRecords.Select(r => string.Join("", r.Text))));

                foreach (var record in txtRecords)
                {
                    var value = string.Join("", record.Text);

                    if (value.Equals(expectedValue, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking DNS TXT record for host: {Host}", host);
                return false;
            }
        }
    }
}