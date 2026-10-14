using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Features.Chats.Commands.MarkConversationRead;
using Application.Features.Chats.Commands.SendMessage;
using Application.Features.Chats.Queries.GetConversationMessages;
using Application.Features.Chats.Queries.GetConversations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.Controllers;

[ApiController]
[Authorize(Roles = "Patient,Doctor")]
[Route("api/[controller]")]
public sealed class ChatController : ControllerBase
{
    private readonly ISender _mediator;

    public ChatController(ISender mediator) => _mediator = mediator;

    private (string Type, long Id) GetRequester()
    {
        // Retrieve the user ID from the JWT token claims
        var idClaim =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(idClaim))
            throw new UnauthorizedAccessException("User id claim was not found in token.");

        var id = long.Parse(idClaim);

        // Determine the user type based on their role (Doctor or Patient)
        var type = User.IsInRole("Doctor") ? "doctor" : "patient";
        return (type, id);
    }


    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var requester = GetRequester();
        var result = await _mediator.Send(new GetConversationsQuery(requester.Type, requester.Id));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("conversations/{conversationId:long}/messages")]
    public async Task<IActionResult> GetMessages(
        long conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var requester = GetRequester();
        var result = await _mediator.Send(new GetConversationMessagesQuery(
            conversationId, requester.Type, requester.Id, page, pageSize));

        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpPost("doctors/{doctorId:long}/messages")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> SendToDoctor(long doctorId, [FromBody] SendChatMessageRequest request)
    {
        var requester = GetRequester();
        var result = await _mediator.Send(new SendMessageCommand(
            requester.Id, doctorId, requester.Type, requester.Id, request.Body, request.MessageType, request.ReplyToMessageId));

        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpPost("patients/{patientId:long}/messages")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> SendToPatient(long patientId, [FromBody] SendChatMessageRequest request)
    {
        var requester = GetRequester();
        var result = await _mediator.Send(new SendMessageCommand(
            patientId, requester.Id, requester.Type, requester.Id, request.Body, request.MessageType, request.ReplyToMessageId));

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("conversations/{conversationId:long}/read")]
    public async Task<IActionResult> MarkRead(long conversationId)
    {
        var requester = GetRequester();
        var result = await _mediator.Send(new MarkConversationReadCommand(
            conversationId, requester.Type, requester.Id));

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
public sealed record SendChatMessageRequest(
    string Body,
    string MessageType = "text",
    long? ReplyToMessageId = null);
