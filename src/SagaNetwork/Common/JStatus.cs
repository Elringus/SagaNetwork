using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace SagaNetwork
{
    /// <summary>
    /// Represents status of the controller response, serialized in JToken.
    /// </summary>
    public class JStatus
    {
        public JToken JToken { get; }
        public JStatus (JToken jToken) { JToken = jToken; }
        public T ToObject<T>() { return JToken.ToObject<T>(); }
        public static implicit operator JToken (JStatus jStatus) { return jStatus.JToken; }
        public static JStatus CallerNameToJStatus ([CallerMemberName] string name = null) => new JStatus(JToken.Parse($"{{'Status':'{name}'}}"));

        public static JStatus Ok => CallerNameToJStatus();
        public static JStatus Fail => CallerNameToJStatus();
        public static JStatus Wait => CallerNameToJStatus();
        public static JStatus Updating => CallerNameToJStatus();
        public static JStatus Offline => CallerNameToJStatus();
        public static JStatus ControllerNotFound => CallerNameToJStatus();
        public static JStatus ControllerFail => CallerNameToJStatus();
        public static JStatus OccFail => CallerNameToJStatus();
        public static JStatus WrongQuery => CallerNameToJStatus();
        public static JStatus WrongPassword => CallerNameToJStatus();
        public static JStatus Overcount => CallerNameToJStatus();
        public static JStatus AuthFail => CallerNameToJStatus();
        public static JStatus ServerAuthFail => CallerNameToJStatus();
        public static JStatus NotFound => CallerNameToJStatus();
        public static JStatus MetaNotFound => CallerNameToJStatus();
        public static JStatus RequestedInstanceNotFound => CallerNameToJStatus();
        public static JStatus PlayerNotFound => CallerNameToJStatus();
        public static JStatus PlayerAlreadyExists => CallerNameToJStatus();
        public static JStatus InvalidAccessKey => CallerNameToJStatus();
        public static JStatus NotEnoughKeys => CallerNameToJStatus();
        public static JStatus RequirementNotFulfilled => CallerNameToJStatus();
        public static JStatus BuildingNotFound => CallerNameToJStatus();
        public static JStatus CityNotFound => CallerNameToJStatus();
        public static JStatus WardNotFound => CallerNameToJStatus();
        public static JStatus BuildingSpotNotFound => CallerNameToJStatus();
        public static JStatus BuildingSpotOccupied => CallerNameToJStatus();
        public static JStatus TypeMismatch => CallerNameToJStatus();
        public static JStatus NotReady => CallerNameToJStatus();
        public static JStatus NotEnoughResources => CallerNameToJStatus();
        public static JStatus ContractNotFound => CallerNameToJStatus();
        public static JStatus ItemNotFound => CallerNameToJStatus();
        public static JStatus ItemDuplication => CallerNameToJStatus();
        public static JStatus CharacterNotFound => CallerNameToJStatus();
        public static JStatus ArenaUnavailable => CallerNameToJStatus();
        public static JStatus AlreadyUnlocked => CallerNameToJStatus();
        public static JStatus MaxLevelReached => CallerNameToJStatus();

        public static JStatus RequestArgumentNotFound (string argName)
        {
            var jStatus = CallerNameToJStatus();
            jStatus.JToken["ArgumentName"] = argName;

            return jStatus;
        }
    }
}
