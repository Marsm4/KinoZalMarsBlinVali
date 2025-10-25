using System;
using System.Collections.Generic;
using KinoZalMarsBlinVali.Models;
using Microsoft.EntityFrameworkCore;

namespace KinoZalMarsBlinVali.Data;

public partial class CinemaDbContext : DbContext
{
    public CinemaDbContext()
    {
    }

    public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdditionalService> AdditionalServices { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<FinancialTransaction> FinancialTransactions { get; set; }

    public virtual DbSet<Hall> Halls { get; set; }

    public virtual DbSet<HallSeat> HallSeats { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketService> TicketServices { get; set; }

    public virtual DbSet<TicketType> TicketTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=cinema_db;Username=postgres;Password=123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdditionalService>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("additional_services_pkey");

            entity.ToTable("additional_services");

            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(500)
                .HasColumnName("image_path");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(255)
                .HasColumnName("service_name");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("customers_pkey");

            entity.ToTable("customers");

            entity.HasIndex(e => e.Email, "customers_email_key").IsUnique();

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.BonusPoints)
                .HasDefaultValue(0)
                .HasColumnName("bonus_points");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("employees_pkey");

            entity.ToTable("employees");

            entity.HasIndex(e => e.Username, "employees_username_key").IsUnique();

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Position)
                .HasMaxLength(100)
                .HasColumnName("position");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EquipmentId).HasName("equipment_pkey");

            entity.ToTable("equipment");

            entity.Property(e => e.EquipmentId).HasColumnName("equipment_id");
            entity.Property(e => e.EquipmentName)
                .HasMaxLength(255)
                .HasColumnName("equipment_name");
            entity.Property(e => e.HallId).HasColumnName("hall_id");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(500)
                .HasColumnName("image_path");
            entity.Property(e => e.LastMaintenanceDate).HasColumnName("last_maintenance_date");
            entity.Property(e => e.Model)
                .HasMaxLength(255)
                .HasColumnName("model");
            entity.Property(e => e.NextMaintenanceDate).HasColumnName("next_maintenance_date");
            entity.Property(e => e.PurchaseDate).HasColumnName("purchase_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Hall).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.HallId)
                .HasConstraintName("equipment_hall_id_fkey");
        });

        modelBuilder.Entity<FinancialTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("financial_transactions_pkey");

            entity.ToTable("financial_transactions");

            entity.HasIndex(e => e.TransactionTime, "idx_financial_transactions_time");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.TransactionTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("transaction_time");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(50)
                .HasColumnName("transaction_type");

            entity.HasOne(d => d.Employee).WithMany(p => p.FinancialTransactions)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("financial_transactions_employee_id_fkey");

            entity.HasOne(d => d.Ticket).WithMany(p => p.FinancialTransactions)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("financial_transactions_ticket_id_fkey");
        });

        modelBuilder.Entity<Hall>(entity =>
        {
            entity.HasKey(e => e.HallId).HasName("halls_pkey");

            entity.ToTable("halls");

            entity.Property(e => e.HallId).HasColumnName("hall_id");
            entity.Property(e => e.HallName)
                .HasMaxLength(100)
                .HasColumnName("hall_name");
            entity.Property(e => e.HallType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'standard'::character varying")
                .HasColumnName("hall_type");
            entity.Property(e => e.LayoutSchemaPath)
                .HasMaxLength(500)
                .HasColumnName("layout_schema_path");
            entity.Property(e => e.SeatColumns).HasColumnName("seat_columns");
            entity.Property(e => e.SeatRows).HasColumnName("seat_rows");
            entity.Property(e => e.TotalSeats).HasColumnName("total_seats");
        });

        modelBuilder.Entity<HallSeat>(entity =>
        {
            entity.HasKey(e => e.SeatId).HasName("hall_seats_pkey");

            entity.ToTable("hall_seats");

            entity.HasIndex(e => new { e.HallId, e.RowNumber, e.SeatNumber }, "hall_seats_hall_id_row_number_seat_number_key").IsUnique();

            entity.Property(e => e.SeatId).HasColumnName("seat_id");
            entity.Property(e => e.HallId).HasColumnName("hall_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PriceMultiplier)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("1.00")
                .HasColumnName("price_multiplier");
            entity.Property(e => e.RowNumber).HasColumnName("row_number");
            entity.Property(e => e.SeatNumber).HasColumnName("seat_number");
            entity.Property(e => e.SeatType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'standard'::character varying")
                .HasColumnName("seat_type");

            entity.HasOne(d => d.Hall).WithMany(p => p.HallSeats)
                .HasForeignKey(d => d.HallId)
                .HasConstraintName("hall_seats_hall_id_fkey");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("images_pkey");

            entity.ToTable("images");

            entity.HasIndex(e => new { e.EntityType, e.EntityId }, "idx_images_entity");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(50)
                .HasColumnName("entity_type");
            entity.Property(e => e.FilePath)
                .HasMaxLength(500)
                .HasColumnName("file_path");
            entity.Property(e => e.ImageData).HasColumnName("image_data");
            entity.Property(e => e.ImageName)
                .HasMaxLength(255)
                .HasColumnName("image_name");
            entity.Property(e => e.ImageType)
                .HasMaxLength(50)
                .HasColumnName("image_type");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploaded_at");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.MovieId).HasName("movies_pkey");

            entity.ToTable("movies");

            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.AgeRating)
                .HasMaxLength(20)
                .HasColumnName("age_rating");
            entity.Property(e => e.CastText).HasColumnName("cast_text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Director)
                .HasMaxLength(255)
                .HasColumnName("director");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.Genre)
                .HasMaxLength(100)
                .HasColumnName("genre");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PosterPath)
                .HasMaxLength(500)
                .HasColumnName("poster_path");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("sessions_pkey");

            entity.ToTable("sessions");

            entity.HasIndex(e => e.HallId, "idx_sessions_hall_id");

            entity.HasIndex(e => e.MovieId, "idx_sessions_movie_id");

            entity.HasIndex(e => e.StartTime, "idx_sessions_start_time");

            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.BasePrice)
                .HasPrecision(10, 2)
                .HasColumnName("base_price");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.EndTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_time");
            entity.Property(e => e.HallId).HasColumnName("hall_id");
            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.StartTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_time");

            entity.HasOne(d => d.Hall).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.HallId)
                .HasConstraintName("sessions_hall_id_fkey");

            entity.HasOne(d => d.Movie).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.MovieId)
                .HasConstraintName("sessions_movie_id_fkey");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("tickets_pkey");

            entity.ToTable("tickets");

            entity.HasIndex(e => e.CustomerId, "idx_tickets_customer_id");

            entity.HasIndex(e => e.SeatId, "idx_tickets_seat_id");

            entity.HasIndex(e => e.SessionId, "idx_tickets_session_id");

            entity.HasIndex(e => e.Status, "idx_tickets_status");

            entity.HasIndex(e => new { e.SessionId, e.SeatId }, "tickets_session_id_seat_id_key").IsUnique();

            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.FinalPrice)
                .HasPrecision(10, 2)
                .HasColumnName("final_price");
            entity.Property(e => e.PurchaseTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("purchase_time");
            entity.Property(e => e.QrCodeData).HasColumnName("qr_code_data");
            entity.Property(e => e.ReservationExpires)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reservation_expires");
            entity.Property(e => e.ReservationTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reservation_time");
            entity.Property(e => e.SeatId).HasColumnName("seat_id");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'sold'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TicketTypeId).HasColumnName("ticket_type_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_customer_id_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_employee_id_fkey");

            entity.HasOne(d => d.Seat).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.SeatId)
                .HasConstraintName("tickets_seat_id_fkey");

            entity.HasOne(d => d.Session).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("tickets_session_id_fkey");

            entity.HasOne(d => d.TicketType).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TicketTypeId)
                .HasConstraintName("tickets_ticket_type_id_fkey");
        });

        modelBuilder.Entity<TicketService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ticket_services_pkey");

            entity.ToTable("ticket_services");

            entity.HasIndex(e => new { e.TicketId, e.ServiceId }, "ticket_services_ticket_id_service_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.Service).WithMany(p => p.TicketServices)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("ticket_services_service_id_fkey");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketServices)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("ticket_services_ticket_id_fkey");
        });

        modelBuilder.Entity<TicketType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("ticket_types_pkey");

            entity.ToTable("ticket_types");

            entity.HasIndex(e => e.TypeName, "ticket_types_type_name_key").IsUnique();

            entity.Property(e => e.TypeId).HasColumnName("type_id");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("discount_percent");
            entity.Property(e => e.TypeName)
                .HasMaxLength(100)
                .HasColumnName("type_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
