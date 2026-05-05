using Fourteen.Domain.Common;
using MediatR;


namespace Fourteen.Domain.EventHandlers
{ 
    /// <summary>
    /// Raised when a domain is verified.
    /// </summary>
    public record ScanCompletedEvent(ScanId ScanId, DomainId DomainId, UserId RequestedBy) : DomainEvent;

    public class ScanCompletedEventHandler
       : INotificationHandler<ScanCompletedEvent>
    {
        // private readonly IEmailService _emailService;

        // public ScanCompletedEventHandler(IEmailService emailService)
        // {
        //     _emailService = emailService;
        // }
        public Task Handle(ScanCompletedEvent notification, CancellationToken cancellationToken)
        {
            // Here you can implement any logic that should happen after a domain is verified.
            // For example, you might want to send a notification email to the user, log the event, etc.

            Console.WriteLine($"Domain with ID {notification.DomainId} has been verified for user {notification.RequestedBy}.");

            return Task.CompletedTask;
        }
    }
}