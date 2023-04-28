using System;
using System.Collections;
using System.Resources;
using System.Windows;

namespace Foxconn.UI
{
    public static class Localization
    {
        public static object CreateLocalizationResourceKey(object key) => key != null ? (object)new LocalizationResourceKey(key) : throw new ArgumentNullException(nameof(key));

        public static object GetInternalKey(object key) => key is LocalizationResourceKey localizationResourceKey ? localizationResourceKey.InternalKey : throw new ArgumentException("Not type of LocalizationResourceKey.", nameof(key));

        public static void MergeResource(this ResourceDictionary dictionary, object key, object value) => dictionary[CreateLocalizationResourceKey(key)] = value;

        public static void MergeResources(this ResourceDictionary dictionary, IResourceReader reader)
        {
            foreach (DictionaryEntry dictionaryEntry in reader)
                dictionary.MergeResource(dictionaryEntry.Key, dictionaryEntry.Value);
        }

        public static void MergeResources(this ResourceDictionary dictionary, ResourceSet resourceSet)
        {
            foreach (DictionaryEntry resource in resourceSet)
                dictionary.MergeResource(resource.Key, resource.Value);
        }
    }
}
