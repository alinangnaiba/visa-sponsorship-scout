namespace VisaSponsorshipScout.Core.Models
{
    public class PagedResult<T>
    {
        public int CurrentPage { get; set; }
        public List<T> Data { get; set; } = [];
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }
}