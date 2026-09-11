//NSMouseController.cpp : Unity側にマウスの入力状態を受け渡す

#define NN_POINTING_EXPORT_API extern "C"

#include <nn/hid/hid_Mouse.h>

/// <summary>
/// nn::hid::MouseStateをUnity側に渡すための構造体
/// </summary>
struct NSMouseState
{
    long SamplingNumber;
    int X;
    int Y;
    int DeltaX;
    int DeltaY;
    int WheelDelta;
    int Buttons;
    int Attribute;
};

/// <summary>
/// nn::hid::MouseStateをNSMouseStateに変換
/// </summary>
/// <param name="_pOutValue">返還後のNSMouseState</param>
/// <param name="_source">nn::hid::MouseStateの元データ</param>
void Convert( NSMouseState* _pOutValue, nn::hid::MouseState* _source )
{
    _pOutValue->SamplingNumber = _source->samplingNumber;
    _pOutValue->X = _source->x;
    _pOutValue->Y = _source->y;
    _pOutValue->DeltaX = _source->deltaX;
    _pOutValue->DeltaY = _source->deltaY;
    _pOutValue->WheelDelta = _source->wheelDelta;

    const nn::hid::MouseButtonSet buttons = _source->buttons;

    for( int i = 0; i < buttons.GetCount(); i++ )
    {
        if( buttons.Test( i ) ) _pOutValue->Buttons |= 0x1 << i;
    }

    const nn::hid::MouseAttributeSet attributes = _source->attributes;

    for( int i = 0; i < attributes.GetCount(); i++ )
    {
        if( attributes.Test( i ) ) _pOutValue->Attribute |= 0x1 << i;
    }
}

/// <summary>
/// 初期化
/// </summary>
NN_POINTING_EXPORT_API void nn_mouse_Initialize()
{
	nn::hid::InitializeMouse();
}

/// <summary>
/// Mouse の最新の入力状態を取得
/// </summary>
/// <param name="_outValue">入力状態を格納</param>
NN_POINTING_EXPORT_API void nn_mouce_GetState( NSMouseState* _pOutValue )
{
    nn::hid::MouseState state;
    nn::hid::GetMouseState( &state );

    Convert( _pOutValue, &state);
}

/// <summary>
/// Mouse の入力状態を過去に遡って取得
/// </summary>
/// <remarks>取得できる上限はAPIの仕様上16まで</remarks>
/// <param name="_outValue">入力状態を格納</param>
/// <param name="_count">取得する入力状態の数</param>
NN_POINTING_EXPORT_API int nn_mouce_GetStates( NSMouseState* _pOutValue, int _count )
{
    if( _count > nn::hid::MouseStateCountMax ) _count = nn::hid::MouseStateCountMax;

    nn::hid::MouseState stateList[ _count ];

    int resultCount = nn::hid::GetMouseStates( stateList, _count );

    for( int i = 0; i < resultCount; i ++)
    {
        Convert( &_pOutValue[ i ], &stateList[ i ]);
    }

    return resultCount;
}

