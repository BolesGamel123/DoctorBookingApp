using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Chats.Abstractions;
using Application.Features.Chats.DTOs;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Chats.Commands.SendMessage;

public sealed record SendMessageCommand(
    long PatientId,
    long DoctorId,
    string SenderType,
    long SenderId,
    string Body,
    string MessageType = "text",
    long? ReplyToMessageId = null
) : IRequest<ApiResponse<ChatMessageResponse>>;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.DoctorId).GreaterThan(0);
        RuleFor(x => x.SenderId).GreaterThan(0);
        RuleFor(x => x.SenderType).Must(x => x is "patient" or "doctor");
        RuleFor(x => x.MessageType).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
    }
}

/// <summary>
/// This handler processes the SendMessageCommand by validating the sender's access to the conversation, checking for an existing booking between the patient and doctor, creating a new conversation if necessary, and storing the message in the database. It also updates the conversation's last message information and unread message counts accordingly.
/// </summary>
public sealed class SendMessageCommandHandler
    : IRequestHandler<SendMessageCommand, ApiResponse<ChatMessageResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IChatRealtimeNotifier _chatRealtimeNotifier;

    public SendMessageCommandHandler(IApplicationDbContext context, IChatRealtimeNotifier chatRealtimeNotifier)
    {
        _context = context; 
        _chatRealtimeNotifier = chatRealtimeNotifier; 
    }


    public async Task<ApiResponse<ChatMessageResponse>> Handle(
        SendMessageCommand request, CancellationToken cancellationToken)
    {
        // Validate that the requester is either the patient or the doctor in the conversation
        if (!ChatAccess.RequesterMatchesConversation(request.SenderType, request.SenderId, request.PatientId, request.DoctorId))
            return ApiResponse<ChatMessageResponse>.Fail("You are not allowed to send messages in this conversation.");

        // Check if the patient has a booking with the doctor before allowing them to send a message
        var hasBooking = await ChatAccess.HasBookingAsync(
            _context, request.PatientId, request.DoctorId, cancellationToken);

        if (!hasBooking)
            return ApiResponse<ChatMessageResponse>.Fail(
                "Chat is available only after the patient books this doctor.");

        var now = DateTime.UtcNow;

        // Check if a conversation already exists between the patient and doctor
        var conversation = await _context.chat_conversations
            .FirstOrDefaultAsync(x => x.user_id == request.PatientId && x.doctor_id == request.DoctorId, cancellationToken);

        // If no conversation exists, create a new one
        if (conversation is null)
        {
            conversation = new chat_conversation
            {
                user_id = request.PatientId,
                doctor_id = request.DoctorId,
                created_at = now,
                updated_at = now
            };

            _context.chat_conversations.Add(conversation);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Determine the receiver's type and ID based on the sender's type
        var receiverType = request.SenderType == "patient" ? "doctor" : "patient";
        var receiverId = request.SenderType == "patient" ? request.DoctorId : request.PatientId;

        // Create a new chat message with the provided details
        var message = new chat_message
        {
            conversation_id = conversation.id,
            sender_type = request.SenderType,
            sender_id = request.SenderId,
            receiver_type = receiverType,
            receiver_id = receiverId,
            message_type = request.MessageType,
            body = request.Body.Trim(),
            reply_to_message_id = request.ReplyToMessageId,
            is_read = false,
            created_at = now,
            updated_at = now
        };

        // Add the new message to the database and save changes
        _context.chat_messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        // Update the conversation's last message information and unread message counts
        conversation.last_message_id = message.id;
        conversation.last_message_at = message.created_at;
        conversation.updated_at = now;
        if (receiverType == "patient")
            conversation.user_unread_count++;
        else
            conversation.doctor_unread_count++;

        await _context.SaveChangesAsync(cancellationToken);


        var response = ChatMapper.ToMessageResponse(message);

        await _chatRealtimeNotifier.MessageReceivedAsync(
            response.ConversationId,
            response,
            cancellationToken);

        return ApiResponse<ChatMessageResponse>.Ok(response, "Message sent.");
    }
}
