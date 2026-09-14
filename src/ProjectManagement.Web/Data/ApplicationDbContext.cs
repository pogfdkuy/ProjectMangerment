using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<SystemCategory> SystemCategories => Set<SystemCategory>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectSubTask> ProjectSubTasks => Set<ProjectSubTask>();
    public DbSet<GeneralRequirement> GeneralRequirements => Set<GeneralRequirement>();
    public DbSet<GeneralRequirementProgressLog> GeneralRequirementProgressLogs => Set<GeneralRequirementProgressLog>();
    public DbSet<ScheduleChangeLog> ScheduleChangeLogs => Set<ScheduleChangeLog>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<ItemNote> ItemNotes => Set<ItemNote>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.HasOne(u => u.Department)
                .WithMany()
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Project>(b =>
        {
            b.Property(p => p.Name).IsRequired();
            b.HasOne(p => p.Department)
                .WithMany()
                .HasForeignKey(p => p.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(p => p.SystemCategory)
                .WithMany()
                .HasForeignKey(p => p.SystemCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(p => p.ResponsibleUser)
                .WithMany()
                .HasForeignKey(p => p.ResponsibleUserId)
                .OnDelete(DeleteBehavior.SetNull);
            // 這兩個跟 ResponsibleUser 一樣是指向 AspNetUsers 的可為 null 外鍵。SQL Server 對於同一張表
            // 有 3 個以上會連鎖動作（含 SET NULL）的外鍵指向同一個對照表會拒絕建立索引/條件約束
            // （"may cause cycles or multiple cascade paths"），所以這兩個改成 Restrict（NO ACTION），
            // 使用者被刪除時不會自動處理這兩個欄位（正常情況下也不會真的刪除使用者，是用 IsActive 停用）。
            b.HasOne(p => p.RequesterUser)
                .WithMany()
                .HasForeignKey(p => p.RequesterUserId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(p => p.DeveloperUser)
                .WithMany()
                .HasForeignKey(p => p.DeveloperUserId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasMany(p => p.SubTasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProjectSubTask>(b =>
        {
            b.Property(t => t.EstimatedHours).HasColumnType("decimal(10,2)");
        });

        builder.Entity<GeneralRequirement>(b =>
        {
            b.Property(r => r.Title).IsRequired();
            b.Property(r => r.EstimatedHours).HasColumnType("decimal(10,2)");
            b.HasOne(r => r.Department)
                .WithMany()
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(r => r.SystemCategory)
                .WithMany()
                .HasForeignKey(r => r.SystemCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(r => r.ResponsibleUser)
                .WithMany()
                .HasForeignKey(r => r.ResponsibleUserId)
                .OnDelete(DeleteBehavior.SetNull);
            // 原因同 Project：同一張表對 AspNetUsers 有 3 個以上會連鎖動作的外鍵，SQL Server 會拒絕，
            // 所以這兩個改成 Restrict（NO ACTION）。
            b.HasOne(r => r.RequesterUser)
                .WithMany()
                .HasForeignKey(r => r.RequesterUserId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(r => r.DeveloperUser)
                .WithMany()
                .HasForeignKey(r => r.DeveloperUserId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasMany(r => r.ProgressLogs)
                .WithOne(l => l.GeneralRequirement)
                .HasForeignKey(l => l.GeneralRequirementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GeneralRequirementProgressLog>(b =>
        {
            b.HasOne(l => l.ChangedByUser)
                .WithMany()
                .HasForeignKey(l => l.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ScheduleChangeLog>(b =>
        {
            b.HasIndex(l => new { l.OwnerType, l.OwnerId });
            b.HasOne(l => l.ChangedByUser)
                .WithMany()
                .HasForeignKey(l => l.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Attachment>(b =>
        {
            b.HasIndex(a => new { a.OwnerType, a.OwnerId });
            b.HasOne(a => a.UploadedByUser)
                .WithMany()
                .HasForeignKey(a => a.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ItemNote>(b =>
        {
            b.HasIndex(n => new { n.OwnerType, n.OwnerId });
            b.HasOne(n => n.CreatedByUser)
                .WithMany()
                .HasForeignKey(n => n.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Notification>(b =>
        {
            b.HasIndex(n => new { n.RecipientUserId, n.IsRead });
            b.HasOne(n => n.RecipientUser)
                .WithMany()
                .HasForeignKey(n => n.RecipientUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Department>().HasIndex(d => d.Name).IsUnique();
        builder.Entity<SystemCategory>().HasIndex(c => c.Name).IsUnique();
    }
}
