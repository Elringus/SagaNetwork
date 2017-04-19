using System.Collections.Generic;
using System.Threading.Tasks;

namespace SagaNetwork.Models
{
    /// <summary>
    /// Stores a set of variations for the described appearance element.
    /// </summary>
    [GenerateApi(UClassType.MetaDescribedModel)]
    public class AppearanceElement : MetaDescribedModel<AppearanceElementMeta>
    {
        public List<int> Variations { get; set; } = new List<int>();

        public AppearanceElement () { }
        public AppearanceElement (string metaId) : base (metaId) { }

        /// <summary>
        /// Sets random variations based on cooresponding meta.
        /// </summary>
        /// <param name="appearanceElementMeta">Provide to skip meta retrieving.</param>
        public async Task RandomizeVarationsAsync (AppearanceElementMeta appearanceElementMeta = null)
        {
            Variations.Clear();

            if (appearanceElementMeta == null)
                appearanceElementMeta = await GetMetaAsync();

            foreach (var variationRange in appearanceElementMeta.VariationRanges)
                Variations.Add(StaticRandom.Next(variationRange.MinValue, variationRange.MaxValue));
        }
    }
}
