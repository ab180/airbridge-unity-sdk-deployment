#if UNITY_EDITOR

using System;
using System.Linq;

internal class AirbridgeVersion : IComparable<AirbridgeVersion>
{
    private readonly int _major;
    private readonly int _minor;
    private readonly int _patch;

    private int[] Parts => new[] { _major, _minor, _patch };

    public AirbridgeVersion(int major, int minor, int patch)
    {
        _major = major;
        _minor = minor;
        _patch = patch;
    }

    public int CompareTo(AirbridgeVersion other)
    {
        if (other == null)
        {
            // Return 1 if parsing fails,
            // indicating a higher version.
            return 1;
        }

        return Parts
            .Zip(other.Parts, (currentPart, otherPart) => currentPart.CompareTo(otherPart))
            .FirstOrDefault(result => result != 0);
    }

    public override string ToString()
    {
        return $"{_major}.{_minor}.{_patch}";
    }
}

internal static class AirbridgeVersionExtension
{
    public static AirbridgeVersion ToAirbridgeVersion(this string version)
    {
        try
        {
            var parts = version.Split('.').Select(int.Parse).ToArray();
            if (parts.Length >= 3)
            {
                return new AirbridgeVersion(parts[0], parts[1], parts[2]);
            }
        }
        catch
        {
            /* ignored */
        }

        return null;
    }
}

#endif