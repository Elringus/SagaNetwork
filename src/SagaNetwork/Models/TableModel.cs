using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace SagaNetwork.Models
{
    /// <summary>
    /// Base class for models stored in the Azure Table Storage. Handles basic table config and operations. 
    /// Also provides JSON serialization for custom types (azure tables native support for types is limited).
    /// https://msdn.microsoft.com/en-us/library/azure/dd179338.aspx
    /// </summary>
    /// <typeparam name="T">
    /// Auto-constrainted generic type to fake return type covariance.
    /// Use child class type here (eg, Player : TableModel<Player/>).
    /// </typeparam>
    public abstract class TableModel<T> : TableEntity where T : TableModel<T>, new()
    {
        #region PROPERTIES
        /// <summary>
        /// Using constant shared table row key to allow point queries.
        /// http://stackoverflow.com/questions/31816798/
        /// </summary>
        public const string SHARED_ROW_KEY = "SHARED_ROW_KEY";

        /// <summary>
        /// Unique entity identifier, needed for models schema consistency.
        /// Mirrors partition key.
        /// </summary>
        [IgnoreProperty]
        public virtual string Id
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        /// <summary>
        /// Base name of the table that will contain the entities of the class. 
        /// </summary>
        public abstract string BaseTableName { get; }

        /// <summary>
        /// Represents fully qualified table name.
        /// Contains tier affix to differentiate between dev-test-prod deployments.
        /// </summary>
        public string FullTableName => $"{Configuration.TierAffix}{BaseTableName}"; 

        /// <summary>
        /// Custom attributes of the model used by the client. Should be agnostic of the controllers logic.
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Instance of the table controller to which the entity belongs.
        /// Will auto-create a table if it's not exists.
        /// </summary>
        protected CloudTable Table
        {
            get
            {
                if (_table == null)
                {
                    _table = CloudStorage.TableClient.GetTableReference(FullTableName);
                    _table.CreateIfNotExistsAsync().Wait();
                }
                return _table;
            }
        }

        private CloudTable _table;
        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// Default constructor is required by TableEntity for serialization purposes. 
        /// Should not be used directly.
        /// </summary>
        protected TableModel () : this(null) { }

        /// <summary>
        /// The main constructor. Prepares the entity to work with its table.
        /// </summary>
        /// <param name="id">ID of the entity. Will be auto-generated if null is passed.</param>
        protected TableModel (string id) 
        {
            // Not using GUID here to allow entity grouping by 
            // providing an incrementing pattern of the partition keys.
            if (string.IsNullOrWhiteSpace(id))
                id = DateTime.UtcNow.Ticks.ToString(); 

            PartitionKey = id;
            RowKey = SHARED_ROW_KEY;
        }
        #endregion

        /// <summary>
        /// Attempts to find and load an entity with corresponding ID from the table.
        /// </summary>
        /// <returns>Instance of the entity if found, null otherwise.</returns>
        public virtual async Task<T> LoadAsync ()
        {
            var retrieveOperation = TableOperation.Retrieve<T>(Id, SHARED_ROW_KEY);
            var retrieveResult = await Table.ExecuteAsync(retrieveOperation);

            return retrieveResult.Result as T;
        }

        /// <summary>
        /// Attempts to insert new entity if it's not already exists in the table.
        /// </summary>
        /// <returns>Whether the operation was succesfull.</returns>
        public virtual async Task<bool> InsertAsync (bool checkForExisting = true)
        {
            // TODO: handle '404 not found' exception here.
            if (checkForExisting && await LoadAsync() != null) return false;

            var insertOperation = TableOperation.Insert(this);
            var insertOperationResult = await Table.ExecuteAsync(insertOperation);

            return insertOperationResult.Result != null;
        }

        /// <summary>
        /// Attempts to replace (update) entity. Insures OCC.
        /// </summary>
        /// <returns>Whether OCC was positive.</returns>
        public virtual async Task<bool> ReplaceAsync ()
        {
            try
            {
                var replaceOperation = TableOperation.Replace(this);
                var replaceOperationResult = await Table.ExecuteAsync(replaceOperation);
            }
            catch (StorageException e)
            // Optimistic concurrency violation.
            when (e.RequestInformation.HttpStatusCode == 412)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to delete entity from the table. Insures OCC.
        /// </summary>
        /// <returns>Whether OCC was positive.</returns>
        public virtual async Task<bool> DeleteAsync ()
        {
            try
            {
                var deleteOperation = TableOperation.Delete(this);
                var deleteResult = await Table.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e)
            // Optimistic concurrency violation.
            when (e.RequestInformation.HttpStatusCode == 412)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves all the entities from the table.
        /// Might be very slow — use with caution.
        /// </summary>
        /// <param name="limit">Limit max count of the results.</param>
        /// <returns>List of all the entities in the table.</returns>
        public virtual async Task<List<T>> RetrieveAllAsync (int? limit = null)
        {
            TableContinuationToken continuationToken = null;
            var entities = new List<T>();
            var tableQuery = new TableQuery<T>();
            if (limit.HasValue && limit > 9999)
                limit = 9999; // max query limit is 10000
            tableQuery.TakeCount = limit;

            do
            {
                var queryResult = await Table.ExecuteQuerySegmentedAsync(tableQuery, continuationToken);
                entities.AddRange(queryResult.Results);

                if (limit.HasValue && entities.Count >= limit)
                    break;

                continuationToken = queryResult.ContinuationToken;
            }
            while (continuationToken != null);

            // Isolating entity data for the test environment.
            if (Configuration.IsTestEnvironment)
                entities.RemoveAll(e => !e.Id.StartsWith("Test_"));
            else entities.RemoveAll(e => e.Id.StartsWith("Test_"));

            return entities;
        }

        #region SERIALIZATION
        public override IDictionary<string, EntityProperty> WriteEntity (OperationContext operationContext)
        {
            var properties = base.WriteEntity(operationContext);

            // Iterating through the properties of the entity
            foreach (var property in GetType().GetProperties().Where(property =>
                    // Excluding props which are explicitly marked to ignore serialization
                    !property.GetCustomAttributes<IgnorePropertyAttribute>(true).Any() &&
                    // Excluding already serialized props
                    !properties.ContainsKey(property.Name) &&
                    // Excluding internal TableEntity props
                    typeof(TableEntity).GetProperties().All(p => p.Name != property.Name)))
            {
                var value = property.GetValue(this);
                if (value != null)
                    // Serializing property to JSON
                    properties.Add(property.Name, new EntityProperty(JsonConvert.SerializeObject(value)));
            }

            return properties;
        }

        public override void ReadEntity (IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);

            // Iterating through the properties of the entity
            foreach (var property in GetType().GetProperties().Where(property =>
                    // Excluding props without a set accessor
                    property.CanWrite &&
                    // Excluding props which are explicitly marked to ignore serialization
                    !property.GetCustomAttributes<IgnorePropertyAttribute>(true).Any() &&
                    // Excluding props which were not originally serialized
                    properties.ContainsKey(property.Name) &&
                    // Excluding props with target type of string (they are natively supported)
                    property.PropertyType != typeof(string) &&
                    // Excluding non-string table (EDM) fields (this will filter-out 
                    // all the remaining natively supported props like byte, DateTime, etc)
                    properties[property.Name].PropertyType == EdmType.String))
            {
                // Checking if property contains a valid JSON
                var jToken = JParser.TryParse(properties[property.Name].StringValue);
                if (jToken != null)
                {
                    // Constructing method for deserialization 
                    var toObjectMethod = jToken.GetType().GetMethod("ToObject", new[] { typeof(Type) });
                    // Invoking the method with the target property type; eg, jToken.ToObject(CustomType)
                    var value = toObjectMethod.Invoke(jToken, new object[] { property.PropertyType });

                    property.SetValue(this, value);
                }
            }
        }
        #endregion
    }
}
