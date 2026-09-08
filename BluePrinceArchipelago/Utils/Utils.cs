using Archipelago.MultiClient.Net.Enums;
using Il2CppInterop.Runtime;
using StableNameDotNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BluePrinceArchipelago.Utils
{
    /// <summary>
    ///     A series of Object extensions utilized all accross the codebase.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        ///     Converts a dictionary to a given object of type T.
        /// </summary>
        /// <typeparam name="T">The type of object to convert to.</typeparam>
        /// <param name="source"></param>
        /// <returns>An object type of type T with properties from the dictionary.</returns>
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

        /// <summary>
        ///     Attempts to convert an object into a dictionary.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="bindingAttr">Which BindingFlags to apply to when getting the properties.</param>
        /// <returns>A generic dictionary of properties from a given object.</returns>
        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }
    }
    /// <summary>
    ///     A series of utility functions for dealing with unity assets.
    /// </summary>
    public static class AssetExtensions {

        /// <summary>
        ///     Gets the full asset paths of an asset with a given name from a given AssetBundle.
        ///     Not case sensitive.
        /// </summary>
        /// <param name="bundle">The AssetBundle to search.</param>
        /// <param name="name">The name of the asset to find. Not case sensitive.</param>
        /// <returns>A list of assets paths of assets that include the given asset name.</returns>
        public static string GetAssetPath(this AssetBundle bundle, string name) {
            string[] names = bundle.GetAllAssetNames();
            for (int i = 0; i < names.Length; i++) {
                if (names[i].ToLower().Contains("/" + name.ToLower())){
                    return names[i];
                }
            }
            return "";
        }
        /// <summary>
        ///     Loads an AssetBundle from a given resource path.
        /// </summary>
        /// <param name="resourceName">The Resource name of the resource to be loaded as an asset.</param>
        /// <returns>An AssetBundle.</returns>
        public static AssetBundle LoadAssetBundleFromAssembly( string resourceName) {
            // Creates a memory stream.
            using (MemoryStream ms = new MemoryStream())
            {
                // Gets the currently executing mod assembly, loads the resource, and copies it to the memory stream.
                Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName).CopyTo(ms);
                // Converts the memory stream into an IL2Cpp memorystream.
                Il2CppSystem.IO.MemoryStream memoryStream = new Il2CppSystem.IO.MemoryStream(ms.ToArray());
                // Loads the asset bundle from the stream (since regular loading is mostly stripped).
                AssetBundle bundle = AssetBundle.LoadFromStream(memoryStream);
                return bundle;
            }
        }
        /// <summary>
        ///     Returns the resource path name of any resources that have been added to the executing assembly.
        /// </summary>
        /// <param name="filePath">The file path of the given resource.</param>
        /// <returns>The resource name for the resource.</returns>
        public static string GetResourceNameFromPath(string filePath) { 
            return "BluePrinceArchipelago." + filePath.Replace("\\", "/").Replace("/", ".");
        }

    }
    /// <summary>
    ///     A series of Transform Extension utilities to assist in common tasks involving Transforms.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        ///     Recursively searches the children of a transform for the first child with a matching name.
        /// </summary>
        /// <param name="transform">The Transform to search.</param>
        /// <param name="name">The name to match.</param>
        /// <param name="caseinsensitive">Whether the search should match case. Defaults to false.</param>
        /// <returns>A Transform with a matching name. Null if not found.</returns>
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
        /// <summary>
        ///     Recursively searches the children of a transform for all children with a matching names.
        /// </summary>
        /// <param name="transform">The Transform to search.</param>
        /// <param name="name">The name to match.</param>
        /// <param name="caseinsensitive">Whether the search should match case. Defaults to false.</param>
        /// <returns>An Array of Transforms that match the given name.</returns>
        public static Transform[] FindAllRecursive(this Transform transform, string name, bool caseinsensitive = false)
        {
            Queue<Transform> queue = new Queue<Transform>();
            List<Transform> transforms = new();
            queue.Enqueue(transform);
            while (queue.Count > 0)
            {
                Transform current = queue.Dequeue();
                if (caseinsensitive)
                {
                    if (current.name.ToLower() == name.ToLower() && current != transform)
                    {
                        transforms.Add(current);
                    }
                }
                else if (current.name == name && current != transform)
                {
                    transforms.Add(current);
                }

                for (int i = 0; i < current.childCount; i++)
                    queue.Enqueue(current.GetChild(i));
            }
            return transforms.ToArray();
        }
    }
    /// <summary>
    ///     A series of utility functions for common operations involving strings.
    /// </summary>
    public static class StringExtensions {

        /// <summary>
        ///     Changes the case of a string to Title Case.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>The String in Title Case.</returns>
        public static string ToTitleCase(this string str) {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        ///     A helper function that attempts divide a given text among any number of lines to be as close to equal width as possible without breaking up words.
        /// </summary>
        /// <param name="text">The text to divide</param>
        /// <param name="n">The number of lines to divide the text among. Defaults to 3.</param>
        /// <returns>The output text divided into n lines.</returns>
        // Borrowed from https://stackoverflow.com/questions/6426017/word-wrap-to-x-lines-instead-of-maximum-width-least-raggedness.
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

    /// <summary>
    ///     A series of utility functions for common operations involving GameObjects.
    /// </summary>
    public static class GameObjectExtensions {

        /// <summary>
        ///     Finds a GameObject with a matching name among all currently loaded resources.
        ///     Is not case sensitive.
        /// </summary>
        /// <param name="name">The name of the object to find.</param>
        /// <returns>The best matching game object prioritizing objects not in the main game scene. Returns null if not found.</returns>
        // Based on a similar algorithm in UnityExplorer.
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
        /// <summary>
        ///     Destroys all children of a given game object.
        /// </summary>
        /// <param name="go">The game object.</param>
        public static void DestroyAllChildren(this GameObject go) {
            for (int i = 0; i < go.transform.childCount; i++) { 
                Transform child = go.transform.GetChild(i);
                GameObject.Destroy(child.gameObject);
            }
        }
        /// <summary>
        ///     Reparents all children of one GameObject to another GameObject
        /// </summary>
        /// <param name="from">The GameObject to reparent children from.</param>
        /// <param name="to">The GameObject to reparent children to.</param>
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
        /// <summary>
        ///     Gets the First child of a Gameobject with the given name.
        /// </summary>
        /// <param name="parent">The parent object to search.</param>
        /// <param name="name">The name of the child object to find.</param>
        /// <returns>The child GameObject. Null if not found.</returns>
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
        /// <summary>
        ///     Gets the current resource path of the invoking GameObject.
        /// </summary>
        /// <param name="current">The GameObject.</param>
        /// <returns>The GameObject path as a string relative to it's scene.</returns>
        public static string GetPath(this GameObject current) => current.transform.GetPath();

        /// <summary>
        ///     Gets the current resource path of the invoking Transform.
        /// </summary>
        /// <param name="current">The Transform.</param>
        /// <returns>The Transformarms path as a string relative to it's scene.</returns>
        public static string GetPath(this Transform current) {
            if (current.parent == null)
                return "/" + current.name;
            return current.parent.GetPath() + "/" + current.name;
        }
    }
    /// <summary>
    ///     A series of utility functions for enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Converts the item flag into a string.
        /// </summary>
        /// <param name="flag">The ItemFlag Enum</param>
        /// <returns>A string generated based on the AP item flags.</returns>
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
        // 

        /// <summary>
        ///     Converts an enum to an Il2Cpp version of that Enum.
        /// </summary>
        /// <typeparam name="TEnum">The Type of the Enum</typeparam>
        /// <param name="value">The string value of the enum.</param>
        /// <returns>The Il2Cpp version of the Enum. Returns null on failure.</returns>
        public static Il2CppSystem.Enum EnumToIl2Cpp<TEnum>(string value) where TEnum : System.Enum
        {
            try
            {
                return Il2CppSystem.Enum.Parse(Il2CppType.Of<TEnum>(), value).TryCast<Il2CppSystem.Enum>();
            }
            catch {
                return null;
            }
        }
    }
    /// <summary>
    ///     A series of Utility functions for Dictionaries.
    /// </summary>
    public static class DictionaryExtensions {

        // , 
        /// <summary>
        ///     Gets the highest key number.
        /// </summary>
        /// <typeparam name="Tkey">The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <param name="dictionary">The Dictionaray to check.</param>
        /// <returns>The highest integer Key. int.MinValue by default.</returns>
        public static int HighestKey<Tkey, TValue>(this Dictionary<int, TValue> dictionary) where TValue : notnull
        {
            int highest = int.MinValue;
            if (typeof(Tkey) == typeof(int))
            {

                if (dictionary.Count > 0)
                {
                    foreach (int key in dictionary.Keys)
                    {
                        if (key > highest)
                        {
                            highest = key;
                        }
                    }
                }
            }
            return highest;
        }
    }
    /// <summary>
    ///     A series of Utility functions for lists.
    /// </summary>
    public static class ListExtensions {

        /// <summary>
        ///     Removes the first instance of a string value from a list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="value">The value to find.</param>
        /// <returns>The index of the removed value or -1 if not found.</returns>
        public static int RemoveFirst(this List<string> list, string value) {
            int index = 0;
            while (index < list.Count)
            {
                if (list[index] == value){
                    list.RemoveAt(index);
                    return index;
                }
                index++;
            }
            return -1;
        }

        /// <summary>
        ///     A Fisher-Yates Shuffle algorithm.
        ///     Taken from https://stackoverflow.com/questions/273313/randomize-a-listt
        /// </summary>
        /// <typeparam name="T">The Type of the list's items.</typeparam>
        /// <param name="list">The List to Shuffle</param>
        /// <param name="rng">The System.Random rng to use.</param>
        public static void Shuffle<T>(this IList<T> list, System.Random rng = null)
        {
            if (rng == null)
            {
                rng = new System.Random();
            }
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        /// <summary>
        ///     Finds a value in the list then moves it to the new index.
        ///     Adds the value to the back of the list if the index is too high.
        /// </summary>
        /// <typeparam name="T">The type of values in the list</typeparam>
        /// <param name="list">The list to modify</param>
        /// <param name="value">The value to find and move.</param>
        /// <param name="i">The position to move it to.</param>
        public static void FindAndInsert<T>(this IList<T> list, T value, int i = 0) {
            int n = list.Count;
            int j = 0;

            // Restrict i to be 0 to n - 1 to make sure it's within bounds.
            i = i < 0 ? 0 : i > n - 1 ? n - 1 : i;
            while (j < n)
            {
                T val = list[j];
                if (val.Equals(value))
                {
                    n = j;
                    list.RemoveAt(j);
                    list.Insert(i, value);
                    return;
                }
                j++;
            }

        }

        /// <summary>
        ///     Finds the provided values then inserts them in the prescribed order if found.
        /// </summary>
        /// <typeparam name="T">The type of values in the list</typeparam>
        /// <param name="list">The list the operation is performed on.</param>
        /// <param name="values">The list of values to find.</param>
        /// <param name="i">The starting index for inserting values at.</param>
        public static void FindAndInsertInOrder<T>(this IList<T> list, T[] values, int i = 0) {
            int n = list.Count;
            int j = 0;
            if (i < 0)
            {
                i = 0;
            }
            int count = 0;
            int removed = 0;

            // Initialize a list with which values were found.
            List<bool> found = new();
            foreach (T value in values) {
                found.Add(false);
            }

            // Find all the values, note which were found and remove it from the list;
            while (j < n - removed && found.Count < values.Length)
            {
                T val = list[j];
                count = 0;
                foreach (T value in values)
                {
                    if (val.Equals(value))
                    {
                        list.RemoveAt(j);
                        removed++;
                        j--; // Remove so the next item isn't skipped.
                        found[count] = true;
                    }
                    count++;
                }
                j++;
            }

            int shift = 0;
            count = 0;
            int insertIndex = 0;

            // Now finally add back in the items in the correct order.
            foreach (bool wasfound in found) {
                if (wasfound) {
                    insertIndex = i + shift;
                    if (insertIndex < n - 1)
                    {
                        list.Insert(insertIndex, values[count]);
                    }
                    else { 
                        list.Add(values[insertIndex]);
                    }
                    shift++;
                }
                count++;
            }
        }
    }
}
