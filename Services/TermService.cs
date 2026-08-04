using DictionaryAPI.Data;
using DictionaryAPI.Interfaces;
using DictionaryAPI.Models.DTOs.Term;
//using DictionaryAPI.Models.DTOs.Term.TermResponseDto;
using DictionaryAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DictionaryAPI.Services
{
    public class TermService : ITermService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public TermService(ApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        #region "PUBLIC"
        //Public CRUD operations
        // Home service for daily word
        public async Task<TermSummaryPublicResponseDto?> GetDailyWordAsync()
        {
            var visibleTermsQuery = _context.Terms
                .AsNoTracking()
                .Where(t => t.IsVisible);

            var totalWords = await visibleTermsQuery.CountAsync();
            if (totalWords == 0)
                return null;

            var dayNumber = DateTime.UtcNow.DayOfYear;

            return await visibleTermsQuery
                .OrderBy(w => w.Id)
                .Skip(dayNumber % totalWords)
                .Select(t => new TermSummaryPublicResponseDto
                (
                    t.Id,
                    t.Word,
                    t.Definition,
                    t.Etymology,
                    t.Category
                ))
                .FirstOrDefaultAsync();
        }

        //Get term by id
        public async Task<TermDetailPublicResponseDto?> GetPublicTermByIdAsync(int id)
        {
            // Implementation to get term by id from the database
            var term = await _context.Terms
                // o relatedword
                .Include(t => t.OutgoingTermRelations).ThenInclude(r => r.RelatedWord)
                .Include(t => t.TermTags).ThenInclude(tt => tt.Tag)
                .Where(t => 
                t.IsVisible)
                .FirstOrDefaultAsync(t => t.Id == id);


           
            if (term == null) return null;
            return new TermDetailPublicResponseDto
               (
                term.Id,
                term.Word,
                term.Definition,
                term.ExtraInformation,
                term.Example,
                term.Etymology,
                term.Category,
                term.TermTags.Select(tt => tt.Tag.Name),
                term.OutgoingTermRelations.Select(r =>
                new TermRelationResponseDto(r.RelatedWordId, r.RelatedWord?.Word, r.RelationType))
                );
        }

        //Get all terms
        public async Task<IEnumerable<TermSummaryPublicResponseDto>> GetAllPublicTermsAsync()
        {
            // Implementation to get all terms from the database
            return await _context.Terms
                .AsNoTracking()
                .Where(t => t.IsVisible)
            .Select(t => new TermSummaryPublicResponseDto
            (
                t.Id,
                t.Word,
                t.Definition,
                t.Etymology,
                t.Category
             ))
            .ToListAsync();
            //throw new NotImplementedException();
        }

        public async Task<IEnumerable<TermSummaryPublicResponseDto>> GetTermsByEtymologyAsync(string keyword)
        {
            return await _context.Terms
                .AsNoTracking()
                .Where(t => EF.Functions.Like(t.Etymology, $"%{keyword}%"))
                .OrderBy(t => t.Word)
                .Select(t => new TermSummaryPublicResponseDto(
                    t.Id,
                    t.Word,
                    t.Definition,
                    t.Etymology,
                    t.Category
                ))
                .ToListAsync();
        }

        public async Task<IEnumerable<TermSummaryPublicResponseDto>> SearchPublicTermsAsync(string? query, TermCategory? category, string? orderBy)
        {
            var termQuery = _context.Terms.AsNoTracking().Where(t => t.IsVisible);

            // Query search
            if (!string.IsNullOrWhiteSpace(query))
            {
                termQuery = termQuery.Where(t => t.Word.Contains(query) || t.Definition.Contains(query));
            }

            // Category filter
            if (category.HasValue)
            {
                termQuery = termQuery.Where(t => t.Category == category.Value);
            }

            // Order
            termQuery = orderBy?.ToLower() switch
            {
                "word_desc" => termQuery.OrderByDescending(t => t.Word),
                "word_asc" => termQuery.OrderBy(t => t.Word),
                _ => termQuery.OrderBy(t => t.Word) // Default order
            };

            // Execute query
            return await termQuery
                .Select(t => new TermSummaryPublicResponseDto(
                    t.Id,
                    t.Word,
                    t.Definition,
                    t.Etymology,
                    t.Category))
                .ToListAsync();
        }
        #endregion

        #region "PRIVATE"
        //Create a new term
        public async Task<TermDetailPrivateResponseDto> CreateTermAsync(NewTermDto request)
        {
            // Implementation to create a new term in the database
            var term = new Term
            {
                Word = request.Word,
                Definition = request.Definition,
                ExtraInformation = request.ExtraInformation,
                Example = request.Example,
                Etymology = request.Etymology,
                Category = request.Category,
                CreationDate = DateTime.UtcNow,
                VideoUrl = request.VideoUrl
            };

            _context.Terms.Add(term);

            await _context.SaveChangesAsync();

            await SyncTagsAsync(term, request.Tags);
            await SyncRelationsAsync(term, request.RelatedTerms);

            await _context.SaveChangesAsync();

            return await GetPrivateTermByIdAsync(term.Id)
            ?? throw new InvalidOperationException("Term creation was succesfull, but failed to retrieve the created term.");
        }

        //Update an existing term
        public async Task<TermDetailPrivateResponseDto?> UpdateTermAsync(int id, UpdateTermDto request)
        {
            // Implementation to update an existing term in the database
            var term = await _context.Terms
                .Include(t => t.TermTags)
                .Include(t => t.OutgoingTermRelations)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (term is null) return null!;

            term.Word = request.Word!;
            term.Definition = request.Definition!;
            term.ExtraInformation = request.ExtraInformation;
            term.Example = request.Example;
            term.Etymology = request.Etymology;
            term.Category = request.Category;
            term.UpdatedAt = DateTime.Now;
            term.VideoUrl = request.VideoUrl;

            await SyncTagsAsync(term, request.Tags);
            await SyncRelationsAsync(term, request.RelatedTerms);

            await _context.SaveChangesAsync();

            return await GetPrivateTermByIdAsync(term.Id);
        }

        //Get term by id
        public async Task<TermDetailPrivateResponseDto?> GetPrivateTermByIdAsync(int id)
        {
            // Implementation to get term by id from the database
            var term = await _context.Terms
                // o relatedword
                .Include(t => t.OutgoingTermRelations).ThenInclude(r => r.RelatedWord)
                .Include(t => t.TermTags).ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t => t.Id == id);



            if (term == null) return null;
            return new TermDetailPrivateResponseDto
               (
                term.Id,
                term.Word,
                term.Definition,
                term.ExtraInformation,
                term.Example,
                term.Etymology,
                term.Category,
                term.TermTags.Select(tt => tt.Tag.Name),
                term.OutgoingTermRelations.Select(r =>
                new TermRelationResponseDto(r.RelatedWordId, r.RelatedWord?.Word, r.RelationType)),
                term.CreationDate,
                term.UpdatedAt,
                term.IsVisible,
                term.VideoUrl
                );
        }

        //Get all terms
        public async Task<IEnumerable<TermSummaryPrivateResponseDto>> GetAllPrivateTermsAsync()
        {
            // Implementation to get all terms from the database
            return await _context.Terms
                .AsNoTracking()
            .Select(t => new TermSummaryPrivateResponseDto
            (
                t.Id,
                t.Word,
                t.Definition,
                t.Category,
                t.IsVisible,
                t.CreationDate
             ))
            .ToListAsync();
            //throw new NotImplementedException();
        }

        private async Task SyncTagsAsync(Term term, IEnumerable<string>? tagNames)
        {
            term.TermTags.Clear();

            if (tagNames is null) return;

            foreach (var name in tagNames.Distinct())
            {
                var tag = await _context.Set<Tag>()
                    .FirstOrDefaultAsync(t => t.Name == name)
                    ?? new Tag { Name = name };

                var termTag = new TermTag
                {
                    Word = term,
                    Tag = tag
                };

                term.TermTags.Add(termTag);
            }
        }

        private async Task SyncRelationsAsync(Term term, IEnumerable<TermRelationDto>? relatedTermIds)
        {
            term.OutgoingTermRelations.Clear();

            if (relatedTermIds is null) return;

            foreach (var rel in relatedTermIds.Distinct())
            {
                if (rel.RelatedTermId == term.Id) continue; // evita auto-relación

                var relatedExists = await _context.Terms.AnyAsync(t => t.Id == rel.RelatedTermId);
                if (!relatedExists) continue; // o lanza excepción si prefieres ser estricto

                term.OutgoingTermRelations.Add(new TermRelation
                {
                    RelatedWordId = rel.RelatedTermId,
                    RelationType = rel.RelationType
                });
            }
        }

        // Change the visibility of a term (or soft delete)
        public async Task<bool> ToggleVisibilityAsync(int id)
        {
            var term = await _context.Terms.FindAsync(id);
            if (term == null) return false;

            term.IsVisible = !term.IsVisible;

            _context.Terms.Update(term);
            await _context.SaveChangesAsync();
            return true;
        }

        // Hard delete a term
        public async Task<bool> HardDeleteTermAsync(int id)
        {
            // Delete a term from the database (hard delete)
            var term = await _context.Terms.FindAsync(id);
            if (term == null) return false;

            _context.Terms.Remove(term);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<TermSummaryPrivateResponseDto>> SearchPrivateTermsAsync(string? query, TermCategory? category, bool? isVisible, bool? hasVideo, string? orderBy)
        {
            var termQuery = _context.Terms.AsNoTracking();

            // Query search
            if (!string.IsNullOrWhiteSpace(query))
            {
                termQuery = termQuery.Where(t => t.Word.Contains(query) || t.Definition.Contains(query));
            }

            // Category filter
            if (category.HasValue)
            {
                termQuery = termQuery.Where(t => t.Category == category.Value);
            }

            // Visibility filter
            if (isVisible.HasValue)
            {
                termQuery = termQuery.Where(t => t.IsVisible == isVisible.Value);
            }

            // Has video filter
            if (hasVideo.HasValue)
            {
                termQuery = hasVideo.Value
                    ? termQuery.Where(t => t.VideoUrl != null && t.VideoUrl != "")
                    : termQuery.Where(t => t.VideoUrl == null || t.VideoUrl == "");
            }

            // Order
            termQuery = orderBy?.ToLower() switch
            {
                "word_desc" => termQuery.OrderByDescending(t => t.Word),
                "date_asc" => termQuery.OrderBy(t => t.CreationDate),
                "date_desc" => termQuery.OrderByDescending(t => t.CreationDate),
                _ => termQuery.OrderBy(t => t.Word) // Default order
            };

            // Execute query
            return await termQuery
                .Select(t => new TermSummaryPrivateResponseDto(
                    t.Id, 
                    t.Word,
                    t.Definition,
                    t.Category,
                    t.IsVisible,
                    t.CreationDate))
                .ToListAsync();
        }
        #endregion

        //#region "SHARED"
        ////Search terms by query
        //public async Task<IEnumerable<TermSummaryPublicResponseDto>> SearchTermsAsync(string query, bool showHidden)
        //{
        //    var termsQuery = _context.Terms.AsNoTracking();

        //    if (!showHidden)
        //    {
        //        termsQuery = termsQuery.Where(t => t.IsVisible);
        //    }
            
        //    return await termsQuery
        //        .Where(t => t.Word.Contains(query) || t.Definition.Contains(query))
        //        .Select(t => new TermSummaryPublicResponseDto
        //        (
        //            t.Id,
        //            t.Word,
        //            t.Definition,
        //            t.Category
        //        ))
        //        .ToListAsync();
        //    //throw new NotImplementedException();
        //}
        //#endregion
    }
}
