﻿using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using UnityEngine.UI;
using Mobcast.Coffee;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Mobcast.Coffee.UI.ScrollModule
{
	/// <summary>
	/// スクロールスナッパー.
	/// スクロール領域をオブジェクトにスナップさせるモジュールです.
	/// ドラッグ終了後にスナップをトリガー出来ます.
	/// </summary>
	[Serializable]
	public class SnapModule
	{
#region Serialize

		[SerializeField] bool m_SnapOnEndDrag = false;
		[SerializeField][Range(10,1000)] float m_VelocityThreshold = 200;

#endregion Serialize

#region Public

		public ScrollRectEx handler { get; set;}

		/// <summary>
		/// ドラッグが終了した際、スナップをトリガします.
		/// </summary>
		public bool snapOnEndDrag { get{ return m_SnapOnEndDrag;} set{ m_SnapOnEndDrag = value;} }

		/// <summary>
		/// スクロール速度が値以下になったとき、Tweenを実行します.
		/// </summary>
		public float velocityThreshold { get{ return m_VelocityThreshold;} set{ m_VelocityThreshold = value;} }

		public event Action onEndNextTween;

		public void OnScroll(PointerEventData eventData)
		{
			_StopScrollTween();
			_mouseScrollCount = 10;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			_StopScrollTween();
			_isDragging = true;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			_isDragging = false;
			// スナップをトリガ. Tween中の場合はトリガしない.
			_triggerSnap = (_coTweening == null) && snapOnEndDrag;

			_oldVelocity = velocity;
		}

		float velocity { get { return handler.scrollRect.vertical ? handler.scrollRect.velocity.y : handler.scrollRect.velocity.x; } }

		public void Update()
		{
			if (0 < _mouseScrollCount && _mouseScrollCount-- == 0 && (_coTweening == null) && snapOnEndDrag)
			{
				_triggerSnap = true;
			}

			if (!_isDragging && _triggerSnap)
			{
				float v = velocity;
				float average = (v + _oldVelocity) / 2;

				if (Mathf.Abs(average) <= velocityThreshold)
				{
					_triggerSnap = false;
					handler.JumpTo(handler.activeIndex);
				}
				else if (handler.scrollRect.inertia && v == 0f)
				{
					v = average;
					handler.scrollRect.velocity = handler.scrollRect.vertical
						? new Vector2(0, v)
						: new Vector2(v, 0); 
				}
				_oldVelocity = v;
			}
		}

		public void StartScrollTween(Tweening.TweenMethod tweenType, float time, float startValue, float endValue)
		{
			_StopScrollTween();
			_coTweening = handler.scrollRect.StartCoroutine(_CoScrollTweening(tweenType, time, startValue, endValue));
		}

#endregion Public


#region Private

		bool _isDragging;
		int _mouseScrollCount;
		Coroutine _coTweening;
		bool _triggerSnap;
		bool _inertia = false;
		ScrollRect.MovementType _movementType;
		float _oldVelocity = 0;

		void _StopScrollTween()
		{
			_triggerSnap = false;
			_mouseScrollCount = 0;
			if (_coTweening != null)
			{
				handler.scrollRect.StopCoroutine(_coTweening);
				_coTweening = null;
				handler.scrollRect.inertia = _inertia;
				handler.scrollRect.movementType = _movementType;
				handler.scrollRect.velocity = Vector2.zero;
				if (onEndNextTween != null)
					onEndNextTween.Invoke();

				onEndNextTween = null;
			}
		}

		/// <summary>
		/// Tweenコルーチン.
		/// </summary>
		IEnumerator _CoScrollTweening(Tweening.TweenMethod tweenType, float time, float startValue, float endValue)
		{
			// Tween中はScrollRect自体の動作(inertia/movementType)を制限します.
			handler.scrollRect.velocity = Vector2.zero;
			_inertia = handler.scrollRect.inertia;
			_movementType = handler.scrollRect.movementType;
			handler.scrollRect.inertia = false;
			handler.scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
			bool positive = 0 < (endValue - startValue);

			if (tweenType == Tweening.TweenMethod.immediate || time <= 0)
			{
				handler.OnChangeTweenPosition(endValue, positive);
				yield return null;
			}
			else
			{
				// Tweenを実行します.
				float unscaleTimer = 0;
				while (unscaleTimer < time)
				{
					handler.OnChangeTweenPosition(Tweening.GetTweenValue(tweenType, startValue, endValue, unscaleTimer / time), positive);
					unscaleTimer += Time.unscaledDeltaTime;
					yield return null;
				}
			}

			// Tweenを停止します.
			handler.OnChangeTweenPosition(endValue, positive);
			_StopScrollTween();
			yield break;
		}
#endregion Private
	}
}