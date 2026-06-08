using Archipelago.MultiClient.Net.Enums;
using StableNameDotNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEngine;

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

        public static string GetAssetPath(this AssetBundle bundle, string name) {
            string[] names = bundle.GetAllAssetNames();
            for (int i = 0; i < names.Length; i++) {
                if (names[i].Contains("/" + name.ToLower())){
                    return names[i];
                }
            }
            return "";
        }
        public static AssetBundle LoadAssetFromAssembly(this AssetBundle assetBundle, string resourceName) {
            using (MemoryStream ms = new MemoryStream())
            {
                Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName).CopyTo(ms);
                Il2CppSystem.IO.MemoryStream memoryStream = new Il2CppSystem.IO.MemoryStream(ms.ToArray());
                AssetBundle bundle = AssetBundle.LoadFromStream(memoryStream);
                return bundle;
            }
        }
        public static string GetResourceNameFromPath(string filePath) { 
            return "BluePrinceArchipelago." + filePath.Replace("\\", "/").Replace("/", ".");
        }

    }
    public static class TransformExtensions
    {
        public static Transform FindRecursive(this Transform transform, string name, bool caseinsensitive = false)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(transform);
            while (queue.Count > 0)
            {
                Transform current = queue.Dequeue();
                if (caseinsensitive)
                {
                    if (current.name.ToLower() == name.ToLower() && current != transform)
                    {
                        return current;
                    }
                }
                else if (current.name == name && current != transform)
                {
                    return current;
                }

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

        // Borrowed from https://stackoverflow.com/questions/6426017/word-wrap-to-x-lines-instead-of-maximum-width-least-raggedness. Divides a series of words into their least ragged form.
        public static string Minragged(this string text, int n = 3)
        {
            var words = text.Split();

            var cumwordwidth = new List<int>();
            cumwordwidth.Add(0);

            foreach (var word in words)
                cumwordwidth.Add(cumwordwidth[cumwordwidth.Count - 1] + word.Length);

            var totalwidth = cumwordwidth[cumwordwidth.Count - 1] + words.Length - 1;

            var linewidth = (double)(totalwidth - (n - 1)) / n;

            var cost = new Func<int, int, double>((i, j) =>
            {
                var actuallinewidth = Math.Max(j - i - 1, 0) + (cumwordwidth[j] - cumwordwidth[i]);
                return (linewidth - actuallinewidth) * (linewidth - actuallinewidth);
            });

            var best = new List<List<Tuple<double, int>>>();

            var tmp = new List<Tuple<double, int>>();
            best.Add(tmp);
            tmp.Add(new Tuple<double, int>(0.0f, -1));
            foreach (var word in words)
                tmp.Add(new Tuple<double, int>(double.MaxValue, -1));

            for (int l = 1; l < n + 1; ++l)
            {
                tmp = new List<Tuple<double, int>>();
                best.Add(tmp);
                for (int j = 0; j < words.Length + 1; ++j)
                {
                    var min = new Tuple<double, int>(best[l - 1][0].Item1 + cost(0, j), 0);
                    for (int k = 0; k < j + 1; ++k)
                    {
                        var loc = best[l - 1][k].Item1 + cost(k, j);
                        if (loc < min.Item1 || (loc == min.Item1 && k < min.Item2))
                            min = new Tuple<double, int>(loc, k);
                    }
                    tmp.Add(min);
                }
            }

            var lines = new List<string>();
            var b = words.Length;

            for (int l = n; l > 0; --l)
            {
                var a = best[l][b].Item2;
                lines.Add(string.Join(" ", words, a, b - a));
                b = a;
            }

            lines.Reverse();
            return lines.Join("\n");
        }
    }
    public static class GameObjectExtensions {

        public static GameObject FindGameObject(string name)
        {
            List<GameObject> gos = new List<GameObject>();
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go != null)
                {
                    
                    if (go?.name?.ToUpper()?.Trim() == name.ToUpper().Trim())
                    {
                        // Make sure the object is not our prefab.
                        string goName = go?.transform?.parent?.name?.ToLower();
                        if (goName != null)
                        {
                            if (goName != "prefabs")
                            {
                                gos.Add(go);
                            }
                        }
                    }
                }

            }
            foreach (GameObject go in gos) {
                if (go.scene.name == null) {    
                    return go;
                }
            }
            if (gos.Count > 0) {
                return gos[0];
            }
            Logging.Log($"Unable to Find GameObject with name: {name}");
            return null;

        }
        public static void DestroyAllChildren(this GameObject go) {
            for (int i = 0; i < go.transform.childCount; i++) { 
                Transform child = go.transform.GetChild(i);
                GameObject.Destroy(child.gameObject);
            }
        }
        public static void MoveChildrenTo(this GameObject from, GameObject to) {
            Transform[] children = new Transform[from.transform.childCount];
            for (int i = 0; i < from.transform.childCount; i++)
            {
                children[i] = from.transform.GetChild(i);
            }
            foreach (Transform child in children) {
                child.parent = to.transform;
            }
        }
        public static GameObject GetChild(this GameObject parent, string name) {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                Transform child = parent.transform.GetChild(i);
                
                if (child.name.ToLower() == name.ToLower()) {
                    return child.gameObject;
                }
            }
            return null;
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
    public static class DictionaryExtensions {

        // Gets the highest key number, returns int.Minvalue if none found.
        public static int HighestKey<Tkey, TValue>(this Dictionary<int, TValue> dictionary) where TValue : notnull
        {
            int highest = int.MinValue;
            if (dictionary.Count > 0) {
                foreach (int key in dictionary.Keys) {
                    if (key > highest) {
                        highest = key;
                    }
                }
            }
            return highest;
        }
    }
}
