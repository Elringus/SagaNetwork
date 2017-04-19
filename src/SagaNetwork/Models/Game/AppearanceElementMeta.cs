using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.Enum)]
    public enum AppearanceGroup { Headdress, Hairstyle, Face, Beard, Torso }

    /// <summary>
    /// Describes an arbitrary appeareance element.
    /// </summary>
    [GenerateApi(UClassType.MetaModel)]
    public class AppearanceElementMeta : TableModel<AppearanceElementMeta>
    {
        public AppearanceGroup AppearanceGroup { get; set; }
        public Gender Gender { get; set; }
        public string AssetPath { get; set; }
        public List<IntRange> VariationRanges { get; set; } = new List<IntRange>();

        public override string BaseTableName => "AppearanceElementMetas";

        public AppearanceElementMeta () { }
        public AppearanceElementMeta (string id) : base (id) { }
    }
}
