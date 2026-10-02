#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;

internal static class AirbridgeListExtension
{
    internal static void ForEachWithIndex<T>(this List<T> self, Action<T, int> action)
    {
        if (self == null) return;
        for (var i = 0; i < self.Count; i++)
        {
            action(self[i], i);
        }
    }

    internal static List<string> MergeTrimmed(this List<string> self, string str, char separator)
    {
        if (self == null) return null;
        return string.IsNullOrWhiteSpace(str) ? self : self.MergeTrimmedInternal(str.Split(separator));
    }

    internal static List<string> MergeTrimmed(this List<string> self, List<string> list)
    {
        if (self == null) return null;
        if (list == null || list.Count == 0) return self;
        return self.MergeTrimmedInternal(list);
    }

    private static List<string> MergeTrimmedInternal(this List<string> self, IEnumerable<string> source)
    {
        self.AddRange(source
            .Where(elem => !string.IsNullOrWhiteSpace(elem))
            .Select(elem => elem.Trim())
        );
        return self;
    }

    internal static List<string> RemoveDuplicates(this List<string> self)
    {
        if (self == null) return null;

        var distinct = self.Distinct().ToList();
        self.Clear();
        self.AddRange(distinct);
        return self;
    }
}

#endif