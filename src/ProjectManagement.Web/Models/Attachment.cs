using System.ComponentModel.DataAnnotations;
using ProjectManagement.Web.Data;

namespace ProjectManagement.Web.Models;

/// <summary>
/// 附件（掛在專案或一般需求底下）。實體檔案存放在伺服器上設定的上傳目錄（非 wwwroot，避免未經授權直接下載），
/// 下載一律透過 /attachments/{id}/download 這個受權限保護的端點。
/// </summary>
public class Attachment
{
    public int Id { get; set; }

    public OwnerEntityType OwnerType { get; set; }
    public int OwnerId { get; set; }

    [Required, StringLength(260)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>實際存放在磁碟上的檔名（避免重複/特殊字元，通常是 GUID + 副檔名）。</summary>
    [Required, StringLength(260)]
    public string StoredFileName { get; set; } = string.Empty;

    [StringLength(200)]
    public string ContentType { get; set; } = "application/octet-stream";

    public long FileSizeBytes { get; set; }

    public string UploadedByUserId { get; set; } = string.Empty;
    public ApplicationUser? UploadedByUser { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
