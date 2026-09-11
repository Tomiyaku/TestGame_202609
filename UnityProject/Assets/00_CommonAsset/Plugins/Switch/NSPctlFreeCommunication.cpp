//NSPctlFreeCommunication.cpp : Unity側でペアレンタルコントロールのコミュニケーションチェックをする

#define NN_POINTING_EXPORT_API extern "C"

// ペアレンタルコントロールサービス利用
#include <nn/pctl.h>

/// <summary>
/// 「他の人との自由なコミュニケーション」機能の利用開始を試みます。
/// </summary>
NN_POINTING_EXPORT_API bool nn_pctl_TryBeginFreeCommunication(bool isShowUi)
{
	return nn::pctl::TryBeginFreeCommunication(isShowUi);
}

/// <summary>
/// 「他の人との自由なコミュニケーション」機能の利用を終了します。
/// </summary>
NN_POINTING_EXPORT_API void nn_pctl_EndFreeCommunication()
{
	nn::pctl::EndFreeCommunication();
}

/// <summary>
/// 「他の人との自由なコミュニケーション」機能が利用可能か否かを判定します。
/// </summary>
NN_POINTING_EXPORT_API bool nn_pctl_IsFreeCommunicationAvailable()
{
	return nn::pctl::IsFreeCommunicationAvailable();
}

