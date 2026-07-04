using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Chats.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.Chats.Queries.GetConversations;

public sealed record GetConversationsQuery(
    string RequesterType,
    long RequesterId
) : IRequest<ApiResponse<IReadOnlyList<ChatConversationResponse>>>;

//هذا يجلب كل محادثات المستخدم الحالي.
public sealed class GetConversationsQueryHandler
    : IRequestHandler<GetConversationsQuery, ApiResponse<IReadOnlyList<ChatConversationResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetConversationsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<ApiResponse<IReadOnlyList<ChatConversationResponse>>> Handle(
        GetConversationsQuery request, CancellationToken cancellationToken)
    {
        //Step 1: validate requester type
        if (request.RequesterType is not ("patient" or "doctor"))
            return ApiResponse<IReadOnlyList<ChatConversationResponse>>.Fail("Invalid requester type.");

        //Step 2: Include البيانات المطلوبة
        var query = _context.chat_conversations
            .AsNoTracking()
            .Include(x => x.patient)
            .Include(x => x.doctor)
            .Include(x => x.last_message)
            .AsQueryable();

        //Step 3: فلترة حسب المستخدم (patient أو doctor) وحالة الأرشفة
        query = request.RequesterType == "patient"
            ? query.Where(x => x.user_id == request.RequesterId && !x.is_archived_by_user)
            : query.Where(x => x.doctor_id == request.RequesterId && !x.is_archived_by_doctor);

        //Step 4: ترتيب المحادثات
        var conversations = await query
            .OrderByDescending(x => x.last_message_at ?? x.updated_at)
            .Select(x => ChatMapper.ToConversationResponse(x, request.RequesterType))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<ChatConversationResponse>>.Ok(conversations);
    }
}
