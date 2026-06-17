using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Runtime.CompilerServices;

namespace Pupa.BusinessObjects
{
    internal sealed class NullLazyLoader : ILazyLoader
    {
        private NullLazyLoader()
        {
        }

        public static NullLazyLoader Instance { get; } = new NullLazyLoader();

        public void Load(object entity, [CallerMemberName] string navigationName = "")
        {
        }

        public Task LoadAsync(object entity, CancellationToken cancellationToken = default, [CallerMemberName] string navigationName = "")
            => Task.CompletedTask;

        public void SetLoaded(object entity, [CallerMemberName] string navigationName = "", bool loaded = true)
        {
        }

        public void Dispose()
        {
        }

        public bool IsLoaded(object entity, [CallerMemberName] string navigationName = "")
        {
            throw new NotImplementedException();
        }
    }
}
