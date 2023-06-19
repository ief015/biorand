﻿using System;
using System.Collections.Generic;
using System.Linq;
using IntelOrca.Biohazard.Script;

namespace IntelOrca.Biohazard
{
    public static class Extensions
    {
        public static string ToTitle(this string x)
        {
            var chars = x.ToCharArray();
            var newWord = true;
            for (int i = 0; i < chars.Length; i++)
            {
                if (newWord && char.IsLetter(chars[i]))
                {
                    chars[i] = char.ToUpper(chars[i]);
                    newWord = false;
                }
                else if (!char.IsLetter(chars[i]))
                {
                    newWord = true;
                }
            }
            return new string(chars);
        }

        public static string ToActorString(this string x)
        {
            var actor = x;
            var fsIndex = actor.IndexOf('.');
            if (fsIndex != -1)
            {
                var name = actor.Substring(0, fsIndex).ToTitle();
                var game = actor.Substring(fsIndex + 1).ToUpper();
                actor = $"{name} ({game})";
            }
            else
            {
                actor = actor.ToTitle();
            }
            return actor;
        }

        public static T[] Shuffle<T>(this IEnumerable<T> items, Rng rng)
        {
            var array = items.ToArray();
            for (int i = 0; i < array.Length - 1; i++)
            {
                var ri = rng.Next(i, array.Length);
                var tmp = array[ri];
                array[ri] = array[i];
                array[i] = tmp;
            }
            return array;
        }

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }

        public static IEnumerable<T> UnionExcept<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            return a.Except(b).Union(b.Except(a));
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items)
        {
            return new HashSet<T>(items);
        }

        public static Queue<T> ToQueue<T>(this IEnumerable<T> items)
        {
            return new Queue<T>(items);
        }

        internal static EndlessBag<T> ToEndlessBag<T>(this IEnumerable<T> items, Rng rng)
        {
            return new EndlessBag<T>(rng, items);
        }

        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
        {
            foreach (var item in items)
                set.Add(item);
        }

        public static void RemoveMany<T>(this ICollection<T> items, IEnumerable<T> removeList)
        {
            foreach (var item in removeList)
            {
                items.Remove(item);
            }
        }

        public static IEnumerable<T> EnumerateOpcodes<T>(this Rdt rdt, RandoConfig config) => AstEnumerator<T>.Enumerate(rdt.Ast!, config);
    }
}