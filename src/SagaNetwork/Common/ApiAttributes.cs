using System;

namespace SagaNetwork
{
    /// <summary>
    /// Represents type of the UE4 class to generate.
    /// </summary>
    public enum UClassType
    {
        /// <summary>
        /// An independent UE4 struct (FStruct).
        /// Used to represent arbitrary data.
        /// </summary>
        Struct,

        /// <summary>
        /// Class derived from UDbModel. 
        /// Used to represent an independent TableModel.
        /// </summary>
        DbModel,

        /// <summary>
        /// Class derived from UDbMetaModel. 
        /// Used to represent TableModel which describe other model.
        /// </summary>
        MetaModel,

        /// <summary>
        /// Class derived from UDbMetaDescribedModel.
        /// Used to represent models described by MetaModels.
        /// </summary>
        MetaDescribedModel,

        /// <summary>
        /// Class derived from USagaNetworkRequest.
        /// Used to communicate with the decorated controller.
        /// Use ApiRequestData and ApiResponseData attributes on the class fields to route in/out data.
        /// </summary>
        Request,

        /// <summary>
        /// An arbitrary enum.
        /// </summary>
        Enum
    }

    /// <summary>
    /// Tells API generator to emit UE4 client API for the decorated class.
    /// Will mirror all the declared properties with the corresponding UE4 types and
    /// generate specific code (props de-/serialization, request logic, etc), depending on the UClassType.
    /// </summary>
    public class GenerateApiAttribute : Attribute
    {
        public UClassType ClassType { get; }

        public GenerateApiAttribute (UClassType classType)
        {
            ClassType = classType;
        }
    }

    /// <summary>
    /// The controller requires valid PlayerId and SessionToken fields.
    /// </summary>
    public class RequireAuthAttribute : Attribute { }

    /// <summary>
    /// The controller requires valid server auth key stored in SessionToken field.
    /// </summary>
    public class RequireServerAuthAttribute : Attribute { }

    /// <summary>
    /// The data will be provided by the UE4 client as a request argument.
    /// </summary>
    public class ApiRequestDataAttribute : Attribute { }

    /// <summary>
    /// The data should be provided by the controller and will be exposed to the UE4 client on response.
    /// </summary>
    public class ApiResponseDataAttribute : Attribute { }
}
