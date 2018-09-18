using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Framework.Domain.Infrastructure.Extensions
{
    public static class EnumExtension
    {
        public const int EmptyItemId = default(int);
        public const string EmptyItemText = "Not selected";
        public static readonly IdValueModel EmptyItem = CreateEmptyItem(EmptyItemId, EmptyItemText);

        private static Dictionary<string, string> _enumDescriptions = new Dictionary<string, string>();
        private static object _enumDescriptionsLock = new object();

        public static string GetDescription(string valueName, Type t)
        {
            var key = $"{t.Name}.{valueName}";

            if (_enumDescriptions.ContainsKey(key))
                return _enumDescriptions[key];

            lock (_enumDescriptionsLock)
            {
                if (_enumDescriptions.ContainsKey(key))
                    return _enumDescriptions[key];

                DescriptionAttribute[] attributes = (DescriptionAttribute[])t.GetField(valueName).GetCustomAttributes(typeof(DescriptionAttribute), false);
                var description = (attributes.Length > 0) ? attributes[0].Description : valueName;
                _enumDescriptions[key] = description;
                return description;
            }
        }

        public static string GetDescription(this Enum enumObj, Type t)
        {
            if (t == null) t = enumObj.GetType();
            string name = Enum.GetName(t, enumObj);
            return (name == null) ? null : GetDescription(name, t);
        }

        public static string GetDescription(this Enum enumObj)
        {
            return GetDescription(enumObj, enumObj.GetType());
        }

        public static List<IdValueModel> GetDictionaryItems<T>(bool addEmptyOption = false)
        {
            return GetDictionaryItems(typeof(T), addEmptyOption);
        }

        public static List<IdValueModel> GetDictionaryItems(Type type, bool addEmptyOption = false)
        {
            var list = Enum.GetValues(type).Cast<object>()
                .Select(value => new IdValueModel((int)value, GetDescription(value.ToString(), type)))
                .ToList();
            if (addEmptyOption)
            {
                list.Insert(0, EmptyItem);
            }
            return list;
        }

        public static List<IdValueModel> GetDictionaryItemsWithoutCasting<T>(bool addEmptyOption = false)
        {
            Type type = typeof(T);
            var list = Enum.GetValues(type).Cast<object>()
                .Select(value => new IdValueModel(value, GetDescription(value.ToString(), type)))
                .ToList();
            if (addEmptyOption)
            {
                list.Insert(0, EmptyItem);
            }
            return list;
        }

        public static IdValueModel CreateEmptyItem(int undefinedId, string undefinedText)
        {
            return new IdValueModel(undefinedId, undefinedText);
        }

        public static IEnumerable<IdValueModel> AddEmptyOption(this IEnumerable<IdValueModel> list)
        {
            return AddEmptyOption(list, EmptyItem);
        }

        public static IEnumerable<IdValueModel> AddEmptyOption(this IEnumerable<IdValueModel> list, IdValueModel undefinedItem)
        {
            yield return undefinedItem;
            foreach (var item in list) yield return item;
        }

        public static IEnumerable<Tuple<object, string>> ToTupleItems<T>()
        {
            Type t = typeof(T);

            return Enum.GetValues(t).Cast<object>().Select(
                enumValue => new Tuple<object, string>(
                    Enum.Parse(t, enumValue.ToString()),
                    GetDescription(enumValue.ToString(), t)
                ));
        }
    }
}