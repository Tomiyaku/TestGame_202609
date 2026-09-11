#define NN_POINTING_EXPORT_API extern "C"

#include <nn/hid/hid_Keyboard.h>

/// <summary>
/// nn::hid::KeyboardState をUnity側に渡すための構造体
/// </summary>
struct NSKeyboatdState
{
	/// <summary>Keyboard の入力状態がサンプリングされる度に増加する値</summary>
	long SamplingNumber;
	/// <summary>キーボードの状態</summary>
	int Attribute;
	/// <summary>修飾キーの状態</summary>
	int Modifier;
	/// <summary>キーの状態</summary>
	int* Keys;
};

/// <summary>
/// 初期化
/// </summary>
NN_POINTING_EXPORT_API void nn_keyboard_Initialize()
{
	nn::hid::InitializeKeyboard();
}

/// <summary>
/// キーボードの状態を取得
/// </summary>
/// <param name="_pOutValue"></param>
/// <returns></returns>
NN_POINTING_EXPORT_API void nn_keyboard_GetState( NSKeyboatdState* _pOutValue )
{
	const int KEY_SIZE = 8;
	const int INT_BYTE_SIZE = 32;

	nn::hid::KeyboardState  state;
	nn::hid::GetKeyboardState( &state );

	_pOutValue->SamplingNumber = state.samplingNumber;
	_pOutValue->Attribute = 0;
	_pOutValue->Modifier = 0;
	_pOutValue->Keys = new int[ KEY_SIZE ];

	for( int i = 0; i < KEY_SIZE; i++ )
	{
		_pOutValue->Keys[ i ] = 0;
	}

	// Keyboard の入力状態を取得
	const nn::hid::KeyboardAttributeSet attributes = state.attributes;	

	for( int i = 0; i < attributes.GetCount(); i++ )
	{
		if( attributes.Test( i ) ) _pOutValue->Attribute |= 0x1 << i;
	}

	// Keyboard の修飾情報を取得
	const nn::hid::KeyboardModifierSet modifiers = state.modifiers;	

	for( int i = 0; i < modifiers.GetCount(); i++ )
	{
		if( modifiers.Test( i ) ) _pOutValue->Modifier |= 0x1 << i;
	} 

	// Keyboard のキー状謡を取得
	const nn::hid::KeyboardKeySet keys = state.keys;
	
	for( int i = 0; i < keys.GetCount(); i++ )
	{
		int index = i / INT_BYTE_SIZE;
		int shift = i % INT_BYTE_SIZE;

		if( keys.Test( i ) )
		{
			_pOutValue->Keys[ index ] |= 0x1 << shift;
		}
	}
}