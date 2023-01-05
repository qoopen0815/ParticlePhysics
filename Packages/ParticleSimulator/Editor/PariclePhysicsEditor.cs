using UnityEngine;
using UnityEditor;

namespace ParticleSimulator
{
    [CustomEditor(typeof(ParticlePhysics))]
    public class PariclePhysicsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ParticlePhysics obj = target as ParticlePhysics;
            //DrawDefaultInspector();

            EditorGUILayout.LabelField("Particle Setting", EditorStyles.boldLabel);
            obj._particleNum = (ParticleNumEnum)EditorGUILayout.EnumPopup("Max Particle Num", obj._particleNum);
            obj._particleNum = (ParticleNumEnum)EditorGUILayout.EnumPopup("Particle Shape", obj._particleNum);
            // Radius
            obj._spornPos = EditorGUILayout.Vector3Field("Sporn Point", obj._spornPos);  //いずれ消す⇒またはデバッグ用コンポーネントを別途用意する

            EditorGUILayout.LabelField("Colision Object", EditorStyles.boldLabel);
            obj._terrain = (Terrain)EditorGUILayout.ObjectField("terrain", obj._terrain, typeof(Terrain), true);
        }
    }
}
