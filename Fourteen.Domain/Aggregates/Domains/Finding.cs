using System.Security.Cryptography;
using Fourteen.Domain.Common;
using Fourteen.Domain.EventHandlers;

namespace Fourteen.Domain.Aggregates.Domains
{
    public class Finding : AggregateRoot<FindingId>
    {
        public ScanId ScanId { get; private set; } = default!;
        public FindingType Type { get; private set; }
        public Severity Severity { get; private set; }
        public string Title { get; private set; } = default!;
        public string RawData { get; private set; } = default!;  // JSON blob from worker
        public string? AiExplanation { get; private set; }
        public string? AiRecommendation { get; private set; }
        public DateTime CreatedAt { get; private set; }


        public static Finding Create(ScanId scanId, FindingType type, Severity severity,
            string title, string rawData) => new()
            {
                Id = FindingId.New(),
                ScanId = scanId,
                Type = type,
                Severity = severity,
                Title = title,
                RawData = rawData,
                CreatedAt = DateTime.UtcNow
            };

        public void AttachAiInsight(string explanation, string recommendation)
        {
            AiExplanation = explanation;
            AiRecommendation = recommendation;
        }

    }

}