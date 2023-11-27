using Microsoft.EntityFrameworkCore;

namespace AppFactory.Framework.DataLayer.ReadModel
{
    public interface IContextFactory
    {
        DbContext Create();
    }
}