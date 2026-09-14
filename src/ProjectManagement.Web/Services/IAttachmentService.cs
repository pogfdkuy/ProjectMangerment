using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Services;

public interface IAttachmentService
{
    Task<Attachment> SaveAsync(OwnerEntityType ownerType, int ownerId, string fileName, string contentType, Stream content, string uploadedByUserId);

    Task<List<Attachment>> GetForOwnerAsync(OwnerEntityType ownerType, int ownerId);

    Task<Attachment?> GetByIdAsync(int id);

    /// <summary>取得附件實體檔案的完整路徑，供下載端點串流讀取。</summary>
    string GetPhysicalPath(Attachment attachment);

    Task DeleteAsync(int id);
}
