using System.Diagnostics;
using Autofac;

namespace Framework.DataLayer
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly ILifetimeScope _kernel;

        public UnitOfWorkFactory(ILifetimeScope kernel)
        {
            _kernel = kernel;
        }

        public IUnitOfWorkDbContext Create()
        {
            var unitOfWork = _kernel.Resolve<IUnitOfWorkDbContext>();
            Debug.WriteLine("UoW from IoC - {0}", unitOfWork.GetHashCode());
            return unitOfWork;
        }
    }
}