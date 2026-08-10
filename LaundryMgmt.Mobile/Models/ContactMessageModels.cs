namespace LaundryMgmt.Mobile.Models;

public enum ContactMessageType
{
    Feedback = 0,
    Complaint = 1,
    Query = 2
}

public enum ContactMessageStatus
{
    Open = 0,
    Resolved = 1
}

public record MyContactMessageDto(
    Guid Id, ContactMessageType Type, string Message, string? ImageUrl,
    ContactMessageStatus Status, string? Response, DateTimeOffset CreatedAtUtc);

public record CreateContactMessageRequest(ContactMessageType Type, string Message, string? ImageUrl);
