namespace ARCServer.Business.Dtos.Common
{
    /// <summary>
    /// Reusable list query: page, size, search and sort.
    /// Bind from query string: ?page=1&amp;pageSize=10&amp;search=&amp;sortBy=order&amp;sortDirection=asc
    /// </summary>
    public class PaginationRequestDto
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;

        public int Page { get; set; } = DefaultPage;

        public int PageSize { get; set; } = DefaultPageSize;

        public string? Search { get; set; }

        /// <summary>
        /// Field name to sort by (entity-specific, e.g. order, az, en, ru, id).
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// asc or desc (case-insensitive).
        /// </summary>
        public string SortDirection { get; set; } = "asc";

        public bool IsDescending =>
            string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        public string? NormalizedSearch =>
            string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();

        public string NormalizedSortBy =>
            string.IsNullOrWhiteSpace(SortBy) ? string.Empty : SortBy.Trim().ToLowerInvariant();

        public int Skip => Math.Max(Page - 1, 0) * PageSize;
    }
}
