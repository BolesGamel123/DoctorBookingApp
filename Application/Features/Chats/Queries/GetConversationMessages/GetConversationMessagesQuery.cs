using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Chats.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Chats.Queries.GetConversationMessages;

public sealed record GetConversationMessagesQuery(
    long ConversationId,
    string RequesterType,
    long RequesterId,
    int Page = 1,
    int PageSize = 50
) : IRequest<ApiResponse<PagedResult<ChatMessageResponse>>>;

//هذا يجلب رسائل conversation معينة مع pagination.
public sealed class GetConversationMessagesQueryHandler
    : IRequestHandler<GetConversationMessagesQuery, ApiResponse<PagedResult<ChatMessageResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetConversationMessagesQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<ApiResponse<PagedResult<ChatMessageResponse>>> Handle(
        GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        //Step 1: جلب المحادثة
        var conversation = await _context.chat_conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.id == request.ConversationId, cancellationToken);

        //Step 2: التحقق من وجود المحادثة
        if (conversation is null)
            return ApiResponse<PagedResult<ChatMessageResponse>>.Fail("Conversation not found.");

        //Step 3: التحقق من صلاحية الوصول
        if (!ChatAccess.RequesterMatchesConversation(
                request.RequesterType, request.RequesterId, conversation.user_id, conversation.doctor_id))
            return ApiResponse<PagedResult<ChatMessageResponse>>.Fail("You are not allowed to view this conversation.");

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        //Step 4: جلب الرسائل مع pagination
        var query = _context.chat_messages
            .AsNoTracking()
            .Where(x => x.conversation_id == request.ConversationId)
            .OrderByDescending(x => x.created_at);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(x => x.created_at)
            .Select(x => ChatMapper.ToMessageResponse(x))
            .ToListAsync(cancellationToken);

        //Step 6: إرجاع النتيجة مع pagination 
        return ApiResponse<PagedResult<ChatMessageResponse>>.Ok(new PagedResult<ChatMessageResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }
}
