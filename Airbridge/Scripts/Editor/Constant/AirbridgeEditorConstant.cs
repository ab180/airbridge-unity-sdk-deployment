#if UNITY_EDITOR

internal struct AirbridgeEditorConstant
{
    internal struct CustomDomain
    {
        internal const string CustomDomainLabel = "Custom Domain";
        internal const string CustomDomainElementLabel = "URL";
        internal const char CustomDomainSeparator = ' ';
    }

    internal struct BlockList
    {
        internal const string TrackingBlocklistLabel = "Tracking Blocklist";
        internal const string TrackingBlocklistSeparator = ",";
    }
}

#endif