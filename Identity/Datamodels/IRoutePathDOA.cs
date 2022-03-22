using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Prinubes.Identity.Datamodels
{
    public interface IRoutePathDOA
    {
        void Dispose();
        void SyncronizeAsync(IReadOnlyList<ActionDescriptor> _descriptors);
    }
}