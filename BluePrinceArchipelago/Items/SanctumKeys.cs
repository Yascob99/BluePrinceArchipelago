using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    //TODO Later for a later goal. The locations they are found at is different from where they can be used. Should not persist across days
    public class SanctumKeys(string name, GameObject gameObject, int count = 0) : GroupedItems(name, gameObject, false, 1, true)
    {

    }
}
