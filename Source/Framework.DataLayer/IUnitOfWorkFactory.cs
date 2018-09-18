namespace Framework.DataLayer
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWorkDbContext Create();
    }
}