using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Globalization;
using Archipelago.MultiClient.Net.Enums;
using System.IO;

namespace BluePrinceArchipelago.Utils
{
    public static class ObjectExtensions
    {
        public static T ToObject<T>(this IDictionary<string, object> source)
            where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (var item in source)
            {
                someObjectType
                         .GetProperty(item.Key)
                         .SetValue(someObject, item.Value, null);
            }

            return someObject;
        }

        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }
    }
    public static class AssetExtensions {

        //Fix for older versions of Bepinex where this version of the function isn't available.
        public static T LoadAsset<T>(this AssetBundle bundle, string assetPath) where T : Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
        { 
            return bundle.LoadAsset(assetPath).TryCast<T>();
        }

        public static AssetBundle LoadAssetFile(string filePath)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (FileStream fs = File.OpenRead(filePath))
                {
                    fs.CopyTo(ms);
                }
                Il2CppSystem.IO.MemoryStream memoryStream = new Il2CppSystem.IO.MemoryStream(ms.ToArray());

                AssetBundle loadFromMemoryInternal = AssetBundle.LoadFromStream(memoryStream);
                return loadFromMemoryInternal;
            }

        }
    }
    public static class TransformExtensions
    {
        public static Transform FindRecursive(this Transform transform, string name)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(transform);
            while (queue.Count > 0)
            {
                Transform current = queue.Dequeue();
                if (current.name == name && current != transform)
                    return current;

                for (int i = 0; i < current.childCount; i++)
                    queue.Enqueue(current.GetChild(i));
            }
            return null;
        }
    }
    public static class StringExtensions {

        public static string ToTitleCase(this string str) {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }
    }

    public static class EnumExtensions
    {
        public static string ItemFlagDescription(this ItemFlags flag)
        {
            if (flag == ItemFlags.None)
                return "Filler";
            
            List<string> description = [];

            if (flag.HasFlag(ItemFlags.Advancement))
                description.Add("Progression");
            
            if (flag.HasFlag(ItemFlags.NeverExclude))
                description.Add("Helpful");

            if (flag.HasFlag(ItemFlags.Trap))
                description.Add("Trap");

            return string.Join(" ", description);
        }
    }
}
