using gui_generator.multimappers.common_tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace gui_generator.comparison
{
    public static class ObjectComparer
    {
        public static bool TreatNullAndEmptyAsEqual = false; // Default behavior doesn't treat them as equal

        public static bool AreEquivalent(object obj1, object obj2)
        {
            StringBuilder path = new StringBuilder();
            CompareObjects(obj1, obj2, path);
            return true; // If it reaches here, objects are considered equivalent or an exception would have been thrown.
        }

        private static void CompareObjects(object obj1, object obj2, StringBuilder path)
        {
            if (obj1 == null || obj2 == null)
            {
                if (obj1 != obj2)
                    throw new FieldValueMismatchException(path.ToString(), obj1, obj2);
                return;
            }

            if (!AreObjectsOfSameType(obj1, obj2))
                throw new InvalidOperationException($"Type mismatch at {path}. Type 1: {obj1.GetType().FullName}, Type 2: {obj2.GetType().FullName}.");

            var type = obj1.GetType();
            var constructor = ConstructorParamGetter.GetConstructor(type);
            if (constructor == null)
                throw new InvalidOperationException($"No suitable constructor found for type {type.FullName} at {path}.");

            if (path.Length > 0)
                path.Append(" -> ");
            path.Append(type.Name);

            var parameters = constructor.GetParameters();
            foreach (var param in parameters)
            {
                var field = FindMatchingField(type, param);
                if (field == null) continue;

                var currentPath = new StringBuilder(path.ToString()).Append($".{field.FieldType.Name} {field.Name}");
                if (!AreFieldValuesEquivalent(field, obj1, obj2, currentPath))
                    throw new FieldValueMismatchException(currentPath.ToString(), field.GetValue(obj1), field.GetValue(obj2));
            }
        }

        private static bool AreObjectsOfSameType(object obj1, object obj2) => obj1.GetType() == obj2.GetType();

        private static FieldInfo FindMatchingField(Type type, ParameterInfo parameter)
        {
            return type.GetField(parameter.Name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private static bool AreFieldValuesEquivalent(FieldInfo field, object obj1, object obj2, StringBuilder path)
        {
            var value1 = field.GetValue(obj1);
            var value2 = field.GetValue(obj2);
            if (!AreValuesEquivalent(value1, value2, field.FieldType, path))
                throw new FieldValueMismatchException(path.ToString(), value1, value2);

            return true;
        }

        private static bool AreValuesEquivalent(object value1, object value2, Type fieldType, StringBuilder path)
        {

            if (TreatNullAndEmptyAsEqual && fieldType == typeof(string))
            {
                string str1 = value1 as string;
                string str2 = value2 as string;
                if (string.IsNullOrEmpty(str1) && string.IsNullOrEmpty(str2))
                    return true;
            }


            if (fieldType.IsPrimitive || fieldType == typeof(string) || value1 is IComparable)
            {
                if (!Equals(value1, value2))
                    throw new FieldValueMismatchException(path.ToString(), value1, value2);
            }
            else if (typeof(IDictionary).IsAssignableFrom(fieldType))
            {
                CompareDictionaries(value1 as IDictionary, value2 as IDictionary, path);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(fieldType) && !(value1 is string))
            {
                CompareEnumerables(value1 as IEnumerable, value2 as IEnumerable, path);
            }
            else
            {
                CompareObjects(value1, value2, path); // Recursive call for complex types
            }
            return true;
        }

        private static void CompareEnumerables(IEnumerable enum1, IEnumerable enum2, StringBuilder path)
        {
            // Check for null or empty collections being treated as equal
            bool enum1IsEmpty = enum1 == null || !enum1.Cast<object>().Any();
            bool enum2IsEmpty = enum2 == null || !enum2.Cast<object>().Any();

            if (TreatNullAndEmptyAsEqual && enum1IsEmpty && enum2IsEmpty)
            {
                return; // Consider them equivalent
            }

            var enum1List = enum1?.Cast<object>().ToList() ?? new List<object>();
            var enum2List = enum2?.Cast<object>().ToList() ?? new List<object>();

            if (enum1List.Count != enum2List.Count)
                throw new FieldValueMismatchException(path.ToString(), $"Count {enum1List.Count}", $"Count {enum2List.Count}");

            for (int i = 0; i < enum1List.Count; i++)
            {
                var newPath = new StringBuilder(path.ToString()).Append($"[Index {i}]");
                if (!AreValuesEquivalent(enum1List[i], enum2List[i], enum1List[i]?.GetType() ?? typeof(object), newPath))
                    throw new FieldValueMismatchException(newPath.ToString(), enum1List[i], enum2List[i]);
            }
        }


        private static void CompareDictionaries(IDictionary dict1, IDictionary dict2, StringBuilder path)
        {
            // Check for null or empty dictionaries being treated as equal
            bool dict1IsEmpty = dict1 == null || dict1.Count == 0;
            bool dict2IsEmpty = dict2 == null || dict2.Count == 0;

            if (TreatNullAndEmptyAsEqual && dict1IsEmpty && dict2IsEmpty)
            {
                return; // Consider them equivalent
            }

            if (dict1 == null || dict2 == null || dict1.Count != dict2.Count)
                throw new FieldValueMismatchException(path.ToString(), $"Dictionary Count {dict1?.Count ?? 0}", $"Dictionary Count {dict2?.Count ?? 0}");

            foreach (var key in dict1.Keys)
            {
                if (!dict2.Contains(key))
                    throw new FieldValueMismatchException(path.ToString(), "Key exists in first dictionary", "Key does not exist in second dictionary");

                var value1 = dict1[key];
                var value2 = dict2[key];
                var newPath = new StringBuilder(path.ToString()).Append($".{key}");
                if (!AreValuesEquivalent(value1, value2, value1?.GetType() ?? typeof(object), newPath))
                    throw new FieldValueMismatchException(newPath.ToString(), value1, value2);
            }
        }


    }

    public class FieldValueMismatchException : Exception
    {
        public FieldValueMismatchException(string path, object value1, object value2)
            : base($"Mismatch found at '{path}': Value 1: {value1}, Value 2: {value2}")
        {
        }
    }




}
