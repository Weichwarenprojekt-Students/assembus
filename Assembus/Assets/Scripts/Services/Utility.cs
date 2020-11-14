using UnityEngine;

namespace Services
{
    public static class Utility
    {
        /// <summary>
        ///     Recursively searches for a child by its name
        /// </summary>
        /// <param name="parent">The parent transform (where the search begins) </param>
        /// <param name="name">The name of the searched child</param>
        /// <returns></returns>
        public static Transform FindChild(Transform parent, string name)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                if (name.Equals(parent.GetChild(i).name))
                    return parent.GetChild(i);

                var childTransform = FindChild(parent.GetChild(i), name);
                if (!(childTransform is null))
                    return childTransform;
            }

            return null;
        }
    }
}