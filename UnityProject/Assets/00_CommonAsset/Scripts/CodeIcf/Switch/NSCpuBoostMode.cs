#define USE_CPU_BOOST_MODE

using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_SWITCH
using UnityEngine.Switch;
#endif

namespace CodeIcf.Switch
{
    /// <summary>
    /// Nintendo SwitchのCPUブーストモードの切り替え
    /// </summary>
    public static class NSCpuBoostMode
    {
#if USE_CPU_BOOST_MODE
        /// <summary>バックグランドの優先順位</summary>
        private static ThreadPriority DefaultThreadPriority = ThreadPriority.Normal;
#endif
        public static bool EnableCpuBoost { get; private set; } = false;

        /// <summary>
        /// CPUブーストモードを有効にする
        /// </summary>
        /// <param name="_isSceneLoadedDisable">シーンの読み込みが終わった際に自動で無効化するか</param>
        public static void SetEnable( bool _isSceneLoadedDisable = true )
        {
            Debug.Log( "EnableCpuBootsMode!" );

#if USE_CPU_BOOST_MODE
            DefaultThreadPriority = Application.backgroundLoadingPriority;
            Application.backgroundLoadingPriority = ThreadPriority.High;
#if UNITY_SWITCH && !UNITY_EDITOR
            Performance.SetCpuBoostMode( Performance.CpuBoostMode.FastLoad );
#endif //UNITY_SWITCH
            if( _isSceneLoadedDisable ) SceneManager.sceneLoaded += SceneLoadedEvent;
#endif //USE_CPU_BOOST_MODE

            EnableCpuBoost = true;
        }

        /// <summary>
        /// CPUブーストモードを有効にする
        /// </summary>
        public static void SetDisable()
        {
            if( !EnableCpuBoost ) return;

            Debug.Log( "DisableCpuBootsMode!" );

#if USE_CPU_BOOST_MODE
#if UNITY_SWITCH && !UNITY_EDITOR
            Performance.SetCpuBoostMode( Performance.CpuBoostMode.Normal );
#endif //UNITY_SWITCH
            Application.backgroundLoadingPriority = DefaultThreadPriority;
            SceneManager.sceneLoaded -= SceneLoadedEvent;
#endif //USE_CPU_BOOST_MODE

            EnableCpuBoost = false;
        }

        /// <summary>
        /// シーン遷移後にCPUブーストモードを無効にする
        /// </summary>
        /// <param name="_next"></param>
        /// <param name="_mode"></param>
        public static void SceneLoadedEvent( Scene _next, LoadSceneMode _mode )
        {
            SetDisable();            
        }
    }
}