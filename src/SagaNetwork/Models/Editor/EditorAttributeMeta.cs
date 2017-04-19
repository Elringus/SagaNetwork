using System.Collections.Generic;

namespace SagaNetwork.Models
{
    /// <summary>
    /// Used to populate drop-down menus in editor for predefined attribute keys and values.
    /// Meta postfix used to allow migration between tiers with the other metas.
    /// ID of the entity is used to bind with specific entity type editors through reflection.
    /// </summary>
    public class EditorAttributeMeta : TableModel<EditorAttributeMeta>
    {
        public List<string> PredefinedKeys { get; set; } = new List<string>();
        public List<string> PredefinedValues { get; set; } = new List<string>();

        public override string BaseTableName => "EditorAttributeMetas"; 

        public EditorAttributeMeta () { }
        public EditorAttributeMeta (string id) : base (id) { }
    }
}
