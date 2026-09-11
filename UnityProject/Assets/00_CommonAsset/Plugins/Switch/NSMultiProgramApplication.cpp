//NSMultiProgramApplication.cpp : Unityからマルチプログラムアプリケーション関連のAPIを呼び出す

#define NN_POINTING_EXPORT_API extern "C"

#include <nn/account/account_StateRetention.h>
#include <nn/account/account_Types.h>
#include <nn/oe/oe_MultiProgramApis.h>
#include <nn/oe/oe_SelfControlApis.h>

/// <summary>
/// Open状態のユーザーアカウントを、状態を維持したまま異なるプログラム間で利用可能にします。
/// </summary>
NN_POINTING_EXPORT_API void nn_acount_PushOpenUsers()
{
    nn::account::PushOpenUsers();
}

/// <summary>
/// プログラム間でOpen状態を維持しているユーザーアカウントの UserHandle を取得します。
/// </summary>
/// <param name="_pOut">取得したUserHandleの個数を格納する領域</param>
/// <param name="_pOutHandles">取得したUserHandleを格納する領域</param>
/// <param name="_handleCount">_pOutHandlesの要素数</param>
NN_POINTING_EXPORT_API void nn_acount_PopOpenUsers( int* pOut, nn::account::UserHandle* pOutHandles, int handleCount )
{
    nn::account::PopOpenUsers( pOut, pOutHandles, handleCount );
}

/// <summary>
/// 現在のプログラムを終了し、指定したプログラムを起動します。
/// </summary>
/// <param name="_programIndex">移行対象のプログラムのインデックス</param>
/// <param name="_data">受け渡すデータへのポインタ</param>
/// <param name="_dataSize">受け渡すデータのサイズ[</param>
NN_POINTING_EXPORT_API void nn_oe_ExecuteProgram( int programIndex, const void* pData, int dataSize )
{
     nn::oe::ExecuteProgram( programIndex, pData, dataSize );
}

/// <summary>
/// 直前に走っていたプログラムのインデックスを取得
/// </summary>
NN_POINTING_EXPORT_API int nn_oe_GetPreviousProgramIndex()
{
    return nn::oe::GetPreviousProgramIndex();
}

/// <summary>
/// プログラムの起動パラメータの取得
/// </summary>
/// <param name="_pOutParameterSize">起動パラメータの実サイズの格納先</param>
/// <param name="_pOutBuffer">起動パラメータを格納するバッファのアドレス</param>
/// <param name="_bufferSize">起動パラメータを格納するバッファのサイズ</param>
NN_POINTING_EXPORT_API bool nn_oe_TryPopLaunchParameter( size_t* pOutParameterSize, void* pOutBuffer, int bufferSize )
{
    return nn::oe::TryPopLaunchParameter( pOutParameterSize, pOutBuffer, bufferSize );
}