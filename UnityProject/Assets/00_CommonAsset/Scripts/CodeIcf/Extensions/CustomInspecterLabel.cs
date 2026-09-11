using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeIcf.Extensions
{
    /// <summary>
    /// Inspectorでの編集表示名変更
    /// </summary>
    public class CustomLabelAttribute : PropertyAttribute
    {
        public readonly string CustomLabel;

        public CustomLabelAttribute( string _label )
        {
            CustomLabel = _label;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof( CustomLabelAttribute ) )]
    public class CustomLabelAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI( Rect _position, SerializedProperty _property, GUIContent _label )
        {
            CustomLabelAttribute labelAttribute = attribute as CustomLabelAttribute;
            EditorGUI.PropertyField( _position, _property, new GUIContent( labelAttribute.CustomLabel ), true );
        }

        public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
        {
            return EditorGUI.GetPropertyHeight( property, true );
        }
    }
#endif
}
