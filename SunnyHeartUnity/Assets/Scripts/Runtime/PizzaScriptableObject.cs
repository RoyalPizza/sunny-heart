using UnityEngine;

namespace Pizza.Runtime
{
    /// <summary>
    /// a wrapper for scriptable object
    /// </summary>
    public abstract class PizzaScriptableObject : ScriptableObject
    {
        public void SetFilthy()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

}
