using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using SagaNetwork;
using Microsoft.WindowsAzure.Storage.Table;

namespace ApiGenerator
{
    public class GeneratedUClass : Generated
    {
        static string MODEL_TEMPLATE_NAME => "Model";
        static string STRUCT_TEMPLATE_NAME => "Struct";
        static string REQUEST_TEMPLATE_NAME => "Request";
        static string ENUM_TEMPLATE_NAME => "Enum";

        public override string TemplateName => GetTemplateName();

        public string Name { get; }
        public UClassType Type { get; }
        public List<UProperty> Properties { get; } = new List<UProperty>();
        public List<UProperty> RequestArguments { get; } = new List<UProperty>();
        public List<string> EnumValues { get; } = new List<string>();

        public string UName => GenerateUName();
        public string ParentUName => GenerateParentUName();
        public bool HasParent => !string.IsNullOrEmpty(ParentUName);
        public string RequestArgumentsExpression => GenerateRequestArgumentsExpression();
        public bool HasResponseData => Properties.Count > 0;
        public string ControllerName => Name.Replace("Controller", string.Empty);

        private Type reflectedClassType;

        public GeneratedUClass (Type reflectedClass) 
        {
            reflectedClassType = reflectedClass;

            Name = reflectedClass.Name;
            Type = reflectedClass.GetCustomAttribute<GenerateApiAttribute>().ClassType;

            if (Type == UClassType.Request)
            {
                foreach (var requesDataField in reflectedClass.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                    .Where(property => property.GetCustomAttributes<ApiRequestDataAttribute>(true).Any()))
                    RequestArguments.Add(new UProperty(requesDataField, true));

                if (reflectedClass.GetCustomAttributes<RequireServerAuthAttribute>(true).Any())
                {
                    RequestArguments.Add(new UProperty() { Name = "ServerAuthKey", Type = PropertyType.String, IsArgument = true });
                    if (RequestArguments.Count >= 2)
                        RequestArguments.Swap(0, RequestArguments.Count - 1);
                }
                else if (reflectedClass.GetCustomAttributes<RequireAuthAttribute>(true).Any())
                {
                    if (!RequestArguments.Exists(a => a.Name.Equals("PlayerId", StringComparison.OrdinalIgnoreCase)))
                    {
                        RequestArguments.Add(new UProperty() { Name = "PlayerId", Type = PropertyType.String, IsArgument = true });
                        if (RequestArguments.Count >= 2)
                            RequestArguments.Swap(0, RequestArguments.Count - 1);
                    }
                    RequestArguments.Add(new UProperty() { Name = "SessionToken", Type = PropertyType.String, IsArgument = true });
                    if (RequestArguments.Count >= 3)
                        RequestArguments.Swap(1, RequestArguments.Count - 1);
                }

                foreach (var responseDataField in reflectedClass.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                    .Where(property => property.GetCustomAttributes<ApiResponseDataAttribute>(true).Any()))
                    Properties.Add(new UProperty(responseDataField));
            }
            else if (Type == UClassType.Enum)
            {
                foreach (var enumValue in reflectedClass.GetEnumNames())
                    EnumValues.Add(enumValue);
            }
            else
            {
                foreach (var reflectedProperty in reflectedClass.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                    .Where(property => !property.GetCustomAttributes<IgnorePropertyAttribute>(true).Any() && property.CanWrite))
                    Properties.Add(new UProperty(reflectedProperty));
            }
        }

        public bool DependsOn (GeneratedUClass other) => Properties.Exists(p => p.CustomTypeName.Equals(other.Name)) || ParentUName.Equals(other.UName);

        private string GetTemplateName ()
        {
            switch (Type)
            {
                case UClassType.DbModel:
                case UClassType.MetaModel:
                case UClassType.MetaDescribedModel:
                    return MODEL_TEMPLATE_NAME;
                case UClassType.Request:
                    return REQUEST_TEMPLATE_NAME;
                case UClassType.Struct:
                    return STRUCT_TEMPLATE_NAME;
                case UClassType.Enum:
                    return ENUM_TEMPLATE_NAME;
                default:
                    return "UnknownType";
            }
        }

        private string GenerateUName ()
        {
            switch (Type)
            {
                case UClassType.DbModel:
                case UClassType.MetaModel:
                case UClassType.MetaDescribedModel:
                    return $"UDb{Name}";
                case UClassType.Request:
                    return $"U{Name}".Replace("Controller", "Request");
                case UClassType.Struct:
                    return $"F{Name}";
                case UClassType.Enum:
                    return $"E{Name}";
                default:
                    return "UnknownType";
            }
        }

        private string GenerateParentUName ()
        {
            switch (Type)
            {
                case UClassType.DbModel:
                    return "UDbModel";
                case UClassType.MetaModel:
                    return "UDbMetaModel";
                case UClassType.MetaDescribedModel:
                    return "UDbMetaDescribedModel";
                case UClassType.Request:
                    return "USagaNetworkRequest";
                case UClassType.Struct:
                    if (reflectedClassType.BaseType.GetCustomAttributes<GenerateApiAttribute>(true).Any())
                        return new GeneratedUClass(reflectedClassType.BaseType).UName;
                    else return string.Empty;
                default:
                    return "UnknownType";
            }
        }

        private string GenerateRequestArgumentsExpression ()
        {
            var argumentStrings = new List<string>();

            argumentStrings.Add("UObject* worldContext");

            foreach (var argument in RequestArguments)
            {
                var expression = string.Empty;
                expression += $"const {argument.UnrealType}";
                if (argument.IsArray || (argument.Type == PropertyType.DateTime || 
                                         argument.Type == PropertyType.String ||
                                         argument.Type == PropertyType.StringMap || 
                                         argument.Type == PropertyType.TimeSpan || 
                                         argument.Type == PropertyType.UStruct))
                    expression += "&";
                expression += $" {argument.Name.FirstToLower()}";
                argumentStrings.Add(expression);
            }

            return string.Join(", ", argumentStrings);
        }
    }
}
