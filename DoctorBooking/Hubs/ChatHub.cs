using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Features.Chats.Commands.MarkConversationRead;
using Application.Features.Chats.Commands.SendMessage;
using Application.Features.Chats.Queries.CanAccessConversation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DoctorBooking.Hubs;

[Authorize(Roles = "Patient,Doctor")]
public sealed class ChatHub : Hub
{
    private readonly ISender _mediator;

    public ChatHub(ISender mediator) => _mediator = mediator;

    public async Task JoinConversation(long conversationId)
    {
        var requester = GetRequester();

        var canAccess = await _mediator.Send(new CanAccessConversationQuery(
            conversationId, requester.Type, requester.Id));

        if (!canAccess)
            throw new HubException("You are not allowed to join this conversation.");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversationId));
    }

    public async Task SendMessage(long patientId, long doctorId, string body)
    {
        var requester = GetRequester();

        var result = await _mediator.Send(new SendMessageCommand(
            patientId, doctorId, requester.Type, requester.Id, body));

        if (!result.Success || result.Data is null)
            throw new HubException(result.Message);
    }

    public async Task MarkConversationRead(long conversationId)
    {
        var requester = GetRequester();

        // Mark the conversation as read using the MarkConversationReadCommand
        var result = await _mediator.Send(new MarkConversationReadCommand(
            conversationId, requester.Type, requester.Id));

        if (!result.Success)
            throw new HubException(result.Message);

        // Notify all clients in the conversation group that the conversation has been marked as read
        await Clients.Group(GroupName(conversationId))
            .SendAsync("ConversationRead", new { ConversationId = conversationId, ReaderType = requester.Type, ReaderId = requester.Id });
    }


    private (string Type, long Id) GetRequester()
    {
        var user = Context.User ?? throw new HubException("Unauthenticated.");

        var idClaim =
            user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(idClaim))
            throw new HubException("User id claim was not found in token.");

        var id = long.Parse(idClaim);

        var type = user.IsInRole("Doctor") ? "doctor" : "patient";
        return (type, id);
    }

    private static string GroupName(long conversationId) => $"chat:{conversationId}";
}
