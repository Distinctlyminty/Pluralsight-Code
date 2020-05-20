using System;
using Xamarin.Forms;

namespace Wired_Brain_Coffee
{
    public interface IDependencyService
    {
        T Get<T>() where T : class;
    }

    public class DependencyServiceWrapper : IDependencyService
    {
        public T Get<T>() where T : class
        {
            return DependencyService.Get<T>();
        }
    }
}
