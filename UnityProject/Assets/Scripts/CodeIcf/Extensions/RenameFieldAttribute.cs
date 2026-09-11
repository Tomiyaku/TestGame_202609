using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeIcf.Extensions
{
    /// <summary>
    /// プロパティのラベルを指定の文字列に変換してInspectrorで表示するAttribute
    /// </summary>
    class RenameFieldAttribute : PropertyAttribute
    {
        public string Name { get; }

        public RenameFieldAttribute( string name ) => Name = name;

#if UNITY_EDITOR
        [CustomPropertyDrawer( typeof( RenameFieldAttribute ) )]
        class FieldNameDrawer : PropertyDrawer
        {
            public override void OnGUI( Rect _position, SerializedProperty _property, GUIContent _label )
            {
                string[] path = _property.propertyPath.Split( '.' );
                bool isArray = path.Length > 1 && path[ 1 ] == "Array";

                if( !isArray && attribute is RenameFieldAttribute fieldName ) _label.text = fieldName.Name;

                EditorGUI.PropertyField( _position, _property, _label, true );
            }
        }
#endif
    }
}