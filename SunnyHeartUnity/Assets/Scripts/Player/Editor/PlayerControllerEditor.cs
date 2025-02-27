using UnityEditor;

namespace Pizza
{
    [CustomEditor(typeof(PlayerController))]
    public class PlayerControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var playerController = (PlayerController)target;
        }
    }
}