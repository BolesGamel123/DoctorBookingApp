using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Chats.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Chats.Commands.MarkConversationRead;

public sealed record MarkConversationReadCommand(
    long ConversationId,
    string ReaderType,
    long ReaderId
) : IRequest<ApiResponse<string>>;

// This handler processes the MarkConversationReadCommand by marking all unread messages in a conversation as read for a specific reader (patient or doctor). It also resets the unread message count for the reader in the conversation. which indicates that the reader has read all messages in the conversation when he open it.
public sealed class MarkConversationReadCommandHandler
    : IRequestHandler<MarkConversationReadCommand, ApiResponse<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IChatRealtimeNotifier _chatRealtimeNotifier;

    public MarkConversationReadCommandHandler(IApplicationDbContext context, IChatRealtimeNotifier chatRealtimeNotifier)
    {
        _context = context;
        _chatRealtimeNotifier = chatRealtimeNotifier;
    }
    // This method handles the MarkConversationReadCommand by marking all unread messages in a conversation as read for a specific reader (patient or doctor). It also resets the unread message count for the reader in the conversation.
    public async Task<ApiResponse<string>> Handle(
        MarkConversationReadCommand request, CancellationToken cancellationToken)
    {
        //Step 1: جلب المحادثة
        var conversation = await _context.chat_conversations
            .FirstOrDefaultAsync(x => x.id == request.ConversationId, cancellationToken);

        if (conversation is null)
            return ApiResponse<string>.Fail("Conversation not found.");

        //Step 2: التأكد من أن القارئ صاحب المحادثة
        if (!ChatAccess.RequesterMatchesConversation(
                request.ReaderType, request.ReaderId, conversation.user_id, conversation.doctor_id))
            return ApiResponse<string>.Fail("You are not allowed to update this conversation.");

        var now = DateTime.UtcNow;

        //Step 3: جلب الرسائل غير المقروءة لهذا المستخدم فقط
        var unreadMessages = await _context.chat_messages
            .Where(x => x.conversation_id == request.ConversationId
                && x.receiver_type == request.ReaderType
                && x.receiver_id == request.ReaderId
                && !x.is_read)
            .ToListAsync(cancellationToken);

        //Step 4: تحديث الرسائل
        foreach (var message in unreadMessages)
        {
            message.is_read = true;
            message.read_at = now;
            message.updated_at = now;
        }

        //Step 5: تصفير unread count
        if (request.ReaderType == "patient")
            conversation.user_unread_count = 0;
        else
            conversation.doctor_unread_count = 0;

        conversation.updated_at = now;
        await _context.SaveChangesAsync(cancellationToken);

        await _chatRealtimeNotifier.ConversationReadAsync(
    request.ConversationId,
    request.ReaderType,
    request.ReaderId,
    cancellationToken);

        return ApiResponse<string>.Ok("Conversation marked as read.");
    }
}
