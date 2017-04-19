using System.Collections.Generic;

namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaModel)]
    public class ClassMeta : TableModel<ClassMeta>
    {
        public List<int> ExpToLevelUp { get; set; } = new List<int>();
        public List<string> Abilities { get; set; } = new List<string>();
        public Requirement Requirement { get; set; } = new Requirement();
        public List<string> DefaultItems { get; set; } = new List<string>();
        /// <summary>
        /// Whether the character of this class is available at the start of the game.
        /// </summary>
        public bool IsInitiallyAvailable { get; set; }

        public override string BaseTableName => "ClassMetas"; 

        public ClassMeta () { }
        public ClassMeta (string id) : base (id) { }
    }
}
