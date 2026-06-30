using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CleanArchitectureRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "admin"),
                    two_factor_enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    two_factor_secret = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    last_login_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_login_ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admins", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_preferences",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    notifiable_type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    notifiable_id = table.Column<long>(type: "bigint", nullable: false),
                    booking_confirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    booking_cancelled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    booking_rescheduled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    new_message = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    new_review = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    payment_received = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    upcoming_appointment_reminder = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    reminder_hours_before = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)24),
                    promotional = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_preferences", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    notifiable_type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    notifiable_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    sent_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delivery_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "specialties",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    icon_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    icon_type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    sort_order = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_specialties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    actor_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    actor_id = table.Column<long>(type: "bigint", nullable: true),
                    action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    target_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    target_id = table.Column<long>(type: "bigint", nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    mobile_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    birthdate = table.Column<DateOnly>(type: "date", nullable: true),
                    profile_photo_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    location_label = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    is_banned = table.Column<bool>(type: "bit", nullable: false),
                    ban_reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    mobile_verified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    email_verified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    remember_me = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    refresh_token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    refresh_token_expiry = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "announcements",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    admin_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    target_audience = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "all"),
                    is_push_sent = table.Column<bool>(type: "bit", nullable: false),
                    push_sent_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    publish_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_announcements", x => x.id);
                    table.ForeignKey(
                        name: "FK_ann_admin",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "clinics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Egypt"),
                    latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    photo_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    rating_avg = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    working_hours_start = table.Column<TimeOnly>(type: "time", nullable: true),
                    working_hours_end = table.Column<TimeOnly>(type: "time", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinics", x => x.id);
                    table.ForeignKey(
                        name: "FK_clinics_admin",
                        column: x => x.created_by,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "faqs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    sort_order = table.Column<short>(type: "smallint", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faqs", x => x.id);
                    table.ForeignKey(
                        name: "FK_faqs_creator",
                        column: x => x.created_by,
                        principalTable: "admins",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_faqs_updater",
                        column: x => x.updated_by,
                        principalTable: "admins",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "search_histories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    query = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    search_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "general"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_histories", x => x.id);
                    table.ForeignKey(
                        name: "FK_sh_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "doctors",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    admin_id = table.Column<long>(type: "bigint", nullable: false),
                    specialty_id = table.Column<long>(type: "bigint", nullable: false),
                    clinic_id = table.Column<long>(type: "bigint", nullable: true),
                    full_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    mobile_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    temporary_password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    must_change_password = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    license_number = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    license_verified = table.Column<bool>(type: "bit", nullable: false),
                    bio = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    profile_photo_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    years_of_experience = table.Column<byte>(type: "tinyint", nullable: true),
                    session_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "USD"),
                    clinic_latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    clinic_longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    rating_avg = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    rating_count = table.Column<int>(type: "int", nullable: false),
                    total_patients = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_verified = table.Column<bool>(type: "bit", nullable: false),
                    is_banned = table.Column<bool>(type: "bit", nullable: false),
                    activation_token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    activated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctors", x => x.id);
                    table.ForeignKey(
                        name: "FK_doctors_admin",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_doctors_clinic",
                        column: x => x.clinic_id,
                        principalTable: "clinics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_doctors_spec",
                        column: x => x.specialty_id,
                        principalTable: "specialties",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "favorites",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    doctor_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favorites", x => x.id);
                    table.ForeignKey(
                        name: "FK_favorites_doctor",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_favorites_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_reference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    doctor_id = table.Column<long>(type: "bigint", nullable: false),
                    availability_slot_id = table.Column<long>(type: "bigint", nullable: false),
                    clinic_id = table.Column<long>(type: "bigint", nullable: true),
                    specialty_id = table.Column<long>(type: "bigint", nullable: true),
                    booked_date = table.Column<DateOnly>(type: "date", nullable: false),
                    booked_start_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    booked_end_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    cancellation_reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    cancelled_by = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    payment_method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    session_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "USD"),
                    is_rebooking = table.Column<bool>(type: "bit", nullable: false),
                    original_booking_id = table.Column<long>(type: "bigint", nullable: true),
                    notes_for_doctor = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.id);
                    table.ForeignKey(
                        name: "FK_bookings_clinic",
                        column: x => x.clinic_id,
                        principalTable: "clinics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_bookings_doctor",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_bookings_original",
                        column: x => x.original_booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_bookings_specialty",
                        column: x => x.specialty_id,
                        principalTable: "specialties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_bookings_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "doctor_availability_slots",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    doctor_id = table.Column<long>(type: "bigint", nullable: false),
                    slot_date = table.Column<DateOnly>(type: "date", nullable: false),
                    slot_start_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    slot_end_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    duration_minutes = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)60),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "available"),
                    booking_id = table.Column<long>(type: "bigint", nullable: true),
                    recurrence_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "none"),
                    recurrence_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_das", x => x.id);
                    table.ForeignKey(
                        name: "FK_das_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_das_doctor",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    doctor_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    gateway = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    gateway_transaction_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    gateway_response = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "USD"),
                    platform_fee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    doctor_earnings = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "pending"),
                    refunded_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    refund_reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    refunded_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_payments_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_payments_doctor",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_payments_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    doctor_id = table.Column<long>(type: "bigint", nullable: false),
                    rating = table.Column<byte>(type: "tinyint", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    is_visible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_flagged = table.Column<bool>(type: "bit", nullable: false),
                    flag_reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    doctor_notified = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_reviews_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_reviews_doctor",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_reviews_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "review_responses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    review_id = table.Column<long>(type: "bigint", nullable: false),
                    doctor_id = table.Column<long>(type: "bigint", nullable: false),
                    response_text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_responses", x => x.id);
                    table.ForeignKey(
                        name: "FK_rr_doctor",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_rr_review",
                        column: x => x.review_id,
                        principalTable: "reviews",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "chat_conversations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    doctor_id = table.Column<long>(type: "bigint", nullable: false),
                    is_favorited_by_user = table.Column<bool>(type: "bit", nullable: false),
                    is_favorited_by_doctor = table.Column<bool>(type: "bit", nullable: false),
                    is_archived_by_user = table.Column<bool>(type: "bit", nullable: false),
                    is_archived_by_doctor = table.Column<bool>(type: "bit", nullable: false),
                    is_muted_by_user = table.Column<bool>(type: "bit", nullable: false),
                    is_muted_by_doctor = table.Column<bool>(type: "bit", nullable: false),
                    user_unread_count = table.Column<short>(type: "smallint", nullable: false),
                    doctor_unread_count = table.Column<short>(type: "smallint", nullable: false),
                    last_message_id = table.Column<long>(type: "bigint", nullable: true),
                    last_message_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_conversations", x => x.id);
                    table.ForeignKey(
                        name: "FK_cc_doctor",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_cc_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    conversation_id = table.Column<long>(type: "bigint", nullable: false),
                    sender_type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    sender_id = table.Column<long>(type: "bigint", nullable: false),
                    receiver_type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    receiver_id = table.Column<long>(type: "bigint", nullable: false),
                    message_type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "text"),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    duration_seconds = table.Column<short>(type: "smallint", nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_deleted_by_sender = table.Column<bool>(type: "bit", nullable: false),
                    is_deleted_by_receiver = table.Column<bool>(type: "bit", nullable: false),
                    reply_to_message_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_cm_conversation",
                        column: x => x.conversation_id,
                        principalTable: "chat_conversations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_cm_reply_to",
                        column: x => x.reply_to_message_id,
                        principalTable: "chat_messages",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "UQ_admins_email",
                table: "admins",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_announcements_admin_id",
                table: "announcements",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_clinic_id",
                table: "bookings",
                column: "clinic_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_date",
                table: "bookings",
                column: "booked_date");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_doctor",
                table: "bookings",
                columns: new[] { "doctor_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_original_booking_id",
                table: "bookings",
                column: "original_booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_slot",
                table: "bookings",
                column: "availability_slot_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_specialty_id",
                table: "bookings",
                column: "specialty_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_user",
                table: "bookings",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "UQ_bookings_reference",
                table: "bookings",
                column: "booking_reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cc_doctor_inbox",
                table: "chat_conversations",
                columns: new[] { "doctor_id", "last_message_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_cc_user_inbox",
                table: "chat_conversations",
                columns: new[] { "user_id", "last_message_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_chat_conversations_last_message_id",
                table: "chat_conversations",
                column: "last_message_id");

            migrationBuilder.CreateIndex(
                name: "UQ_chat_conversations",
                table: "chat_conversations",
                columns: new[] { "user_id", "doctor_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_reply_to_message_id",
                table: "chat_messages",
                column: "reply_to_message_id");

            migrationBuilder.CreateIndex(
                name: "IX_cm_conversation",
                table: "chat_messages",
                columns: new[] { "conversation_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_cm_sender",
                table: "chat_messages",
                columns: new[] { "sender_type", "sender_id" });

            migrationBuilder.CreateIndex(
                name: "IX_cm_unread",
                table: "chat_messages",
                columns: new[] { "is_read", "receiver_id" });

            migrationBuilder.CreateIndex(
                name: "IX_clinics_city",
                table: "clinics",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "IX_clinics_created_by",
                table: "clinics",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_clinics_location",
                table: "clinics",
                columns: new[] { "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_das_query",
                table: "doctor_availability_slots",
                columns: new[] { "doctor_id", "slot_date", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_doctor_availability_slots_booking_id",
                table: "doctor_availability_slots",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "UQ_das_slot",
                table: "doctor_availability_slots",
                columns: new[] { "doctor_id", "slot_date", "slot_start_time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_doctors_admin_id",
                table: "doctors",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctors_clinic_id",
                table: "doctors",
                column: "clinic_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctors_rating",
                table: "doctors",
                column: "rating_avg",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_doctors_specialty",
                table: "doctors",
                column: "specialty_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctors_status",
                table: "doctors",
                columns: new[] { "is_active", "is_verified" });

            migrationBuilder.CreateIndex(
                name: "UQ_doctors_email",
                table: "doctors",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_doctors_license",
                table: "doctors",
                column: "license_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_doctors_mobile",
                table: "doctors",
                column: "mobile_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_faqs_created_by",
                table: "faqs",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_faqs_updated_by",
                table: "faqs",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_doctor_id",
                table: "favorites",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_user",
                table: "favorites",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_favorites_pair",
                table: "favorites",
                columns: new[] { "user_id", "doctor_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_np_actor",
                table: "notification_preferences",
                columns: new[] { "notifiable_type", "notifiable_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notif_recipient",
                table: "notifications",
                columns: new[] { "notifiable_type", "notifiable_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "IX_notif_type",
                table: "notifications",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_payments_doctor",
                table: "payments",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_user",
                table: "payments",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "UQ_payments_booking",
                table: "payments",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_payments_gateway_txn",
                table: "payments",
                column: "gateway_transaction_id",
                unique: true,
                filter: "[gateway_transaction_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_review_responses_doctor_id",
                table: "review_responses",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "UQ_review_responses",
                table: "review_responses",
                column: "review_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reviews_doctor",
                table: "reviews",
                columns: new[] { "doctor_id", "is_visible" });

            migrationBuilder.CreateIndex(
                name: "IX_reviews_rating",
                table: "reviews",
                column: "rating");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_user_id",
                table: "reviews",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_reviews_booking",
                table: "reviews",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sh_user_recent",
                table: "search_histories",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "UQ_specialties_name",
                table: "specialties",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sl_action",
                table: "system_logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "IX_sl_actor",
                table: "system_logs",
                columns: new[] { "actor_type", "actor_id" });

            migrationBuilder.CreateIndex(
                name: "IX_sl_created_at",
                table: "system_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_sl_target",
                table: "system_logs",
                columns: new[] { "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "IX_users_status",
                table: "users",
                columns: new[] { "is_active", "is_banned" });

            migrationBuilder.CreateIndex(
                name: "UQ_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_users_mobile",
                table: "users",
                column: "mobile_number",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_slot",
                table: "bookings",
                column: "availability_slot_id",
                principalTable: "doctor_availability_slots",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_cc_last_message",
                table: "chat_conversations",
                column: "last_message_id",
                principalTable: "chat_messages",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_clinics_admin",
                table: "clinics");

            migrationBuilder.DropForeignKey(
                name: "FK_doctors_admin",
                table: "doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_bookings_clinic",
                table: "bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_doctors_clinic",
                table: "doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_bookings_doctor",
                table: "bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_cc_doctor",
                table: "chat_conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_das_doctor",
                table: "doctor_availability_slots");

            migrationBuilder.DropForeignKey(
                name: "FK_bookings_slot",
                table: "bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_cc_user",
                table: "chat_conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_cc_last_message",
                table: "chat_conversations");

            migrationBuilder.DropTable(
                name: "announcements");

            migrationBuilder.DropTable(
                name: "faqs");

            migrationBuilder.DropTable(
                name: "favorites");

            migrationBuilder.DropTable(
                name: "notification_preferences");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "review_responses");

            migrationBuilder.DropTable(
                name: "search_histories");

            migrationBuilder.DropTable(
                name: "system_logs");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "clinics");

            migrationBuilder.DropTable(
                name: "doctors");

            migrationBuilder.DropTable(
                name: "doctor_availability_slots");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "specialties");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "chat_conversations");
        }
    }
}
