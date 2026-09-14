using Microsoft.EntityFrameworkCore;
using ProjectManagement.Web.Data;
using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Services;

/// <summary>
/// 附件存放在設定檔 Storage:UploadPath 指定的目錄（預設 App_Data\Uploads，位在 wwwroot 之外），
/// 每個掛載對象各自一個子目錄，磁碟檔名用 GUID 避免衝突與路徑穿越攻擊。
/// 下載一律經過 /attachments/{id}/download 端點驗證登入與存取權後才串流回傳，不會被直接用網址存取。
///
/// 改用 IDbContextFactory 而不是直接注入 ApplicationDbContext：每次操作都建立一個獨立的短生命週期
/// DbContext，避免跟畫面上其他同時執行的元件（例如版面配置裡的通知鈴鐺）共用同一個 Scoped DbContext
/// 實例而互相干擾，撞出 "A second operation was started on this context instance..." 的例外。
/// </summary>
public class AttachmentService(IDbContextFactory<ApplicationDbContext> dbFactory, IConfiguration configuration, IWebHostEnvironment env) : IAttachmentService
{
    private string RootPath
    {
        get
        {
            var configuredPath = configuration["Storage:UploadPath"] ?? "App_Data/Uploads";
            var fullPath = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(env.ContentRootPath, configuredPath);
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }
    }

    public async Task<Attachment> SaveAsync(OwnerEntityType ownerType, int ownerId, string fileName, string contentType, Stream content, string uploadedByUserId)
    {
        var subFolder = Path.Combine(RootPath, ownerType.ToString(), ownerId.ToString());
        Directory.CreateDirectory(subFolder);

        var storedFileName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var fullPath = Path.Combine(subFolder, storedFileName);

        await using (var fileStream = File.Create(fullPath))
        {
            await content.CopyToAsync(fileStream);
        }

        var attachment = new Attachment
        {
            OwnerType = ownerType,
            OwnerId = ownerId,
            FileName = fileName,
            StoredFileName = Path.Combine(ownerType.ToString(), ownerId.ToString(), storedFileName),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            FileSizeBytes = new FileInfo(fullPath).Length,
            UploadedByUserId = uploadedByUserId
        };

        await using var db = await dbFactory.CreateDbContextAsync();
        db.Attachments.Add(attachment);
        await db.SaveChangesAsync();
        return attachment;
    }

    public async Task<List<Attachment>> GetForOwnerAsync(OwnerEntityType ownerType, int ownerId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.Attachments
            .Where(a => a.OwnerType == ownerType && a.OwnerId == ownerId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync();
    }

    public async Task<Attachment?> GetByIdAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Attachments.FirstOrDefaultAsync(a => a.Id == id);
    }

    public string GetPhysicalPath(Attachment attachment) => Path.Combine(RootPath, attachment.StoredFileName);

    public async Task DeleteAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var attachment = await db.Attachments.FindAsync(id);
        if (attachment is null) return;

        var path = GetPhysicalPath(attachment);
        if (File.Exists(path)) File.Delete(path);

        db.Attachments.Remove(attachment);
        await db.SaveChangesAsync();
    }
}
