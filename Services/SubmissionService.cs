using DictionaryAPI.Data;
using DictionaryAPI.Interfaces;
using DictionaryAPI.Models.DTOs.Submission;
using DictionaryAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DictionaryAPI.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public SubmissionService(ApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        #region "PUBLIC"
        // Crud operations for submissions

        // User can view their own submissions
        public async Task<IEnumerable<SubmissionSummaryResponseDto>> GetUserSubmissionsAsync()
        {
            var userId = _currentUser?.UserId ?? throw new UnauthorizedAccessException("Failed to authenticate user.");

            return await _context.Submissions
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreationDate)
                .Select(s => new SubmissionSummaryResponseDto(
                    s.Id,
                    s.User!.UserName!,
                    s.Word,
                    s.CreationDate,
                    s.StatusId
                ))
                .ToListAsync();
        }
        #endregion

        #region "PRIVATE"

        // Get all submissions for admin view
        public async Task<IEnumerable<SubmissionSummaryResponseDto>> GetSubmissionsAsync(int? statusId)
        {
            int defaultStatusId = 1;
            var statusToFilter = statusId ?? defaultStatusId;

            return await _context.Submissions
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Status)
                .Where(s => s.StatusId == statusToFilter)
                .OrderBy(s => s.CreationDate)
                .Select(s => new SubmissionSummaryResponseDto
                (
                    s.Id,
                    s.UserId,
                    s.Word,
                    s.CreationDate,
                    s.StatusId
                ))
                .ToListAsync();
        }

        public async Task<int?> ApproveSubmissionAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var submission = await _context.Submissions.FindAsync(id);

                // Validate if the submission exists and its status is 1: "Pending"
                if (submission == null || submission.StatusId != 1) return null;

                // Change id to 2
                submission.StatusId = 2;
                _context.Submissions.Update(submission);

                // Clone into Terms with IsVisible = false
                var nuevoTermino = new Term
                {
                    Word = submission.Word,
                    Definition = submission.Definition!,
                    Example = submission.Example,
                    Category = submission.Category,
                    IsVisible = false
                };

                await _context.Terms.AddAsync(nuevoTermino);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return nuevoTermino.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 3. RECHAZAR: Cambia el ID al número correspondiente (ej: 3 para Rejected)
        public async Task<bool> RejectSubmissionAsync(int id)
        {
            var suggestion = await _context.Submissions.FindAsync(id);

            if (suggestion == null || suggestion.StatusId != 1) return false;

            suggestion.StatusId = 3; // 👈 3 = Rejected

            _context.Submissions.Update(suggestion);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region "SHARED"
        public async Task<SubmissionDetailResponseDto> CreateSubmissionAsync(NewSubmissionDto request)
        {
            //var user = await _context.Users.FindAsync(userId);
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Failed to authenticate user.");

            var submission = new Submission
            {
                UserId = userId,
                Word = request.Word,
                Definition = request.Definition,
                ExtraInformation = request.ExtraInformation,
                Example = request.Example,
                Etymology = request.Etymology,
                Category = request.Category,
                CreationDate = DateTime.Now,
                StatusId = 1
            };
            //_context.Submissions.Add(submission);

            await _context.Submissions.AddAsync(submission);
            await _context.SaveChangesAsync();

            return await GetSubmissionByIdAsync(submission.Id)
                ?? throw new InvalidOperationException("Submission created, but failed to retrieved it");
        }

        public async Task<SubmissionDetailResponseDto?> GetSubmissionByIdAsync(int submissionId)
        {
            var userId = _currentUser.UserId;
            var isAdmin = _currentUser.IsAdmin;

            var query = _context.Submissions
                .Include(s => s.User)
                .Include(s => s.Status)
                .AsQueryable();
           
            if (!isAdmin)
            {
                query = query.Where(s => s.UserId == userId);
            }

            var submission = await query
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return null;

            return new SubmissionDetailResponseDto
                (
                submission.Id,
                submission.User!.UserName!,
                submission.Word,
                submission.Definition,
                submission.ExtraInformation,
                submission.Example,
                submission.Etymology,
                submission.Category,
                submission.CreationDate,
                submission.StatusId
            );
        }
        #endregion
    }
}
