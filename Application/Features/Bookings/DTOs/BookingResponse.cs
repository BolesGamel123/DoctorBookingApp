namespace Application.Features.Bookings.DTOs;

public sealed class BookingResponse
{
    public long   Id               { get; init; }
    public string BookingReference { get; init; } = default!;
    public long   SlotId           { get; init; }
    public long   DoctorId         { get; init; }
    public string DoctorName       { get; init; } = default!;
    public string Status           { get; init; } = default!;
    public string PaymentMethod    { get; init; } = default!;
    public decimal SessionPrice    { get; init; }
    public string Currency         { get; init; } = default!;
    public DateOnly BookedDate     { get; init; }
    public TimeOnly StartTime      { get; init; }
    public TimeOnly EndTime        { get; init; }
    public string? NotesForDoctor  { get; init; }
    public DateTime CreatedAt      { get; init; }
}
