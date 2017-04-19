using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.Struct)]
    public class Citizen 
    {
        public string Id { get; set; }
        public string RarenessGroupId { get; set; }
        public Gender CitizenGender { get; set; }
        public List<AppearanceElement> Appearance { get; set; } = new List<AppearanceElement>();
        public List<CitizenAbility> Abilities { get; set; } = new List<CitizenAbility>();

        public Citizen ()
        {
            Id = Guid.NewGuid().ToString();
        }

        public static async Task<Citizen> GenerateRandomCitizenAsync ()
        {
            var generatedCitizen = new Citizen();

            var rarenessGroups = await new RarenessGroup().RetrieveAllAsync();
            generatedCitizen.RarenessGroupId = rarenessGroups.WeightedRandom().Id;
            var rarenessGroup = rarenessGroups.Find(rg => rg.Id == generatedCitizen.RarenessGroupId);

            generatedCitizen.CitizenGender = StaticRandom.Bool ? Gender.Female : Gender.Male;

            var appearanceElementMetas = await new AppearanceElementMeta().RetrieveAllAsync();
            foreach (AppearanceGroup appearanceGroup in Enum.GetValues(typeof(AppearanceGroup)))
            {
                var appearanceElementMeta = appearanceElementMetas.Where(x => x.AppearanceGroup == appearanceGroup
                    && (x.Gender == generatedCitizen.CitizenGender || x.Gender == Gender.Undefined)).Random();
                if (appearanceElementMeta == null) continue;
                var appearanceElement = new AppearanceElement(appearanceElementMeta.Id);
                await appearanceElement.RandomizeVarationsAsync(appearanceElementMeta);
                generatedCitizen.Appearance.Add(appearanceElement);
            }

            var citizenAbilityMetas = await new CitizenAbilityMeta().RetrieveAllAsync();
            for (int i = 0; i < rarenessGroup.AbilityLimit; i++)
            {
                if (citizenAbilityMetas.Count == 0) break;

                var selectedAbility = citizenAbilityMetas.WeightedRandom();
                generatedCitizen.Abilities.Add(new CitizenAbility(selectedAbility.Id));
                citizenAbilityMetas.Remove(selectedAbility);
            }

            return generatedCitizen;
        }

        public async Task<RarenessGroup> GetRarenessGroupAsync() => await new RarenessGroup(RarenessGroupId).LoadAsync();
    }
}
