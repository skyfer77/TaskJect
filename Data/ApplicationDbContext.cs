using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Domain.Database;
using Data.DomainEvent;
using Domain.DomainEvents;

namespace Data.DbContexts;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly DomainEventDispatcher _dispatcher;
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, DomainEventDispatcher dispatcher)
        : base(options)
    {
        _dispatcher = dispatcher;
    }

    public virtual DbSet<Membership> Memberships { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Domain.Database.Task> Tasks { get; set; }

    public virtual DbSet<TariffPlan> TariffPlans { get; set; }
    public virtual DbSet<TariffPlanHistory> TariffPlansHistories { get; set; }

    public virtual DbSet<OrganizationAppeal> OrganizationAppeals { get; set; }

    public virtual DbSet<ProjectUserPermission> ProjectUserPermission { get; set; }

    public virtual DbSet<OrganizationFiles> OrganizationFiles { get; set; }

    public virtual DbSet<PersonalTodo> PersonalTodos { get; set; }

    public virtual DbSet<PersonalTodoTask> PersonalTodoTasks { get; set; }

    public virtual DbSet<PersonalNote> PersonalNotes { get; set; }
    public virtual DbSet<GumroadWebhookLog> GumroadWebhookLog { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<PaymentWayForPay> PaymentWayForPays { get; set; }
    public virtual DbSet<PaymentInvoice> PaymentInvoices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
        // optionsBuilder.UseSqlServer("Name=DefaultConnection");
	}
        

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TasksCountByUser>().HasNoKey();
        modelBuilder.Entity<TaskProgressDto>().HasNoKey();
        modelBuilder.Entity<TasksStatisticByPeriod>().HasNoKey();

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.ToTable("Membership");
            entity.Property(e => e.MembershipId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("Organization");
            entity.Property(e => e.OrganizationId).ValueGeneratedNever();
            entity.Property(e => e.LockoutEnabled)
                .IsRequired()
                .HasDefaultValueSql("(CONVERT([bit],(0)))");
            entity.Property(e => e.RegistrationDate).HasDefaultValueSql("('0001-01-01T00:00:00.0000000')");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("Team");
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Project");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            entity.Property(e => e.EndDate).HasColumnType("date");
            entity.Property(e => e.File).HasColumnName("FIle");
            entity.Property(e => e.ManagerId).HasColumnName("ManagerID");
            entity.Property(e => e.OrganizationCode).HasDefaultValueSql("(N'')");
            entity.Property(e => e.ShortDescription).HasMaxLength(200);
            entity.Property(e => e.StartDate).HasColumnType("date");
            entity.Property(e => e.Title).HasMaxLength(28);
        });

        modelBuilder.Entity<Domain.Database.Task>(entity =>
        {
            entity.ToTable("Task");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.AssigneeId).HasColumnName("AssigneeID").HasMaxLength(450);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.IsAgreedOverdue)
                .IsRequired()
                .HasDefaultValueSql("(CONVERT([bit],(0)))");
            entity.Property(e => e.OrganizationCode).HasDefaultValueSql("(N'')");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(128);
        });

        modelBuilder.Entity<TariffPlan>(entity =>
        {
            entity.ToTable("TariffPlan");

            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code)
                .HasColumnName("Code")
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PriceMonth).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PriceMonthlyDiscount).HasMaxLength(50);
            entity.Property(e => e.PriceYearlyDiscount).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MaxUsers).IsRequired();
            entity.Property(e => e.MaxStorageBytes).IsRequired();
        });

        modelBuilder.Entity<OrganizationAppeal>(entity =>
        {
            entity.ToTable("OrganizationAppeal");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.OrganizationCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Title);
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");

            entity.Property(e => e.Date);

            entity.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(e => e.OrganizationCode)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_OrganizationAppeal_Organization");
        });
        modelBuilder.Entity<TariffPlanHistory>(entity =>
        {
            entity.ToTable("TariffPlanHistory");

            entity.Property(e => e.OrganizationCode)
                .IsRequired();

            entity.Property(e => e.TariffPlanCode)
            .IsRequired()
            .HasMaxLength(50);

            entity.Property(e => e.DateFrom)
                .IsRequired();

            entity.HasKey(e => new { e.OrganizationCode, e.TariffPlanCode, e.DateFrom });

            entity.HasOne<Organization>()
                  .WithMany()
                  .HasForeignKey(e => e.OrganizationCode)
                  .HasPrincipalKey(o => o.OrganizationId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<TariffPlan>()
                  .WithMany()
                  .HasForeignKey(e => e.TariffPlanCode)
                  .HasPrincipalKey(tp => tp.Code)
                  .OnDelete(DeleteBehavior.Restrict);

        });
        modelBuilder.Entity<ProjectUserPermission>(entity =>
        {
            entity.ToTable("ProjectUserPermission");

            entity.Property(e => e.ProjectId)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.HasKey(e => new { e.ProjectId, e.UserId });

            entity.HasOne<Project>()
                  .WithMany()
                  .HasForeignKey(e => e.ProjectId)
                  .HasPrincipalKey(o => o.Id)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<ApplicationUser>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .HasPrincipalKey(tp => tp.Id)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<OrganizationFiles>(entity =>
        {
            entity.ToTable("OrganizationFiles");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.HasOne<Project>()
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Domain.Database.Task>()
                .WithMany()
                .HasForeignKey(e => e.TaskId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Organization>()
                  .WithMany()
                  .HasForeignKey(e => e.OrganizationCode)
                  .HasPrincipalKey(o => o.OrganizationId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<PersonalTodo>(entity =>
        {
            entity.ToTable("PersonalTodo");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Title)
                .HasMaxLength(200);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<PersonalTodoTask>(entity =>
        {
            entity.ToTable("PersonalTodoTask");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Text)
                .HasMaxLength(500);

            entity.HasOne(e => e.Todo)
                .WithMany(t => t.Tasks)
                .HasForeignKey(e => e.TodoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<PersonalNote>(entity =>
        {
            entity.ToTable("PersonalNote");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Title)
                .HasMaxLength(256);

            entity.Property(e => e.Text)
                .HasColumnType("nvarchar(max)");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GumroadWebhookLog>(entity =>
        {
            entity.ToTable("GumroadWebhookLog");

            entity.HasKey(x => x.EventId);
            entity.Property(x => x.EventId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SaleId).HasMaxLength(100);
            entity.Property(x => x.SubscriptionId).HasMaxLength(100);

            entity.HasIndex(x => x.SaleId);
            entity.HasIndex(x => x.SubscriptionId);
            entity.HasIndex(x => x.EventType);
        });

		modelBuilder.Entity<Notification>().ToTable("Notification");

        modelBuilder.Entity<PaymentWayForPay>(entity =>
        {
            entity.ToTable("PaymentWayForPay");

            entity.HasKey(x => x.Id);
			entity.Property(e => e.Id)
				.ValueGeneratedOnAdd();

            entity.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");
		});

		modelBuilder.Entity<PaymentInvoice>(entity =>
		{
			entity.ToTable("PaymentInvoice");

			entity.HasKey(x => x.Id);
			entity.Property(e => e.Id)
				.ValueGeneratedOnAdd();

			entity.Property(p => p.Amount)
				.HasColumnType("decimal(18,2)");

			entity.HasOne(i => i.PaymentWayForPay)
		        .WithMany()
		        .HasForeignKey(i => i.PaymentWayForPayId)
		        .OnDelete(DeleteBehavior.Restrict);
		});

		base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
		var domainEvents = ChangeTracker
			.Entries<BaseEntity>()
			.SelectMany(x => x.Entity.DomainEvents)
		    .ToList();

		var result = await base.SaveChangesAsync(cancellationToken);

		foreach (var entityEntry in ChangeTracker.Entries<BaseEntity>())
		{
			entityEntry.Entity.ClearDomainEvents();
		}

		try
		{
			if (domainEvents.Count > 0)
			{
				await _dispatcher.DispatchAsync(domainEvents);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error while dispatching domain events: {ex.Message}");
		}

		return result;
	}
}
