using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SagaNetwork.Models
{
    /// <summary>
    /// Represents a set of rewards, that can be awarded to a player.
    /// </summary>
    [GenerateApi(UClassType.Struct)]
    public class Reward
    {
        /// <summary>
        /// The chance reward will be granted. Should be in 0.0 to 1.0 range.
        /// </summary>
        public float Probability { get; set; } = 1.0f;
        public List<int> Resources { get; set; } = new List<int>();
        /// <summary>
        /// If resources are specified, this will randomly vary cooresponding resource index.
        /// </summary>
        public List<int> ResourceDeltas { get; set; } = new List<int>();
        public List<string> ItemMetaIds { get; set; } = new List<string>();
        public int Experience { get; set; }
        /// <summary>
        /// If experience is specified, this will randomly vary provided amount.
        /// </summary>
        public int ExperienceDelta { get; set; }
        public int RandomCitizens { get; set; }
        public int CitizenSlots { get; set; }

        // TODO: Don't forget to clone new properties (or use reflection to auto-clone them).
        public Reward Clone ()
        {
            // .MemberwiseClone() is not working for some reason
            var other = new Reward();
            other.Probability = Probability;
            other.Resources = new List<int>(Resources);
            other.ResourceDeltas = new List<int>(ResourceDeltas);
            other.ItemMetaIds = new List<string>(ItemMetaIds);
            other.Experience = Experience;
            other.ExperienceDelta = ExperienceDelta;
            other.CitizenSlots = CitizenSlots;

            return other;
        }

        /// <summary>
        /// Executes all the random factors updating actual reward.
        /// </summary>
        /// <returns>Whether the reward should be granted.</returns>
        public bool RollTheDice ()
        {
            if (StaticRandom.RollTheDice(Probability))
            {
                if (ResourceDeltas?.Count > 0)
                    for (int i = 0; i < ResourceDeltas.Count; i++)
                        if (Resources.Count > i)
                            Resources[i] += StaticRandom.Next(-ResourceDeltas[i], ResourceDeltas[i]);

                if (Experience > 0 && ExperienceDelta > 0)
                        Experience += StaticRandom.Next(-ExperienceDelta, ExperienceDelta);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Grants reward to the specified player.
        /// </summary>
        /// <param name="player">Player to award.</param>
        public async Task AwardPlayerAsync (Player player)
        {
            if (Resources?.Count > 0)
                player.AddResources(Resources);

            if (ItemMetaIds?.Count > 0)
                foreach (var itemMetaId in ItemMetaIds)
                    player.AddItem(new Item(itemMetaId));

            if (Experience > 0)
                await player.GetSelectedCharacter().EarnExperienceAsync(Experience);

            if (RandomCitizens > 0)
                for (int i = 0; i < RandomCitizens && player.HasFreeCitizenSlot; i++)
                    player.Citizens.Add(await Citizen.GenerateRandomCitizenAsync());

            if (CitizenSlots > 0)
                player.CitizenSlots += CitizenSlots;
        }

        /// <summary>
        /// Calculates total volume of the awarded rewards; will occupy building storage.
        /// </summary>
        public int CalculateVolume () => Resources.Sum() + ItemMetaIds.Count;
    }
}
