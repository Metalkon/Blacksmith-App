namespace Shared_Classes.Models
{
    public class PaginationDTO
    {
        public int PageNumber;
        public int PageSize;
        public int LastItemId;
        public string SearchQuery;

        public PaginationDTO()
        {
            PageNumber = 1;
            PageSize = 5;
            LastItemId = 0;
            SearchQuery = "";
        }
    }
}
