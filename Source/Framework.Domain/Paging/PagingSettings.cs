namespace Framework.Domain.Paging
{
    public class PagingSettings
    {
        public PagingSettings()
            : this(50)
        { }

        public PagingSettings(int pageSize, int pageNumber = 1)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
        }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}