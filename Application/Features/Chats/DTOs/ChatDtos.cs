namespace Application.Features.Chats.DTOs;

public sealed class ChatMessageResponse
{
    public long Id { get; init; }
    public long ConversationId { get; init; }
    public string SenderType { get; init; } = string.Empty;
    public long SenderId { get; init; }
    public string ReceiverType { get; init; } = string.Empty;
    public long ReceiverId { get; init; }
    public string MessageType { get; init; } = string.Empty;
    public string? Body { get; init; }
    public bool IsRead { get; init; }
    public DateTime? ReadAt { get; init; }
    public long? ReplyToMessageId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class ChatConversationResponse
{
    public long Id { get; init; }
    public long PatientId { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public long DoctorId { get; init; }
    public string DoctorName { get; init; } = string.Empty;
    public DateTime? LastMessageAt { get; init; }
    public ChatMessageResponse? LastMessage { get; init; }
    public short UnreadCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
