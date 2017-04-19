using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaDescribedModel)]
    public class Character : MetaDescribedModel<ClassMeta>
    {
        public string ClassMetaId => MetaId; 
        public int Level { get; set; } 
        public int Experience { get; set; }
        public int TalentPoints { get; set; }
        public List<string> LearnedTalentMetaIds { get; set; } = new List<string>();

        public Character () { }
        public Character (string classMetaId) : base(classMetaId) { }

        /// <summary>
        /// Adds experience points to the character and handles level up logic.
        /// </summary>
        /// <param name="exp">Amount of exp points to add.</param>
        /// <param name="classMeta">Provide class meta, so it won't be loaded again.</param>
        /// <returns>New level if level up occurs, null otherwise.</returns>
        public async Task<int?> EarnExperienceAsync (int exp, ClassMeta classMeta = null)
        {
            if (classMeta == null) classMeta = await GetMetaAsync();
            if (classMeta == null || await IsOnMaxLevel(classMeta)) return null;

            Experience += exp;

            if (Experience >= classMeta.ExpToLevelUp[Level + 1])
            {
                while (classMeta.ExpToLevelUp.Count > (Level + 1) && Experience >= classMeta.ExpToLevelUp[Level + 1])
                {
                    Level++;
                    TalentPoints++; // Currently character receives 1 talent point for each level.
                    Experience = Experience - classMeta.ExpToLevelUp[Level];
                }

                return Level;
            }

            return null;
        }

        /// <summary>
        /// Whether the character is on its maximum level. (Determined by ExpToEarn of class meta)
        /// </summary>
        /// <param name="classMeta">Provide class meta, so it won't be loaded again.</param>
        public async Task<bool> IsOnMaxLevel (ClassMeta classMeta = null)
        {
            if (classMeta == null) classMeta = await GetMetaAsync();
            return classMeta.ExpToLevelUp.Count <= (Level + 1);
        }

        /// <summary>
        /// Checks whether character learned the talent.
        /// </summary>
        public bool IsTalentLearned (string talentMetaId) => LearnedTalentMetaIds.Contains(talentMetaId);

        /// <summary>
        /// Attempts to learn a new talent by spending talent points and adding talent meta ID to the LearnedTalentMetaIds list.
        /// Ensures there are enough talent points, talent is not already learned and is linked with one the character class abilities and all the requirements are fulfilled.
        /// </summary>
        /// <param name="talentMeta">Meta of the talent to learn.</param>
        /// <returns>Whether the talent was learned.</returns>
        public async Task<bool> LearnTalent (TalentMeta talentMeta)
        {
            if (talentMeta == null) return false;

            if (TalentPoints < 1) return false; // Currently all talents cost is 1 point.

            if (IsTalentLearned(talentMeta.Id)) return false;

            // First-tier talents: possibly learning this talent group for the first time. 
            // Need to check if the affected ability of the talent is appropriate for the character class.
            if (Math.Abs(talentMeta.Tier) == 1)
            {
                var classMeta = await GetMetaAsync();
                if (!classMeta.Abilities.Contains(talentMeta.AbilityMetaId)) return false;
            }
            else // Checking if the character is able to learn talent of the tier.
            {
                var learnedTalentMetas = new List<TalentMeta>();
                foreach (var learnedTalentMetaId in LearnedTalentMetaIds)
                {
                    var learnedTalentMeta = await new TalentMeta(learnedTalentMetaId).LoadAsync();
                    if (learnedTalentMeta != null)
                        learnedTalentMetas.Add(await new TalentMeta(learnedTalentMetaId).LoadAsync());
                }

                // Currently the character needs to know at least n-1 talents in a vector tier to learn talent of a n-th tier in the same vector.
                var targetTier = talentMeta.Tier;
                if (targetTier > 0)
                {
                    var n = learnedTalentMetas.Count(learnedTalentMeta => learnedTalentMeta.AbilityMetaId == talentMeta.AbilityMetaId && learnedTalentMeta.Tier > 0);
                    if ((n + 1) < Math.Abs(targetTier)) return false;
                }
                else if (targetTier < 0)
                {
                    var n = learnedTalentMetas.Count(learnedTalentMeta => learnedTalentMeta.AbilityMetaId == talentMeta.AbilityMetaId && learnedTalentMeta.Tier < 0);
                    if ((n + 1) < Math.Abs(targetTier)) return false;
                }
            }

            LearnedTalentMetaIds.Add(talentMeta.Id);
            TalentPoints--;

            return true;
        }
    }
}
