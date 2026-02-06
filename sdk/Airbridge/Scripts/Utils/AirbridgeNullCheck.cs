using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AirbridgeTests")]

internal static class AirbridgeNullCheck
{
    // ReSharper disable once MemberCanBePrivate.Global
    internal class NullException : Exception
    {
        public NullException() : base()
        {
        }

        public NullException(string message) : base(message)
        {
        }
    }

    public static T RequireNonNull<T>(T value)
    {
        if (value == null)
        {
            throw new NullException("arg cannot be null");
        }

        return value;
    }

    public static void CallMethodWithNullCheck(Action action)
    {
        try
        {
            action();
        }
        catch (NullException)
        {
            /* ignored */
        }
    }

    public static T CallMethodWithNullCheck<T>(Func<T> func, T defaultValue)
    {
        try
        {
            return func();
        }
        catch (NullException)
        {
            /* ignored */
        }

        return defaultValue;
    }
}