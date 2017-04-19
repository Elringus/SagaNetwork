using System;
using System.Collections.Generic;
using System.Reflection;
using SagaNetwork;

namespace ApiGenerator
{
    [Flags]
    public enum PropertyType
    {
        String = 0,
        Int = 1,
        Float = 2,
        Bool = 4,
        TimeSpan = 8,
        DateTime = 16,
        StringMap = 32,
        UModel = 64,
        UStruct = 128,
        Enum = 256,
        Unknown = 512,
    }

    public class UProperty
    {
        public string Name { get; set; } 
        public PropertyType Type { get; set; }
        public bool IsArray { get; set; }
        public bool IsArgument { get; set; }
        public string CustomTypeName { get; set; }

        public string UnrealType => GenerateUnrealType();
        public string UnrealCustomTypeName => GenerateUnrealCustomTypeName();
        public string DeclarationExpression => GenerateDeclarationExpression();
        public string AssignmentExpression => GenerateAssignmentExpression();
        public string JsonSetterExpression => GenerateJsonSetterExpression();

        public UProperty () { }

        public UProperty (PropertyInfo reflectedProperty)
        {
            Name = reflectedProperty.Name;
            IsArray = reflectedProperty.PropertyType.IsArray || reflectedProperty.PropertyType.GetInterface(typeof(IList<>).FullName) != null;

            Type nestedType;
            if (IsArray)
            {
                if (reflectedProperty.PropertyType.IsGenericType)
                    nestedType = reflectedProperty.PropertyType.GetGenericArguments()[0];
                else nestedType = reflectedProperty.PropertyType.GetElementType();
            }
            else nestedType = reflectedProperty.PropertyType;

            Type = EvaluateType(nestedType);
            CustomTypeName = nestedType.Name;
        }

        // Shito podelat'...
        public UProperty (FieldInfo reflectedField, bool isArgument = false)
        {
            Name = reflectedField.Name;
            IsArray = reflectedField.FieldType.IsArray || reflectedField.FieldType.GetInterface(typeof(IList<>).FullName) != null;
            IsArgument = isArgument;

            Type nestedType;
            if (IsArray)
            {
                if (reflectedField.FieldType.IsGenericType)
                    nestedType = reflectedField.FieldType.GetGenericArguments()[0];
                else nestedType = reflectedField.FieldType.GetElementType();
            }
            else nestedType = reflectedField.FieldType;

            Type = EvaluateType(nestedType);
            CustomTypeName = nestedType.Name;
        }

        // In case of arrays reflectedType arg should be type of nested elements.
        private PropertyType EvaluateType (Type reflectedType)
        {
            if (reflectedType.IsGenericType &&
                reflectedType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                reflectedType.GetGenericArguments()[0] == typeof(string) &&
                reflectedType.GetGenericArguments()[1] == typeof(string))
                return PropertyType.StringMap;

            if (reflectedType == typeof(string)) return PropertyType.String;
            if (reflectedType == typeof(int) || reflectedType == typeof(int?)) return PropertyType.Int;
            if (reflectedType == typeof(float) || reflectedType == typeof(float?)) return PropertyType.Float;
            if (reflectedType == typeof(bool) || reflectedType == typeof(bool?)) return PropertyType.Bool;
            if (reflectedType == typeof(TimeSpan) || reflectedType == typeof(TimeSpan?)) return PropertyType.TimeSpan;
            if (reflectedType == typeof(DateTime) || reflectedType == typeof(DateTime?)) return PropertyType.DateTime;
            if (reflectedType.GetCustomAttribute<GenerateApiAttribute>()?.ClassType == UClassType.DbModel ||
                reflectedType.GetCustomAttribute<GenerateApiAttribute>()?.ClassType == UClassType.MetaModel ||
                reflectedType.GetCustomAttribute<GenerateApiAttribute>()?.ClassType == UClassType.MetaDescribedModel) return PropertyType.UModel;
            if (reflectedType.GetCustomAttribute<GenerateApiAttribute>()?.ClassType == UClassType.Struct) return PropertyType.UStruct;
            if (reflectedType.GetCustomAttribute<GenerateApiAttribute>()?.ClassType == UClassType.Enum) return PropertyType.Enum;

            return PropertyType.Unknown;
        }

