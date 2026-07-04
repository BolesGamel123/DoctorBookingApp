using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Chats;

internal static class ChatAccess
{
    //تتأكد أن الشخص الذي يحاول يدخل المحادثة هو فعلاً صاحبها.
    public static bool RequesterMatchesConversation(
        string requesterType, long requesterId, long patientId, long doctorId)
        => requesterType switch
        {
            "patient" => requesterId == patientId,
            "doctor" => requesterId == doctorId,
            _ => false
        };

    //لا توجد دردشة إلا إذا كان هناك booking بين patient و doctor.
    public static Task<bool> HasBookingAsync(
        IApplicationDbContext context, long patientId, long doctorId, CancellationToken cancellationToken)

        => context.bookings.AnyAsync(x =>
            x.user_id == patientId
            && x.doctor_id == doctorId
            && x.status != "cancelled",
            cancellationToken);
}
