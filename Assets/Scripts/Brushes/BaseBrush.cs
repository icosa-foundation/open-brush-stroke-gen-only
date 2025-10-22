using Object = UnityEngine.Object;
using UnityEngine;

namespace TiltBrush
{
    /// <summary>
    /// Non-MonoBehaviour helpers for brush functionality.
    /// </summary>
    public static class BaseBrush
    {
        /// <summary>
        /// Clone the given brush GameObject for use in undo operations.
        /// The clone is parented like the source and activated.
        /// A callback allows callers to perform additional initialization.
        /// </summary>
        public static GameObject CloneAsUndoObject(GameObject source, System.Action<GameObject> initUndoClone)
        {
            GameObject clone = Object.Instantiate(source);
            clone.name = "Undo " + clone.name;
            clone.transform.parent = source.transform.parent;
            Coords.AsLocal[clone.transform] = Coords.AsLocal[source.transform];
            clone.SetActive(true);
            Object.Destroy(clone.GetComponent<BaseBrushScript>());
            initUndoClone?.Invoke(clone);
            return clone;
        }
    }
}
