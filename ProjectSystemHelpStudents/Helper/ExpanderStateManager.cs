using System.Collections.Generic;
using System.Windows.Controls;

public static class ExpanderStateManager
{
    private static Dictionary<string, bool> expanderStates = new Dictionary<string, bool>();

    public static void SaveState(string key, bool isExpanded)
    {
        expanderStates[key] = isExpanded;
    }

    public static bool GetState(string key, bool defaultValue = false)
    {
        return expanderStates.TryGetValue(key, out bool value) ? value : defaultValue;
    }
}