        private string GenerateUnrealType ()
        {
            string expression = string.Empty;

            if (IsArray) expression += @"TArray<";

            switch (Type)
            {
                case PropertyType.String:
                    expression += "FString";
                    break;
                case PropertyType.Int:
                    expression += "int32";
                    break;
                case PropertyType.Float:
                    expression += "float";
                    break;
                case PropertyType.Bool:
                    expression += "bool";
                    break;
                case PropertyType.TimeSpan:
                    expression += "FTimespan";
                    break;
                case PropertyType.DateTime:
                    expression += "FDateTime";
                    break;
                case PropertyType.StringMap:
                    expression += "UStringMap*";
                    break;
                case PropertyType.UModel:
                    expression += "UDb" + CustomTypeName + '*';
                    break;
                case PropertyType.UStruct:
                    expression += 'F' + CustomTypeName;
                    break;
                case PropertyType.Enum:
                    expression += 'E' + CustomTypeName;
                    break;
                default:
                    return "UnknownType";
            }

            if (IsArray) expression += ">";

            return expression;
        }

        private string GenerateUnrealCustomTypeName ()
        {
            switch (Type)
            {
                case PropertyType.UModel:
                    return "UDb" + CustomTypeName;
                case PropertyType.UStruct:
                    return 'F' + CustomTypeName;
                default:
                    return "UnknownCustomType";
            }
        }

        private string GenerateDeclarationExpression ()
        {
            return $@"UPROPERTY(EditAnywhere, BlueprintReadWrite) {UnrealType} {Name};";
        }

        private string GenerateAssignmentExpression ()
        {
            string expression = string.Empty;

            if (IsArray)
            {
                expression += $@"if (jsonObj->HasTypedField<EJson::Array>(""{Name}""))
            for (auto value : jsonObj->GetArrayField(""{Name}""))
                ";

                switch (Type)
                {
                    case PropertyType.String:
                        expression += $@"{Name}.Add((FString)value->AsString());";
                        break;
                    case PropertyType.Int:
                        expression += $@"{Name}.Add((int32)value->AsNumber());";
                        break;
                    case PropertyType.Float:
                        expression += $@"{Name}.Add((float)value->AsNumber());";
                        break;
                    case PropertyType.Bool:
                        expression += $@"{Name}.Add(value->AsBool());";
                        break;
                    case PropertyType.Enum:
                        expression += $@"{Name}.Add(StringToEnumValue<{UnrealType}>((FString)value->AsString()));";
                        break;
                    case PropertyType.TimeSpan:
                        expression += $@"{Name}.Add(StringToTimeSpan((FString)value->AsString()));";
                        break;
                    case PropertyType.DateTime:
                        expression += $@"{Name}.Add(StringToDatetime((FString)value->AsString()));";
                        break;
                    case PropertyType.StringMap:
                        return "StringMap arrays are not yet implemented.";
                    case PropertyType.UModel:
                        expression += $@"{Name}.Add({UnrealCustomTypeName}::ConstructFromJson<{UnrealCustomTypeName}>(value->AsObject().ToSharedRef()));";
                        break;
                    case PropertyType.UStruct:
                        expression += $@"{Name}.Add({UnrealCustomTypeName}(value->AsObject().ToSharedRef()));";
                        break;
                    default:
                        return "UnknownType";
                }
            }
            else
            {
                switch (Type)
                {
                    case PropertyType.String:
                        expression += $@"{Name} = GetJsonStringField(jsonObj, ""{Name}"");";
                        break;
                    case PropertyType.Int:
                        expression += $@"{Name} = GetJsonIntField(jsonObj, ""{Name}"");";
                        break;
                    case PropertyType.Float:
                        expression += $@"{Name} = GetJsonFloatField(jsonObj, ""{Name}"");";
                        break;
                    case PropertyType.Bool:
                        expression += $@"{Name} = GetJsonBoolField(jsonObj, ""{Name}"");";
                        break;
                    case PropertyType.Enum:
                        expression += $@"{Name} = GetJsonEnumField<{UnrealType}>(jsonObj, ""{Name}"");";
                        break;
                    case PropertyType.TimeSpan:
                        expression += $@"{Name} = GetJsonTimeSpanField(jsonObj, ""{Name}"");";
                        break;
                    case PropertyType.DateTime:
                        expression += $@"{Name} = GetJsonDateTimeField(jsonObj, ""{Name}"");";
                        break;
                    case PropertyType.StringMap:
                        expression += $@"if (jsonObj->HasTypedField<EJson::Object>(""{Name}""))
        {{
            {Name} = NewObject<UStringMap>();
            {Name}->FromJson(jsonObj->GetObjectField(""{Name}"").ToSharedRef());
        }}";
                        break;
                    case PropertyType.UModel:
                        expression += $@"if (jsonObj->HasTypedField<EJson::Object>(""{Name}""))
            {Name} = {UnrealCustomTypeName}::ConstructFromJson<{UnrealCustomTypeName}>(jsonObj->GetObjectField(""{Name}"").ToSharedRef());";
                        break;
                    case PropertyType.UStruct:
                        expression += $@"if (jsonObj->HasTypedField<EJson::Object>(""{Name}""))
            {Name} = {UnrealCustomTypeName}(jsonObj->GetObjectField(""{Name}"").ToSharedRef());";
                        break;
                    default:
                        return "UnknownType";
                }
            }

            return expression;
        }

