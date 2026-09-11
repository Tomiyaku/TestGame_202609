using UnityEngine;

using CodeIcf.Extensions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace jp.co.liica.q2.Achievement
{
    /// <summary>
    /// 特定のパラメータが条件の実績
    /// </summary>
    [System.Serializable]
    public class TargetParamAchievement
    {
        /// <summary>実績のAPI名</summary>
        [CustomLabel( "API名" )]
        public string ApiName;
        /// <summary>解放条件のパラメータ</summary>
        [CustomLabel( "実績解放条件の値" )]
        public int TargetParam;
    }

    /// <summary>
    /// Steam実績のAPI名と解除条件のデータ
    /// </summary>
    [CreateAssetMenu( fileName = "SteamAchievementData", menuName = "ScriptableObjects/Create SteamAchievementData" )]
    public class SteamAchievementData : ScriptableObject
    {
        public string ClearedStageCountDataApiName = "ClearedStageCount";
        public string GetMedalCountDataApiName = "GetMedalCount";
        public string DrawLehgthDataApiName = "DrawLength";
        public string RetryCountDataApiName = "RetryCount";

        /// <summary>初回起動の実績API名</summary>
        public string FirstBootApiName = "";
        /// <summary>初めて問題を説いた実績</summary>
        public TargetParamAchievement FirstClearedStage = new TargetParamAchievement() { TargetParam = 0 };
        /// <summary>メダルカウント1以上の実績</summary>
        public TargetParamAchievement FirstGetMedal = new TargetParamAchievement() { TargetParam = 1 };
        /// <summary>ID0007までのステージクリアの実績</summary>
        public TargetParamAchievement CrearedTutorialStage = new TargetParamAchievement() { TargetParam = 7 };
        /// <summary>ID0007のステージをクリアの実績</summary>
        public TargetParamAchievement FirstUnlockkNewArea = new TargetParamAchievement() { TargetParam = 7 };
        /// <summary>ID0080のステージをクリアの実績</summary>
        public TargetParamAchievement UnlockRevaerceSide = new TargetParamAchievement() { TargetParam = 80 };
        /// <summary>2人以上でステージをプレイした実績API名</summary>
        public string MultiPlayApiName = "";
        /// <summary>キャラクターの取得実績一覧</summary>
        public TargetParamAchievement[] UnlockCharacterAchivementList;
        /// <summary>ステージクリア数の実績一覧</summary>
        public TargetParamAchievement[] ClearedStageCountAchivementList;
        /// <summary>表面のすべての問題をクリア実績API名</summary>
        public string AllCleardSurfaceStageApiName = "";
        /// <summary>裏面のすべての問題をクリア実績API名</summary>
        public string AllCleardReverseStageApiName = "";
        /// <summary>メダルの獲得数の実績一覧</summary>
        public TargetParamAchievement[] GetMedalCountAchivementList;
        /// <summary>表面のすべてのメダルを取得した実績API名</summary>
        public string AllGetMedalToSurfaceStageApiName = "";
        /// <summary>裏面のすべてのメダルを取得した実績API名</summary>
        public string AllGetMedalToReverseStageApiName = "";
        /// <summary>すべての問題をクリアし、すべてのメダルを取得した実績API名</summary>
        public string AllStageCleredAndAllGetkMedalApiName = "";
        /// <summary>描いた線の長さの実績一覧</summary>
        public TargetParamAchievement[] DrawLengthAchivementList;
        /// <summary>1ステージ内で描いた長さが1000mに達した実績</summary>
        public TargetParamAchievement DrawLength1000mToOneStage = new TargetParamAchievement() { TargetParam = 1000 };
        /// <summary>リトライ回数の実績一覧</summary>
        public TargetParamAchievement[] RetryCountAchivementList;
        /// <summary>すべての実績を解放した実績API名</summary>
        public string CompliteAchivementApiName = "";
    }

#if UNITY_EDITOR
    [CustomEditor( typeof( SteamAchievementData ) )]
    public class TargetParamAchivementDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using( new GUILayout.VerticalScope( GUI.skin.box ) )
            {
                SerializedProperty cleaedStageCountApiNameiPropetry = serializedObject.FindProperty( "ClearedStageCountDataApiName" );
                EditorGUILayout.PropertyField( cleaedStageCountApiNameiPropetry, new GUIContent( "ステージクリア数 データAPI名" ) );

                SerializedProperty getMedalCountApiNameiPropetry = serializedObject.FindProperty( "GetMedalCountDataApiName" );
                EditorGUILayout.PropertyField( getMedalCountApiNameiPropetry, new GUIContent( "メダル獲得数 データAPI名" ) );

                SerializedProperty drawLengthApiNameiPropetry = serializedObject.FindProperty( "DrawLehgthDataApiName" );
                EditorGUILayout.PropertyField( drawLengthApiNameiPropetry, new GUIContent( "描いた線の長さ データAPI名" ) );

                SerializedProperty retryCountApiNameiPropetry = serializedObject.FindProperty( "RetryCountDataApiName" );
                EditorGUILayout.PropertyField( retryCountApiNameiPropetry, new GUIContent( "リトライ回数 データAPI名" ) );
            }

            GUILayout.Space( 20 );

            SerializedProperty firstbootPropetry = serializedObject.FindProperty( "FirstBootApiName" );
            EditorGUILayout.PropertyField( firstbootPropetry, new GUIContent( "No.1 初めてQ2を起動した API名" ) );

            using( new GUILayout.VerticalScope( GUI.skin.box ) )
            {
                SerializedProperty property = serializedObject.FindProperty( "FirstClearedStage" );
                EditorGUILayout.PropertyField( property, new GUIContent( "No.2 初めてステージをクリアした" ) );
            }

            using( new GUILayout.VerticalScope( GUI.skin.box ) )
            {
                SerializedProperty property = serializedObject.FindProperty( "FirstGetMedal" );
                EditorGUILayout.PropertyField( property, new GUIContent( "No.3 初めてメダルを取得した" ) );
            }

            using( new GUILayout.VerticalScope( GUI.skin.box ) )
            {
                SerializedProperty property = serializedObject.FindProperty( "CrearedTutorialStage" );
                EditorGUILayout.PropertyField( property, new GUIContent( "No.4 チュートリアルをクリアした" ) );
            }

            using( new GUILayout.VerticalScope( GUI.skin.box ) )
            {
                SerializedProperty property = serializedObject.FindProperty( "FirstUnlockkNewArea" );
                EditorGUILayout.PropertyField( property, new GUIContent( "No.5 初めて新しいエリアを開放した" ) );
            }

            using( new GUILayout.VerticalScope( GUI.skin.box ) )
            {
                SerializedProperty property = serializedObject.FindProperty( "UnlockRevaerceSide" );
                EditorGUILayout.PropertyField( property, new GUIContent( "No.6 裏ステージを開放した" ) );
            }

            SerializedProperty MultiPlayApiNamePropetry = serializedObject.FindProperty( "MultiPlayApiName" );
            EditorGUILayout.PropertyField( MultiPlayApiNamePropetry, new GUIContent( "No.7 初めてマルチプレイをした API名" ) );

            using( new GUILayout.VerticalScope( GUI.skin.box ) )
            {
                SerializedProperty property = serializedObject.FindProperty( "UnlockCharacterAchivementList" );
                EditorGUILayout.PropertyField( property, new GUIContent( "No.8~25 キャラクター取得実績一覧" ) );
            }

            using( new GUILayout.VerticalScope( GUI.skin.box ) )
            {
                SerializedProperty property = serializedObject.FindProperty( "ClearedStageCountAchivementList" );
                EditorGUILayout.PropertyField( property, new GUIContent( "No.26~37 問題を指定数以上クリア実績一覧" ) );
            }

            SerializedProperty allCleardSurfaceStagePropetry = serializedObject.FindProperty( "AllCleardSurfaceStageApiName" );
            EditorGUILayout.PropertyField( allCleardSurfaceStagePropetry, new GUIContent( "No.38 すべての表ステージをクリアした API名" ) );

            SerializedProperty allCleardReverseStagePropetry = serializedObject.FindProperty( "AllCleardReverseStageApiName" );
            EditorGUILayout.PropertyField( allCleardReverseStagePropetry, new GUIContent( "No.39 すべての裏ステージをクリアした API名" ) );

            using( new GUILayout.VerticalScope( GUI.skin.box ) )
            {
                SerializedProperty property = serializedObject.FindProperty( "GetMedalCountAchivementList" );
                EditorGUILayout.PropertyField( property, new GUIContent( "No.40~51 メダルを指定枚数以上取得した実績一覧" ) );
            }

            SerializedProperty allGetMedalToSurfaceStagePropetry = serializedObject.FindProperty( "AllGetMedalToSurfaceStageApiName" );
            EditorGUILayout.PropertyField( allGetMedalToSurfaceStagePropetry, new GUIContent( "No.52 表ステージのすべてのメダルを獲得した API名" ) );

            SerializedProperty allGetMedalToReverseStagePropetry = serializedObject.FindProperty( "AllGetMedalToReverseStageApiName" );
            EditorGUILayout.PropertyField( allGetMedalToReverseStagePropetry, new GUIContent( "No.53 裏ステージのすべてのメダルを獲得した API名" ) );

            SerializedProperty allStageCleredAndAllGetkMedalPropetry = serializedObject.FindProperty( "AllStageCleredAndAllGetkMedalApiName" );
            EditorGUILayout.PropertyField( allStageCleredAndAllGetkMedalPropetry, new GUIContent( "No.54 すべてのステージをクリアし、すべてのメダルを取得した" ) );

            using( new GUILayout.VerticalScope( GUI.skin.box ) )
            {
                SerializedProperty property = serializedObject.FindProperty( "DrawLengthAchivementList" );
                EditorGUILayout.PropertyField( property, new GUIContent( "No.55~64 描いた長さが指定以上に達した実績一覧" ) );
            }

            using( new GUILayout.VerticalScope( GUI.skin.box ) )
            {
                SerializedProperty property = serializedObject.FindProperty( "DrawLength1000mToOneStage" );
                EditorGUILayout.PropertyField( property, new GUIContent( "No.65 1ステージ内で描いた長さが1000mに達した" ) );
            }

            using( new GUILayout.VerticalScope( GUI.skin.box ) )
            {
                SerializedProperty property = serializedObject.FindProperty( "RetryCountAchivementList" );
                EditorGUILayout.PropertyField( property, new GUIContent( "No.66~67 リトライ回数が指定回数を超えたの実績一覧" ) );
            }

            SerializedProperty allUnlockAchivementlPropetry = serializedObject.FindProperty( "CompliteAchivementApiName" );
            EditorGUILayout.PropertyField( allUnlockAchivementlPropetry, new GUIContent( "No.68 すべての実績を解放した API名" ) );

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}