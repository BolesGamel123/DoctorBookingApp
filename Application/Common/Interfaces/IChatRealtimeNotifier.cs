using Application.Features.Chats.DTOs;

namespace Application.Features.Chats.Abstractions;

public interface IChatRealtimeNotifier
{
    Task MessageReceivedAsync(
        long conversationId,
        ChatMessageResponse message,
        CancellationToken cancellationToken = default);

    Task ConversationReadAsync(
        long conversationId,
        string readerType,
        long readerId,
        CancellationToken cancellationToken = default);
}