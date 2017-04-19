using System.Collections.Generic;

namespace SagaNetwork.Models
{
    /// <summary>
    /// Represents a requirement for a player to have a character of specific class and level.
    /// </summary>
    [GenerateApi(UClassType.Struct)]
    public class ClassRequirement
    {
        public string ClassMetaId { get; set; }
        public int Level { get; set; }

        public ClassRequirement () { }
        public ClassRequirement (string classMetaId = null, int level = 0)
        {
            ClassMetaId = classMetaId;
            Level = level;
        }
    }

    /// <summary>
    /// Represents a set of requirements for a player.
    /// </summary>
    [GenerateApi(UClassType.Struct)]
    public class Requirement
    {
        /// <summary>
        /// How much resources of each index player need to fulfill the requirement.
        /// </summary>
        public List<int> Resources { get; set; } = new List<int>();
        /// <summary>
        /// Characters of which classes and levels player need to have to fulfill the requirement.
        /// </summary>
        public List<ClassRequirement> ClassRequirements { get; set; } = new List<ClassRequirement>();
        /// <summary>
        /// Wards player need to have unlocked in order to fulfill the requirement.
        /// </summary>
        public List<string> WardMetaIds { get; set; } = new List<string>();
        /// <summary>
        /// Buildings player need to have constructed in order to fulfill the requirement.
        /// </summary>
        public List<string> BuildingMetaIds { get; set; } = new List<string>();

        /// <summary>
        /// Checks if the provided player has all the requirements fulfilled.
        /// </summary>
        /// <param name="player">Player to check.</param>
        public bool IsFulfilledByPlayer (Player player)
        {
            if (Resources?.Count > 0)
            {
                if (!player.CanAfford(Resources)) return false;
            }

            if (ClassRequirements?.Count > 0)
            {
                foreach (var classReq in ClassRequirements)
                {
                    if (classReq.Level > 0 && classReq.ClassMetaId != null)
                    {
                        if (!player.HasCharacterOfClass(classReq.ClassMetaId)) return false;
                        if (player.GetCharacterByClass(classReq.ClassMetaId).Level < classReq.Level) return false;
                        continue;
                    }

                    if (classReq.Level > 0 && player.Characters.Find(c => c.Level >= classReq.Level) == null) return false;
                    if (classReq.ClassMetaId != null && !player.HasCharacterOfClass(classReq.ClassMetaId)) return false;
                }
            }

            if (WardMetaIds?.Count > 0)
            {
                var unlockedWardMetaIds = new List<string>();
                foreach (var ward in player.City.Wards)
                    if (ward.IsUnlocked)
                        unlockedWardMetaIds.Add(ward.MetaId);

                foreach (var wardMetaId in unlockedWardMetaIds)
                    if (!unlockedWardMetaIds.Contains(wardMetaId)) return false;
            }

            if (BuildingMetaIds?.Count > 0)
            {
                var constructedBuildingIds = new List<string>();
                foreach (var ward in player.City.Wards)
                    foreach (var spot in ward.BuildingSpots)
                        if (spot.Building != null && spot.Building.IsConstructed)
                            constructedBuildingIds.Add(spot.Building.MetaId);

                foreach (var buildingId in BuildingMetaIds)
                    if (!constructedBuildingIds.Contains(buildingId)) return false;
            }

            return true;
        }
    }
}
