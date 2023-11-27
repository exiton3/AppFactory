using Microsoft.EntityFrameworkCore;

namespace Framework.DataLayer.ReadModel
{
    public interface IContextFactory
    {
        DbContext Create();
    }
}