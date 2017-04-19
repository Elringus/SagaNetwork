using System.Threading.Tasks;

namespace SagaNetwork.Models
{
    /// <summary>
    /// A model with single instance across deployment tier.
    /// Doesn't need ID to be retrieved and is always available.
    /// </summary>
    public abstract class SingletonModel<T> : TableModel<T> where T : SingletonModel<T>, new()
    {
        protected SingletonModel ()
        {
            // TODO: check this.
            // ReSharper disable VirtualMemberCallInConstructor
            Id = $"{Configuration.DeploymentTier}{BaseTableName}";
        }

        public override async Task<T> LoadAsync ()
        {
            // Ensure the entity is always available.

            var entity = await base.LoadAsync();

            if (entity == null)
            {
                await InsertAsync(false);
                return this as T;
            }

            return entity;
        }
    }
}
