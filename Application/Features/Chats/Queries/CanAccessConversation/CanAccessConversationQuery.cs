using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Chats.Queries.CanAccessConversation;

public sealed record CanAccessConversationQuery(
    long ConversationId,
    string RequesterType,
    long RequesterId
) : IRequest<bool>;

/// <summary>
/// This handler checks if a requester (patient or doctor) has access to a specific chat conversation. 
/// </summary>
public sealed class CanAccessConversationQueryHandler
    : IRequestHandler<CanAccessConversationQuery, bool>
{
    private readonly IApplicationDbContext _context;

    public CanAccessConversationQueryHandler(IApplicationDbContext context)
        => _context = context;

    // This method handles the CanAccessConversationQuery by checking if the requester has access to the specified conversation.
    public async Task<bool> Handle(
        CanAccessConversationQuery request, CancellationToken cancellationToken)
    {
        // Step 1: Fetch the conversation from the database
        var conversation = await _context.chat_conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.id == request.ConversationId, cancellationToken);

        // Step 2: Check if the requester matches the conversation participants
        return conversation is not null
            && ChatAccess.RequesterMatchesConversation(
                request.RequesterType, request.RequesterId, conversation.user_id, conversation.doctor_id);
    }
}
