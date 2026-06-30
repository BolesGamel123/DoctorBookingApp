#nullable disable
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public partial class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public virtual DbSet<admin> admins { get; set; }
    public virtual DbSet<announcement> announcements { get; set; }
    public virtual DbSet<booking> bookings { get; set; }
    public virtual DbSet<chat_conversation> chat_conversations { get; set; }
    public virtual DbSet<chat_message> chat_messages { get; set; }
    public virtual DbSet<clinic> clinics { get; set; }
    public virtual DbSet<doctor> doctors { get; set; }
    public virtual DbSet<doctor_availability_slot> doctor_availability_slots { get; set; }
    public virtual DbSet<faq> faqs { get; set; }
    public virtual DbSet<favorite> favorites { get; set; }
    public virtual DbSet<notification> notifications { get; set; }
    public virtual DbSet<notification_preference> notification_preferences { get; set; }
    public virtual DbSet<payment> payments { get; set; }
    public virtual DbSet<review> reviews { get; set; }
    public virtual DbSet<review_response> review_responses { get; set; }
    public virtual DbSet<search_history> search_histories { get; set; }
    public virtual DbSet<specialty> specialties { get; set; }
    public virtual DbSet<system_log> system_logs { get; set; }
    public virtual DbSet<patient> patients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Admin ─────────────────────────────────────────────
        modelBuilder.Entity<admin>(entity =>
        {
            entity.HasIndex(e => e.email, "UQ_admins_email").IsUnique();
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.role).HasDefaultValue("admin");
            entity.Property(e => e.two_factor_enabled).HasDefaultValue(true);
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
        });

        // ── Announcement ──────────────────────────────────────
        modelBuilder.Entity<announcement>(entity =>
        {
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.target_audience).HasDefaultValue("all");
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.admin).WithMany(p => p.announcements)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_ann_admin");
        });

        // ── Booking ───────────────────────────────────────────
        modelBuilder.Entity<booking>(entity =>
        {
            entity.HasIndex(e => e.booked_date, "IX_bookings_date");
            entity.HasIndex(e => new { e.doctor_id, e.status }, "IX_bookings_doctor");
            entity.HasIndex(e => e.availability_slot_id, "IX_bookings_slot");
            entity.HasIndex(e => new { e.user_id, e.status }, "IX_bookings_user");
            entity.HasIndex(e => e.booking_reference, "UQ_bookings_reference").IsUnique();
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.currency).HasDefaultValue("USD");
            entity.Property(e => e.status).HasDefaultValue("pending");
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.availability_slot).WithMany(p => p.bookings)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_bookings_slot");
            entity.HasOne(d => d.clinic).WithMany(p => p.bookings)
                .OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_bookings_clinic");
            entity.HasOne(d => d.doctor).WithMany(p => p.bookings)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_bookings_doctor");
            entity.HasOne(d => d.original_booking).WithMany(p => p.Inverseoriginal_booking)
                .HasConstraintName("FK_bookings_original");
            entity.HasOne(d => d.specialty).WithMany(p => p.bookings)
                .OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_bookings_specialty");
            entity.HasOne(d => d.patient).WithMany(p => p.bookings)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_bookings_user");
        });

        // ── Chat Conversation ─────────────────────────────────
        modelBuilder.Entity<chat_conversation>(entity =>
        {
            entity.HasIndex(e => new { e.doctor_id, e.last_message_at }, "IX_cc_doctor_inbox")
                .IsDescending(false, true);
            entity.HasIndex(e => new { e.user_id, e.last_message_at }, "IX_cc_user_inbox")
                .IsDescending(false, true);
            entity.HasIndex(e => new { e.user_id, e.doctor_id }, "UQ_chat_conversations").IsUnique();
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.doctor).WithMany(p => p.chat_conversations)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_cc_doctor");
            entity.HasOne(d => d.last_message).WithMany(p => p.chat_conversations)
                .OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_cc_last_message");
            entity.HasOne(d => d.patient).WithMany(p => p.chat_conversations)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_cc_user");
        });

        // ── Chat Message ──────────────────────────────────────
        modelBuilder.Entity<chat_message>(entity =>
        {
            entity.HasIndex(e => new { e.conversation_id, e.created_at }, "IX_cm_conversation")
                .IsDescending(false, true);
            entity.HasIndex(e => new { e.sender_type, e.sender_id }, "IX_cm_sender");
            entity.HasIndex(e => new { e.is_read, e.receiver_id }, "IX_cm_unread");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.message_type).HasDefaultValue("text");
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.conversation).WithMany(p => p.chat_messages)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_cm_conversation");
            entity.HasOne(d => d.reply_to_message).WithMany(p => p.Inversereply_to_message)
                .HasConstraintName("FK_cm_reply_to");
        });

        // ── Clinic ────────────────────────────────────────────
        modelBuilder.Entity<clinic>(entity =>
        {
            entity.HasIndex(e => e.city, "IX_clinics_city");
            entity.HasIndex(e => new { e.latitude, e.longitude }, "IX_clinics_location");
            entity.Property(e => e.country).HasDefaultValue("Egypt");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.created_byNavigation).WithMany(p => p.clinics)
                .OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_clinics_admin");
        });

        // ── Doctor ────────────────────────────────────────────
        modelBuilder.Entity<doctor>(entity =>
        {
            entity.HasIndex(e => e.rating_avg, "IX_doctors_rating").IsDescending();
            entity.HasIndex(e => e.specialty_id, "IX_doctors_specialty");
            entity.HasIndex(e => new { e.is_active, e.is_verified }, "IX_doctors_status");
            entity.HasIndex(e => e.email, "UQ_doctors_email").IsUnique();
            entity.HasIndex(e => e.license_number, "UQ_doctors_license").IsUnique();
            entity.HasIndex(e => e.mobile_number, "UQ_doctors_mobile").IsUnique();
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.currency).HasDefaultValue("USD");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.must_change_password).HasDefaultValue(true);
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.admin).WithMany(p => p.doctors)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_doctors_admin");
            entity.HasOne(d => d.clinic).WithMany(p => p.doctors)
                .OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_doctors_clinic");
            entity.HasOne(d => d.specialty).WithMany(p => p.doctors)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_doctors_spec");
        });

        // ── Doctor Availability Slot ──────────────────────────
        modelBuilder.Entity<doctor_availability_slot>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK_das");
            entity.HasIndex(e => new { e.doctor_id, e.slot_date, e.status }, "IX_das_query");
            entity.HasIndex(e => new { e.doctor_id, e.slot_date, e.slot_start_time }, "UQ_das_slot").IsUnique();
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.duration_minutes).HasDefaultValue((short)60);
            entity.Property(e => e.recurrence_type).HasDefaultValue("none");
            entity.Property(e => e.status).HasDefaultValue("available");
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.booking).WithMany(p => p.doctor_availability_slots)
                .OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_das_booking");
            entity.HasOne(d => d.doctor).WithMany(p => p.doctor_availability_slots)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_das_doctor");
        });

        // ── FAQ ───────────────────────────────────────────────
        modelBuilder.Entity<faq>(entity =>
        {
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.created_byNavigation).WithMany(p => p.faqcreated_byNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_faqs_creator");
            entity.HasOne(d => d.updated_byNavigation).WithMany(p => p.faqupdated_byNavigations)
                .HasConstraintName("FK_faqs_updater");
        });

        // ── Favorite ──────────────────────────────────────────
        modelBuilder.Entity<favorite>(entity =>
        {
            entity.HasIndex(e => e.user_id, "IX_favorites_user");
            entity.HasIndex(e => new { e.user_id, e.doctor_id }, "UQ_favorites_pair").IsUnique();
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.doctor).WithMany(p => p.favorites)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_favorites_doctor");
            entity.HasOne(d => d.patient).WithMany(p => p.favorites)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_favorites_user");
        });

        // ── Notification ──────────────────────────────────────
        modelBuilder.Entity<notification>(entity =>
        {
            entity.HasIndex(e => new { e.notifiable_type, e.notifiable_id, e.is_read }, "IX_notif_recipient");
            entity.HasIndex(e => e.type, "IX_notif_type");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.delivery_status).HasDefaultValue("pending");
        });

        // ── Notification Preference ───────────────────────────
        modelBuilder.Entity<notification_preference>(entity =>
        {
            entity.HasIndex(e => new { e.notifiable_type, e.notifiable_id }, "UQ_np_actor").IsUnique();
            entity.Property(e => e.booking_cancelled).HasDefaultValue(true);
            entity.Property(e => e.booking_confirmed).HasDefaultValue(true);
            entity.Property(e => e.booking_rescheduled).HasDefaultValue(true);
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.new_message).HasDefaultValue(true);
            entity.Property(e => e.new_review).HasDefaultValue(true);
            entity.Property(e => e.payment_received).HasDefaultValue(true);
            entity.Property(e => e.promotional).HasDefaultValue(true);
            entity.Property(e => e.reminder_hours_before).HasDefaultValue((byte)24);
            entity.Property(e => e.upcoming_appointment_reminder).HasDefaultValue(true);
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
        });

        // ── Payment ───────────────────────────────────────────
        modelBuilder.Entity<payment>(entity =>
        {
            entity.HasIndex(e => e.doctor_id, "IX_payments_doctor");
            entity.HasIndex(e => new { e.user_id, e.status }, "IX_payments_user");
            entity.HasIndex(e => e.booking_id, "UQ_payments_booking").IsUnique();
            entity.HasIndex(e => e.gateway_transaction_id, "UQ_payments_gateway_txn").IsUnique();
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.currency).HasDefaultValue("USD");
            entity.Property(e => e.status).HasDefaultValue("pending");
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.booking).WithOne(p => p.payment)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_payments_booking");
            entity.HasOne(d => d.doctor).WithMany(p => p.payments)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_payments_doctor");
            entity.HasOne(d => d.patient).WithMany(p => p.payments)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_payments_user");
        });

        // ── Review ────────────────────────────────────────────
        modelBuilder.Entity<review>(entity =>
        {
            entity.HasIndex(e => new { e.doctor_id, e.is_visible }, "IX_reviews_doctor");
            entity.HasIndex(e => e.rating, "IX_reviews_rating");
            entity.HasIndex(e => e.booking_id, "UQ_reviews_booking").IsUnique();
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.is_visible).HasDefaultValue(true);
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.booking).WithOne(p => p.review)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_reviews_booking");
            entity.HasOne(d => d.doctor).WithMany(p => p.reviews)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_reviews_doctor");
            entity.HasOne(d => d.patient).WithMany(p => p.reviews)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_reviews_user");
        });

        // ── Review Response ───────────────────────────────────
        modelBuilder.Entity<review_response>(entity =>
        {
            entity.HasIndex(e => e.review_id, "UQ_review_responses").IsUnique();
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.doctor).WithMany(p => p.review_responses)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_rr_doctor");
            entity.HasOne(d => d.review).WithOne(p => p.review_response)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_rr_review");
        });

        // ── Search History ────────────────────────────────────
        modelBuilder.Entity<search_history>(entity =>
        {
            entity.HasIndex(e => new { e.user_id, e.created_at }, "IX_sh_user_recent")
                .IsDescending(false, true);
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.search_type).HasDefaultValue("general");
            entity.HasOne(d => d.patient).WithMany(p => p.search_histories)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_sh_user");
        });

        // ── Specialty ─────────────────────────────────────────
        modelBuilder.Entity<specialty>(entity =>
        {
            entity.HasIndex(e => e.name, "UQ_specialties_name").IsUnique();
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
        });

        // ── System Log ────────────────────────────────────────
        modelBuilder.Entity<system_log>(entity =>
        {
            entity.HasIndex(e => e.action, "IX_sl_action");
            entity.HasIndex(e => new { e.actor_type, e.actor_id }, "IX_sl_actor");
            entity.HasIndex(e => e.created_at, "IX_sl_created_at");
            entity.HasIndex(e => new { e.target_type, e.target_id }, "IX_sl_target");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
        });

        // ── Patient ───────────────────────────────────────────
        modelBuilder.Entity<patient>(entity =>
        {
            entity.HasIndex(e => new { e.is_active, e.is_banned }, "IX_users_status");
            entity.HasIndex(e => e.email, "UQ_users_email").IsUnique();
            entity.HasIndex(e => e.mobile_number, "UQ_users_mobile").IsUnique();
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.updated_at).HasDefaultValueSql("(sysutcdatetime())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
