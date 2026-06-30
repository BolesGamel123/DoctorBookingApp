using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

/// <summary>
/// Abstraction over the database context. Implemented by Infrastructure.
/// This ensures the Application layer never depends on the concrete DbContext.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<admin> admins { get; }
    DbSet<announcement> announcements { get; }
    DbSet<booking> bookings { get; }
    DbSet<chat_conversation> chat_conversations { get; }
    DbSet<chat_message> chat_messages { get; }
    DbSet<clinic> clinics { get; }
    DbSet<doctor> doctors { get; }
    DbSet<doctor_availability_slot> doctor_availability_slots { get; }
    DbSet<faq> faqs { get; }
    DbSet<favorite> favorites { get; }
    DbSet<notification> notifications { get; }
    DbSet<notification_preference> notification_preferences { get; }
    DbSet<payment> payments { get; }
    DbSet<review> reviews { get; }
    DbSet<review_response> review_responses { get; }
    DbSet<search_history> search_histories { get; }
    DbSet<specialty> specialties { get; }
    DbSet<system_log> system_logs { get; }
    DbSet<patient> patients { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
