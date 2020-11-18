using UnityEngine;

namespace Models.Project
{
    /// <summary>
    ///     This data structure holds the ItemInfo instance and is
    ///     attached to the GameObject/ListView instance.
    ///     Using Originator pattern because classes which
    ///     inherit from MonoBehaviour cannot be serialized.
    /// </summary>
    public class ItemInfoController : MonoBehaviour
    {
        public ItemInfo ItemInfo { get; set; }
    }
}