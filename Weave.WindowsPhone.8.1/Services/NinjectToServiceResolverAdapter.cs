using Ninject;
using SelesGames;
using System;

namespace Weave.Services
{
    public class NinjectToServiceResolverAdapter : IServiceResolver
    {
        IKernel kernel;

        public NinjectToServiceResolverAdapter(IKernel kernel)
        {
            if (kernel == null)
                throw new ArgumentNullException("kernel can't be null");

            this.kernel = kernel;
        }

        public T Get<T>()
        {
            return kernel.Get<T>();
        }

        public T Get<T>(string key)
        {
            return kernel.Get<T>(key);
        }
    }
}
