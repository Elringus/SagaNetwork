using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

public static class ReflectionUtils
{
    /// <summary>
    /// Finds proprety over provided path and retrieves its value.
    /// Path should be seperated by dots; indexes supported in the following format: [index].
    /// Eg: Player.Characters[2].Id 
    /// </summary>
    public static object GetPropertyValueByPath (object @object, string path)
    {
        if (@object == null) throw new ArgumentNullException("value");
        if (path == null) throw new ArgumentNullException("path");

        var currentType = @object.GetType();

        object objCopy = @object;
        foreach (var propertyName in path.Split('.'))
        {
            if (currentType != null)
            {
                PropertyInfo property = null;
                var bracketStartIndex = propertyName.IndexOf("[");
                var bracketEndIndex = propertyName.IndexOf("]");

                property = currentType.GetProperty(bracketStartIndex > 0 ? propertyName.Substring(0, bracketStartIndex) : propertyName);
                objCopy = property.GetValue(objCopy, null);

                if (bracketStartIndex > 0)
                {
                    var index = propertyName.Substring(bracketStartIndex + 1, bracketEndIndex - bracketStartIndex - 1);
                    foreach (var interfaceType in objCopy.GetType().GetInterfaces())
                    {
                        if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                        {
                            objCopy = typeof(ReflectionUtils).GetMethod("GetDictionaryElement")
                                                 .MakeGenericMethod(interfaceType.GetGenericArguments())
                                                 .Invoke(null, new object[] { objCopy, index });
                            break;
                        }
                        if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                        {
                            objCopy = typeof(ReflectionUtils).GetMethod("GetListElement")
                                                 .MakeGenericMethod(interfaceType.GetGenericArguments())
                                                 .Invoke(null, new object[] { objCopy, index });
                            break;
                        }
                    }
                }

                currentType = objCopy?.GetType();
            }
            else return null;
        }
        return objCopy;
    }

    public static PropertyInfo GetPropertyInfoByPath (object @object, string path)
    {
        if (@object == null) throw new ArgumentNullException("value");
        if (path == null) throw new ArgumentNullException("path");

        var currentType = @object.GetType();

        object objCopy = @object;
        PropertyInfo property = null;
        foreach (var propertyName in path.Split('.'))
        {
            if (currentType != null)
            {
                var bracketStartIndex = propertyName.IndexOf("[");
                var bracketEndIndex = propertyName.IndexOf("]");

                property = currentType.GetProperty(bracketStartIndex > 0 ? propertyName.Substring(0, bracketStartIndex) : propertyName);
                objCopy = property.GetValue(objCopy, null);

                if (bracketStartIndex > 0)
                {
                    var index = propertyName.Substring(bracketStartIndex + 1, bracketEndIndex - bracketStartIndex - 1);
                    foreach (var interfaceType in objCopy.GetType().GetInterfaces())
                    {
                        if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                        {
                            objCopy = typeof(ReflectionUtils).GetMethod("GetDictionaryElement")
                                                 .MakeGenericMethod(interfaceType.GetGenericArguments())
                                                 .Invoke(null, new object[] { objCopy, index });
                            break;
                        }
                        if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                        {
                            objCopy = typeof(ReflectionUtils).GetMethod("GetListElement")
                                                 .MakeGenericMethod(interfaceType.GetGenericArguments())
                                                 .Invoke(null, new object[] { objCopy, index });
                            break;
                        }
                    }
                }

                currentType = objCopy?.GetType();
            }
            else return null;
        }
        return property;
    }
    
    public static bool SetPropertyByPath (object @object, string path, object value)
    {
        var propertyInfo = GetPropertyInfoByPath(@object, path);
        var propertyHost = GetPropertyHostByPath(@object, path);

        var valueType = propertyInfo.PropertyType.IsGenericType ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType;
        var converter = TypeDescriptor.GetConverter(valueType);
        var typedValue = converter.CanConvertFrom(value.GetType()) ? converter.ConvertFrom(value) : null;
        if (typedValue == null) return false;

        if (propertyInfo.IsGenericList())
        {
            var listObject = (IList)propertyInfo.GetValue(propertyHost);
            var index = GetLastIndexInPath(path);
            FillListToIndex(ref listObject, valueType, index);
            listObject[index] = typedValue; 
        }
        else if (propertyInfo.IsGenericDict())
        {
            var dictObject = (IDictionary)propertyInfo.GetValue(propertyHost);
            var key = GetLastKeyInPath(path);
            dictObject[key] = typedValue;
        }
        else propertyInfo.SetValue(propertyHost, typedValue);

        return true;
    }

    public static object GetPropertyHostByPath (object @object, string path)
    {
        if (path.LastIndexOf('.') < 0) return @object;

        var hostPath = path.Substring(0, path.LastIndexOf('.'));
        return GetPropertyValueByPath(@object, hostPath);
    }

    public static TValue GetDictionaryElement<TKey, TValue> (IDictionary<TKey, TValue> dict, object index)
    {
        TKey key = (TKey)Convert.ChangeType(index, typeof(TKey), null);
        if (!dict.ContainsKey(key))
        {
            var defaultDictValue = ConstructDefault<TValue>();
            dict.Add(key, defaultDictValue);
        }
        return dict[key];
    }

    public static T GetListElement<T> (IList<T> list, object index)
    {
        var intIndex = Convert.ToInt32(index);
        FillListToIndex(ref list, intIndex);

        return list[intIndex];
    }

    public static object ConstructDefault (Type objectType)
    {
        var defaultValue = objectType == typeof(string) ? "" : Activator.CreateInstance(objectType);

        return defaultValue;
    }

    public static T ConstructDefault<T> ()
    {
        return (T)ConstructDefault(typeof(T));
    }

    public static bool IsGenericList (this PropertyInfo propertyInfo)
    {
        return propertyInfo.PropertyType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
    }

    public static bool IsGenericDict (this PropertyInfo propertyInfo)
    {
        return propertyInfo.PropertyType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }

    private static int GetLastIndexInPath (string path)
    {
        var bracketStartIndex = path.LastIndexOf("[");
        var bracketEndIndex = path.LastIndexOf("]");
        var index = path.Substring(bracketStartIndex + 1, bracketEndIndex - bracketStartIndex - 1);
        return int.Parse(index);
    }

    private static string GetLastKeyInPath (string path)
    {
        var bracketStartIndex = path.LastIndexOf("[");
        var bracketEndIndex = path.LastIndexOf("]");
        var key = path.Substring(bracketStartIndex + 1, bracketEndIndex - bracketStartIndex - 1);
        return key;
    }

    private static void FillListToIndex (ref IList list, Type listGenericType, int index)
    {
        var defaultListValue = ConstructDefault(listGenericType);
        while (list.Count <= index)
            list.Add(defaultListValue);
    }

    private static void FillListToIndex<T> (ref IList<T> list, int index)
    {
        var defaultListValue = ConstructDefault<T>();
        while (list.Count <= index)
            list.Add(defaultListValue);
    }
}
