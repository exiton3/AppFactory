namespace AppFactory.Framework.DataLayer
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWorkDbContext Create();
    }
}