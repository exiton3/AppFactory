using System.Data.Entity;

namespace Framework.DataLayer.ReadModel
{
    public interface IContextFactory
    {
        DbContext Create();
    }
}