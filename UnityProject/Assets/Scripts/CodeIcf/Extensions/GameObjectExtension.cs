using UnityEngine;

namespace CodeIcf.Extensions
{
    /// <summary>
    /// GameObject型拡張クラス
    /// </summary>
    public static class GameObjectExtension
    {
        /// <summary>
        /// オブジェクトのコピー
        /// </summary>
        /// <returns>コピーしたオブジェクトのジェネリックで指定した型</returns>
        /// <param name="_self">コピー元オブジェクト</param>
        /// <param name="_parent">親のTransform</param>
        /// <param name="_name">コピーしたオブジェクトの名前</param>
        /// <typeparam name="T">戻り値で返ってくる型</typeparam>
        public static T CloneObject<T>( this GameObject _self, Transform _parent, string _name = "" ) where T : Component
        {
            string objName = string.IsNullOrEmpty( _name ) ? _self.name : _name;

            return CloneObject<T>( _self, _parent, objName, Vector3.zero, Vector3.one );
        }

        /// <summary>
        /// オブジェクトのコピー
        /// </summary>
        /// <returns>コピーしたオブジェクトのジェネリックで指定した型</returns>
        /// <param name="_self">コピー元オブジェクト</param>
        /// <param name="_parent">親のTransform</param>
        /// <param name="_name">コピーしたオブジェクトの名前</param>
        /// <param name="_localpos">コピーしたオブジェクトのlocalPosition</param>
        /// <param name="_scale">コピーしたオブジェクトのlocalScale</param>
        /// <typeparam name="T">戻り値の型</typeparam>
        public static T CloneObject<T>( this GameObject _self, Transform _parent, string _name, Vector3 _localpos, Vector3 _scale ) where T : Component
        {
            GameObject obj = Object.Instantiate( _self );
            obj.name = _name;
            obj.SetActive( true );

            obj.transform.SetParent( _parent );
            obj.transform.localPosition = _localpos;
            obj.transform.localScale = _scale;

            //TがGameObjectの場合のみ、キャストして返す
            return typeof( T ) == typeof( GameObject ) ? obj as T : obj.GetComponent<T>();
        }

        /// <summary>
        /// UIのコピー
        /// </summary>
        /// <returns>コピーしたオブジェクトのジェネリックで指定した型</returns>
        /// <param name="_self">元のオブジェクト</param>
        /// <param name="_parent">親オブジェクト</param>
        /// <param name="_name">コピーしたオブジェクトの名前</param>
        /// <typeparam name="T">戻り値の型</typeparam>
        public static T CloneUI<T>( this GameObject _self, Transform _parent, string _name = "" ) where T : Component
        {
            string objName = string.IsNullOrEmpty( _name ) ? _self.name : _name;

            return CloneUI<T>( _self, _parent, objName, Vector3.zero, Vector3.one );
        }

        /// <summary>
        /// UIのコピー
        /// </summary>
        /// <returns>コピーしたオブジェクトのジェネリックで指定した型</returns>
        /// <param name="_self">元のオブジェクト</param>
        /// <param name="_parent">親オブジェクト</param>
        /// <param name="_name">コピーしたオブジェクトの名前</param>
        /// <param name="_anchorPos">コピーしたオブジェクトのAnchorPosition</param>
        /// <param name="_scale">コピーしたオブエクトのLocalScale</param>
        /// <typeparam name="T">戻り値の型</typeparam>
        public static T CloneUI<T>( this GameObject _self, Transform _parent, string _name, Vector3 _anchorPos, Vector3 _scale ) where T : Component
        {
            GameObject obj = Object.Instantiate( _self );
            obj.name = _name;
            obj.SetActive( true );

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.SetParent( _parent );
            rt.anchoredPosition3D = _anchorPos;
            rt.localScale = _scale;

            //TがGameObjectの場合のみ、キャストして返す
            return typeof( T ) == typeof( GameObject ) ? obj as T : obj.GetComponent<T>();
        }
    }
}