using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SagaNetwork.Models
{
    /// <summary>
    /// Unique 10-digit+alphabetical key used to grant access for new player account registration.
    /// </summary>
    public class AccessKey : TableModel<AccessKey>
    {
        public bool IsActivated { get; set; } = false;
        public DateTime GenerationDate { get; set; }
        public DateTime? ActivationDate { get; set; }
        public string AssociatedEmail { get; set; }

        public override string BaseTableName => "AccessKeys"; 

        public AccessKey () { }
        public AccessKey (string id) : base (id) { }

        /// <summary>
        /// Generates a new unique key and adds it to the table.
        /// </summary>
        /// <param name="existingKeys">Provide existing keys so the method won't query them.</param>
        /// <param name="associatedEmail">An email to associate with the key.</param>
        public async Task<AccessKey> AddAsync (List<AccessKey> existingKeys = null, string associatedEmail = null)
        {
            if (existingKeys == null)
                existingKeys = await RetrieveAllAsync();

            var key = GenerateKey();
            while (key == null || existingKeys.Any(accessKey => accessKey.Id == key))
                key = GenerateKey();

            var newKey = new AccessKey(key);
            newKey.IsActivated = false;
            newKey.GenerationDate = DateTime.UtcNow;
            newKey.ActivationDate = null;
            newKey.AssociatedEmail = associatedEmail;

            await newKey.InsertAsync();

            return newKey;
        }

        //public async Task<List<AccessKey>> RetrieveAllUngivenAsync ()
        //{
        //    TableContinuationToken continuationToken = null;
        //    var ungivenKeys = new List<AccessKey>();

        //    do
        //    {
        //        var query = new TableQuery<AccessKey>()
        //            .Where(TableQuery.GenerateFilterConditionForBool("IsGiven", QueryComparisons.Equal, false));

        //        var queryResult = await Table.ExecuteQuerySegmentedAsync(query, continuationToken);

        //        ungivenKeys.AddRange(queryResult.Results);
        //        continuationToken = queryResult.ContinuationToken;
        //    }
        //    while (continuationToken != null && ungivenKeys.Count < 100);

        //    return ungivenKeys;
        //}

        public async void Activate ()
        {
            IsActivated = true;
            ActivationDate = DateTime.UtcNow;
            await ReplaceAsync();
        }

        private static string GenerateKey ()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
