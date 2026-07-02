using Application.Common.Models;
using Application.Features.Payments.DTOs;
using MediatR;

namespace Application.Features.Payments.Commands.CreatePaymentIntent;


public sealed record CreatePaymentIntentCommand(long BookingId)
    : IRequest<ApiResponse<PaymentIntentResponse>>;
