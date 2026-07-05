using Application.Features.Chats.Abstractions;
using Application.Features.Chats.DTOs;
using DoctorBooking.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DoctorBooking.Realtime;

public sealed class SignalRChatRealtimeNotifier : IChatRealtimeNotifier
{
    private readonly IHubContext<ChatHub> _hubContext;

    public SignalRChatRealtimeNotifier(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task MessageReceivedAsync(
        long conversationId,
        ChatMessageResponse message,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(GroupName(conversationId))
            .SendAsync("MessageReceived", message, cancellationToken);
    }

    public Task ConversationReadAsync(
        long conversationId,
        string readerType,
        long readerId,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(GroupName(conversationId))
            .SendAsync("ConversationRead", new
            {
                ConversationId = conversationId,
                ReaderType = readerType,
                ReaderId = readerId
            }, cancellationToken);
    }

    private static string GroupName(long conversationId) => $"chat:{conversationId}";
}