        private string GenerateJsonSetterExpression ()
        {
            string expression = string.Empty;
            var varName = IsArgument ? Name.FirstToLower() : Name;

            if (IsArray)
            {
                var jsonArrayName = char.ToLowerInvariant(Name[0]) + Name.Substring(1) + "JsonArray";
                var arrayItemName = "itemOf" + Name;

                expression += $@"TArray<TSharedPtr<FJsonValue, ESPMode::NotThreadSafe>> {jsonArrayName};
        for (auto {arrayItemName} : {varName})
            ";
                switch (Type)
                {
                    case PropertyType.String:
                        expression += $@"{jsonArrayName}.Add(MakeShareable(new FJsonValueString({arrayItemName})));";
                        break;
                    case PropertyType.Int:
                        expression += $@"{jsonArrayName}.Add(MakeShareable(new FJsonValueNumber((double){arrayItemName})));";
                        break;
                    case PropertyType.Float:
                        expression += $@"{jsonArrayName}.Add(MakeShareable(new FJsonValueNumber((double){arrayItemName})));";
                        break;
                    case PropertyType.Bool:
                        expression += $@"{jsonArrayName}.Add(MakeShareable(new FJsonValueBool({arrayItemName})));";
                        break;
                    case PropertyType.Enum:
                        expression += $@"{jsonArrayName}.Add(MakeShareable(new FJsonValueString(EnumValueToString<{UnrealType}>({arrayItemName}))));";
                        break;
                    case PropertyType.TimeSpan:
                        expression += $@"{jsonArrayName}.Add(MakeShareable(new FJsonValueString({arrayItemName}.ToString())));";
                        break;
                    case PropertyType.DateTime:
                        expression += $@"{jsonArrayName}.Add(MakeShareable(new FJsonValueString({arrayItemName}.ToIso8601())));";
                        break;
                    case PropertyType.StringMap:
                        return "StringMap arrays are not yet implemented.";
                    case PropertyType.UModel:
                        expression += $@"{jsonArrayName}.Add(MakeShareable(new FJsonValueObject({arrayItemName}->ToJson())));";
                        break;
                    case PropertyType.UStruct:
                        expression += $@"{jsonArrayName}.Add(MakeShareable(new FJsonValueObject({arrayItemName}.ToJson())));";
                        break;
                    default:
                        return "UnknownType";
                }

                expression += $@"
        jsonObj->SetArrayField(""{Name}"", {jsonArrayName});";
            }
            else
            {
                switch (Type)
                {
                    case PropertyType.String:
                        expression += $@"jsonObj->SetStringField(""{Name}"", {varName});";
                        break;
                    case PropertyType.Int:
                        expression += $@"jsonObj->SetNumberField(""{Name}"", (double){varName});";
                        break;
                    case PropertyType.Float:
                        expression += $@"jsonObj->SetNumberField(""{Name}"", (double){varName});";
                        break;
                    case PropertyType.Bool:
                        expression += $@"jsonObj->SetBoolField(""{Name}"", {varName});";
                        break;
                    case PropertyType.Enum:
                        expression += $@"jsonObj->SetStringField(""{Name}"", EnumValueToString<{UnrealType}>({varName}));";
                        break;
                    case PropertyType.TimeSpan:
                        expression += $@"jsonObj->SetStringField(""{Name}"", {varName}.ToString());";
                        break;
                    case PropertyType.DateTime:
                        expression += $@"jsonObj->SetStringField(""{Name}"", {varName}.ToIso8601());";
                        break;
                    case PropertyType.StringMap:
                    case PropertyType.UModel:
                        expression += $@"if ({Name}) jsonObj->SetObjectField(""{Name}"", {varName}->ToJson());";
                        break;
                    case PropertyType.UStruct:
                        expression += $@"jsonObj->SetObjectField(""{Name}"", {varName}.ToJson());";
                        break;
                    default:
                        return "UnknownType";
                }
            }

            return expression;
        }
    }
}
