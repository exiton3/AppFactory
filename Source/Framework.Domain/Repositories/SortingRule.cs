namespace Framework.Domain.Repositories
{
    public class SortingRule
    {
        public SortingRule()
        {
            SortOrder = SortOrder.Asc;
        }

        public string Field { get; set; }

        public SortOrder SortOrder { get; set; }
    }
}