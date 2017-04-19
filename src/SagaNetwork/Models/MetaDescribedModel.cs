using System;
using System.Threading.Tasks;

namespace SagaNetwork.Models
{
    /// <summary>
    /// Base class for non-table models described by a table model (meta).
    /// </summary>
    /// <typeparam name="T">Type of the meta which describes the model.</typeparam>
    public abstract class MetaDescribedModel<T> where T : TableModel<T>, new()
    {
        /// <summary>
        /// Unique auto-generated ID of the model.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the meta which describes the model.
        /// </summary>
        public string MetaId { get; set; }

        /// <summary>
        /// For serialization and model-binding. Don't use directly.
        /// </summary>
        protected MetaDescribedModel () : this(null) { }

        /// <summary>
        /// Creates a new meta-described model with an unique ID.
        /// </summary>
        /// <param name="metaId">ID of the meta used to describe the model.</param>
        protected MetaDescribedModel (string metaId)
        {
            Id = Guid.NewGuid().ToString();

            var newMetaId = string.IsNullOrWhiteSpace(metaId) ? "NULL" : metaId;
            MetaId = newMetaId;
        }

        public override int GetHashCode ()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return string.IsNullOrEmpty(Id) ? Guid.NewGuid().GetHashCode() : Id.GetHashCode();
        }

        public override bool Equals (object obj)
        {
            var mdm = obj as MetaDescribedModel<T>;
            if (mdm == null) return false;

            return mdm.Id == this.Id;
        }

        /// <summary>
        /// Get meta which describes the model.
        /// </summary>
        /// <returns>Model's meta.</returns>
        public async Task<T> GetMetaAsync ()
        {
            var meta = new T();
            meta.Id = MetaId;
            return await meta.LoadAsync();
        }
    }
}
