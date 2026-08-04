using DictionaryAPI.Data;
using DictionaryAPI.Interfaces;
using DictionaryAPI.Models.DTOs.List;
using DictionaryAPI.Models.DTOs.Term;
using DictionaryAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static DictionaryAPI.Models.DTOs.List.FeaturedListResponseDto;

namespace DictionaryAPI.Services
{
    public class ListService : IListService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public ListService(ApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        #region "PUBLIC"
        public async Task<FeaturedListCategoriesResponseDto> GetFeaturedListsAsync()
        {
            var today = DateTime.UtcNow;

            var activeFeatured = await _context.FeaturedLists
                .AsNoTracking()
                .Where(f => f.StartDate <= today
                        && f.EndDate >= today
                        && f.List.IsPublic)
                .OrderBy(f => f.List.Name)
                .Select(f => new
                {
                    f.Category,
                    ListDto = new ListSummaryResponseDto(
                    f.List.Id,
                    f.List.Name,
                    f.List.TermLists.Count,
                    f.List.Description,
                    f.List.CreationDate
                )
                })
                .ToListAsync();

            var starter = activeFeatured.FirstOrDefault(x => x.Category == FeaturedListCategory.Starter)?.ListDto;
            var featured = activeFeatured.FirstOrDefault(x => x.Category == FeaturedListCategory.WeeklyHistory)?.ListDto;
            var selection = activeFeatured.FirstOrDefault(x => x.Category == FeaturedListCategory.Selection)?.ListDto;
            var special = activeFeatured.FirstOrDefault(x => x.Category == FeaturedListCategory.Special)?.ListDto;

            return new FeaturedListCategoriesResponseDto(starter, featured, selection, special);
        }

        public async Task<IEnumerable<ListSummaryResponseDto>> GetPublicListsAsync()
        {
            var adminRoleId = await _context.Roles
                .Where(r => r.Name == "Admin")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (adminRoleId == null) return Enumerable.Empty<ListSummaryResponseDto>();

            var adminUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync();

            return await _context.Lists
                .AsNoTracking()
                .Where(l => adminUserIds.Contains(l.UserId) && l.IsPublic)
                .OrderBy(l => l.Name)
                .Select(l => new ListSummaryResponseDto(
                    l.Id,
                    l.Name,
                    l.TermLists.Count,
                    l.Description,
                    l.CreationDate
                    ))
                .ToListAsync();
        }

        #endregion

        #region "SHARED"
        public async Task<ListDetailResponseDto?> GetListByIdAsync(int listId)
        {
            var userId = _currentUser.UserId;

            var list = await _context.Lists
                .AsNoTracking()
                .Include(l => l.TermLists).ThenInclude(tl => tl.Word)
                .Where(l => l.Id == listId && (l.IsPublic || l.UserId == userId))
                .FirstOrDefaultAsync();

            if (list is null) return null;

            return new ListDetailResponseDto(
                list.Id,
                list.Name,
                list.TermLists.Count,
                list.Description,
                list.CreationDate,
                list.TermLists.Select(tl => new TermSummaryPublicResponseDto(
                    tl.Word.Id,
                    tl.Word.Word,
                    tl.Word.Definition,
                    tl.Word.Etymology,
                    tl.Word.Category
                ))
            );
        }
        #endregion

        #region "NEEDS AUTH"
        public async Task<IEnumerable<ListSummaryResponseDto>> GetUserListsAsync()
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("Failed to authenticate user.");

            return await _context.Lists
                .AsNoTracking()
                .Where(l => l.UserId == userId)
                .OrderBy(l => l.Name)
                .Select(l => new ListSummaryResponseDto(
                    l.Id,
                    l.Name,
                    l.TermLists.Count,
                    l.Description,
                    l.CreationDate
                ))
                .ToListAsync();
        }

        public async Task<ListSummaryResponseDto> CreateListAsync(NewListDto dto)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("Failed to authenticate user.");

            var list = new List
            {
                Name = dto.Name,
                UserId = userId,
                Description = dto.Description,
                IsPublic = dto.IsPublic,
                CreationDate = DateTime.Now
            };

            await _context.Lists.AddAsync(list);
            await _context.SaveChangesAsync();

            return new ListSummaryResponseDto(
                list.Id,
                list.Name,
                0,
                list.Description,
                list.CreationDate);
        }

        public async Task<ListSummaryResponseDto?> RenameListAsync(int listId, string newName)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("Failed to authenticate user.");

            var list = await _context.Lists
                .Include(l => l.TermLists)
                .Where(l => l.UserId == userId)
                .FirstOrDefaultAsync(l => l.Id == listId);

            if (list is null) return null;

            list.Name = newName;
            await _context.SaveChangesAsync();

            return new ListSummaryResponseDto(
                list.Id,
                list.Name,
                list.TermLists.Count,
                list.Description,
                list.CreationDate);
        }

        public async Task<bool> DeleteListAsync(int listId)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("Failed to authenticate user.");

            var list = await _context.Lists
                .Where(l => l.UserId == userId)
                .FirstOrDefaultAsync(l => l.Id == listId);

            if (list is null) return false;

            _context.Lists.Remove(list);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddTermAsync(int listId, int termId)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("Failed to authenticate user.");

            var list = await _context.Lists
                .Include(l => l.TermLists)
                .Where(l => l.UserId == userId)
                .FirstOrDefaultAsync(l => l.Id == listId);

            if (list is null) return false;

            var alreadyAdded = list.TermLists.Any(tl => tl.WordId == termId);
            if (alreadyAdded) return false;

            var termExists = await _context.Terms.AnyAsync(t => t.Id == termId && t.IsVisible);
            if (!termExists) return false;

            list.TermLists.Add(new TermList { WordId = termId, ListId = listId });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveTermAsync(int listId, int termId)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("Failed to authenticate user.");

            var list = await _context.Lists
                .Include(l => l.TermLists)
                .Where(l => l.UserId == userId)
                .FirstOrDefaultAsync(l => l.Id == listId);

            if (list is null) return false;

            var termList = list.TermLists.FirstOrDefault(tl => tl.WordId == termId);
            if (termList is null) return false;

            list.TermLists.Remove(termList);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region "ADMIN"
        public async Task<bool> CreateScheduledListAsync(NewFeaturedListDto dto)
        {
            // Validar de fechas
            if (dto.StartDate >= dto.EndDate)
            {
                throw new ArgumentException("La fecha de finalización debe ser posterior a la fecha de inicio.");
            }

            if (dto.StartDate < DateTime.UtcNow.Date)
            {
                throw new ArgumentException("No se puede programar una colección para una fecha pasada.");
            }

            //Validar existencia de la Lista
            var listExists = await _context.Lists
                .AnyAsync(l => l.Id == dto.ListId && l.IsPublic);

            if (!listExists) return false;

            // Crear registro temporal
            var featuredAllocation = new FeaturedList
            {
                ListId = dto.ListId,
                Category = dto.Category,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            await _context.FeaturedLists.AddAsync(featuredAllocation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FeaturedListScheduleResponseDto>> GetFeaturedScheduleAsync()
        {
            var today = DateTime.UtcNow;

            return await _context.FeaturedLists
                .AsNoTracking()
                .Include(f => f.List)
                .OrderByDescending(f => f.StartDate)
                .Select(f => new FeaturedListScheduleResponseDto(
                    f.Id,
                    f.ListId,
                    f.List.Name,
                    f.StartDate,
                    f.EndDate,
                    f.StartDate <= today && f.EndDate >= today && f.List.IsPublic
                ))
                .ToListAsync();
        }
        #endregion
    }
}