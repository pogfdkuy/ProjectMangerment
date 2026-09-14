using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Services;

public interface IItemNoteService
{
    Task<List<ItemNote>> GetForOwnerAsync(OwnerEntityType ownerType, int ownerId);

    Task<ItemNote> AddAsync(OwnerEntityType ownerType, int ownerId, string content, string createdByUserId);
}
