using Microsoft.WindowsAzure.Storage.Table;

namespace SagaNetwork.Models
{
    /// <summary>
    /// Represents meta-info about JSON file kept in blob storage.
    /// </summary>
    public class JsonBlob : TableModel<JsonBlob>
    {
        /// <summary>
        /// Path (relative to blob container) to the JSON file represented by this JsonBlob object.
        /// </summary>
        public string BlobPath => $"json/{Id}.json";

        /// <summary>
        /// A hack to pass JSON text from view to controller.
        /// </summary>
        [IgnoreProperty]
        public string JsonText { get; set; }

        public override string BaseTableName => "JsonBlobs";

        public JsonBlob () { }
        public JsonBlob (string id) : base (id) { }
    }
}
