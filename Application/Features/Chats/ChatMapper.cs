using Application.Features.Chats.DTOs;
using Domain.Entities;

namespace Application.Features.Chats;

internal static class ChatMapper
{
    public static ChatMessageResponse ToMessageResponse(chat_message message) => new()
    {
        Id = message.id,
        ConversationId = message.conversation_id,
        SenderType = message.sender_type,
        SenderId = message.sender_id,
        ReceiverType = message.receiver_type,
        ReceiverId = message.receiver_id,
        MessageType = message.message_type,
        Body = message.body,
        IsRead = message.is_read,
        ReadAt = message.read_at,
        ReplyToMessageId = message.reply_to_message_id,
        CreatedAt = message.created_at
    };

    public static ChatConversationResponse ToConversationResponse(
        chat_conversation conversation, string viewerType) => new()
    {
        Id = conversation.id,
        PatientId = conversation.user_id,
        PatientName = conversation.patient?.full_name ?? string.Empty,
        DoctorId = conversation.doctor_id,
        DoctorName = conversation.doctor?.full_name ?? string.Empty,
        LastMessageAt = conversation.last_message_at,
        LastMessage = conversation.last_message is null ? null : ToMessageResponse(conversation.last_message),
        UnreadCount = viewerType == "patient"
            ? conversation.user_unread_count
            : conversation.doctor_unread_count,
        CreatedAt = conversation.created_at,
        UpdatedAt = conversation.updated_at
    };
}
