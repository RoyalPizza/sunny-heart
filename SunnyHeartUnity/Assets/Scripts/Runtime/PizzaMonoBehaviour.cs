using UnityEngine;

namespace Pizza.Runtime
{
    /// <summary>
    /// a wrapper for mono behaviour
    /// </summary>
    public abstract class PizzaMonoBehaviour : MonoBehaviour
    {
        public void SetFilthy()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

}
