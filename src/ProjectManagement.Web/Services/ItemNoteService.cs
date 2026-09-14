using Microsoft.EntityFrameworkCore;
using ProjectManagement.Web.Data;
using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Services;

/// <summary>
/// 改用 IDbContextFactory 而不是直接注入 ApplicationDbContext：每次操作都建立一個獨立的短生命週期
/// DbContext，避免跟畫面上其他同時執行的元件（例如版面配置裡的通知鈴鐺）共用同一個 Scoped DbContext
/// 實例而互相干擾，撞出 "A second operation was started on this context instance..." 的例外。
/// </summary>
public class ItemNoteService(IDbContextFactory<ApplicationDbContext> dbFactory) : IItemNoteService
{
    public async Task<List<ItemNote>> GetForOwnerAsync(OwnerEntityType ownerType, int ownerId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.ItemNotes
            .Include(n => n.CreatedByUser)
            .Where(n => n.OwnerType == ownerType && n.OwnerId == ownerId)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<ItemNote> AddAsync(OwnerEntityType ownerType, int ownerId, string content, string createdByUserId)
    {
        var note = new ItemNote
        {
            OwnerType = ownerType,
            OwnerId = ownerId,
            Content = content,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        await using var db = await dbFactory.CreateDbContextAsync();
        db.ItemNotes.Add(note);
        await db.SaveChangesAsync();
        return note;
    }
}
