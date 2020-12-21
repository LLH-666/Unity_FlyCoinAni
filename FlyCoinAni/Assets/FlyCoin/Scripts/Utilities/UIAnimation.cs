using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

	public class UIAnimation : MonoBehaviour
	{
		#region Enums

		public enum Type
		{
			PositionX,
			PositionY,
			ScaleX,
			ScaleY,
			RotationZ,
			Width,
			Height,
			Color,
			Alpha
		}

		public enum LoopType
		{
			None,
			Repeat,
			Reverse
		}

		public enum Style
		{
			Linear,
			EaseIn,
			EaseOut,
			Custom
		}

		#endregion

		#region Inspector Variables

		public string			id;
		public Type				type;
		public LoopType			loopType;
		public Style			style;
		public float			duration;
		public float			startDelay;
		public bool				startOnFirstFrame;
		public bool				useCurrentFrom;
		public bool				playOnStart;
		public AnimationCurve	animationCurve;
		public float			elapsedTime;

		[SerializeField] private float fromValue;
		[SerializeField] private float toValue;

		[SerializeField] private Color fromColor	= UnityEngine.Color.white;
		[SerializeField] private Color toColor		= UnityEngine.Color.white;

		#endregion

		#region Member Variables

		private bool 	isPlaying;
		private bool	destroyOnFinish;
		private bool	isDestroyed;

		private RectTransform	rectT;
		private Graphic			graphic;
		private CanvasGroup		canvasGroup;

		#endregion

		#region Properties

		public bool							IsPlaying			{ get { return isPlaying; } }
		public System.Action<GameObject>	OnAnimationFinished	{ get; set; }

		#endregion

		#region Unity Methods

		private void Start()
		{
			if (playOnStart)
			{
				Play();
			}
		}

		private void Update()
		{
			if (isPlaying)
			{
				elapsedTime += Time.deltaTime;

				UpdateAnimation(elapsedTime);

				// 检查动画是否结束
				if (elapsedTime >= duration)
				{
					isPlaying = false;

					if (loopType != LoopType.None)
					{
						// 如果循环类型为reverse，则交换from和to值
						if (loopType == LoopType.Reverse)
						{
							useCurrentFrom = false;

							SwapValue(ref fromValue, ref toValue);
							SwapValue(ref fromColor, ref toColor);
						}

						// 再次播放动画
						Play();
					}
					else
					{
						if (destroyOnFinish)
						{
							// 销毁UIAnimation组件
							DestroyAnimation();
						}

						if (OnAnimationFinished != null)
						{
							OnAnimationFinished(gameObject);
						}
					}
				}
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// 使用给定的id播放给定游戏对象上的所有UIAnimations
		/// </summary>
		public static void PlayAllById(GameObject targetGameObject, string animationId)
		{
			UIAnimation[] uiAnimations = targetGameObject.GetComponents<UIAnimation>();

			for (int i = 0; i < uiAnimations.Length; i++)
			{
				if (uiAnimations[i].id == animationId)
				{
					uiAnimations[i].Play();
				}
			}
		}

		/// <summary>
		/// 停止给定游戏对象上具有给定id的所有UIAnimations
		/// </summary>
		public static void StopAllById(GameObject targetGameObject, string animationId)
		{
			UIAnimation[] uiAnimations = targetGameObject.GetComponents<UIAnimation>();

			for (int i = 0; i < uiAnimations.Length; i++)
			{
				if (uiAnimations[i].id == animationId)
				{
					uiAnimations[i].Stop();
				}
			}
		}

		public static UIAnimation GetAnimation(GameObject targetGameObject, Type animType)
		{
			UIAnimation[] uiAnimations = targetGameObject.GetComponents<UIAnimation>();

			for (int i = 0; i < uiAnimations.Length; i++)
			{
				if (uiAnimations[i].type == animType)
				{
					return uiAnimations[i];
				}
			}

			return null;
		}

		public static List<UIAnimation> GetAnimationsById(GameObject targetGameObject, string id)
		{
			UIAnimation[]		uiAnimations		= targetGameObject.GetComponents<UIAnimation>();
			List<UIAnimation>	returnAnimations	= new List<UIAnimation>();

			for (int i = 0; i < uiAnimations.Length; i++)
			{
				if (uiAnimations[i].id == id)
				{
					returnAnimations.Add(uiAnimations[i]);
				}
			}

			return returnAnimations;
		}

		public static void DestroyAllAnimations(GameObject targetGameObject)
		{
			UIAnimation[] uiAnimations = targetGameObject.GetComponents<UIAnimation>();

			for (int i = 0; i < uiAnimations.Length; i++)
			{
				uiAnimations[i].DestroyAnimation();
			}
		}

		public void Play()
		{
			// 初始化动画
			Init();

			// 检查动画设置是否有效
			if (!Check())
			{
				return;
			}

			// 如果使用当前值作为“开始”值，则立即设置它
			if (useCurrentFrom)
			{
				SetFromValue();
			}

			if (startOnFirstFrame)
			{
				// 在第一帧中设置动画
				UpdateAnimation(0);
			}

			if (startDelay > 0)
			{
				StartCoroutine(PlayAnimationAfterDelay(startDelay));
			}
			else
			{
				PlayAnimation();
			}
		}

		public void Stop()
		{
			isPlaying = false;
		}

		#endregion

		#region Private Methods

		private void Init()
		{
			switch (type)
			{
				case Type.PositionX:
				case Type.PositionY:
				case Type.Width:
				case Type.Height:
					rectT = transform as RectTransform;
					break;
				case Type.Color:
					graphic = gameObject.GetComponent<Graphic>();
					break;
				case Type.Alpha:
					canvasGroup = gameObject.GetComponent<CanvasGroup>();
					break;
			}
		}

		private void SetFromValue()
		{
			switch (type)
			{
				case Type.PositionX:
					fromValue = rectT.anchoredPosition.x;
					break;
				case Type.PositionY:
					fromValue = rectT.anchoredPosition.y;
					break;
				case Type.RotationZ:
					fromValue = transform.localEulerAngles.z;
					break;
				case Type.ScaleX:
					fromValue = transform.localScale.x;
					break;
				case Type.ScaleY:
					fromValue = transform.localScale.y;
					break;
				case Type.Width:
					fromValue = rectT.sizeDelta.x;
					break;
				case Type.Height:
					fromValue = rectT.sizeDelta.y;
					break;
				case Type.Alpha:
					fromValue = canvasGroup.alpha;
					break;
				case Type.Color:
					fromColor = graphic.color;
					break;
			}
		}

		private bool Check()
		{
			switch (type)
			{
				case Type.PositionX:
				case Type.PositionY:
				case Type.Width:
				case Type.Height:
					if (rectT == null)
					{
						Debug.LogErrorFormat("[UIAnimation] Cannot play {0} animation on GameObject {1}: There is no RectTransform component.", type, gameObject.name);
						return false;
					}

					break;
				case Type.Color:
					if (graphic == null)
					{
						Debug.LogErrorFormat("[UIAnimation] Cannot play {0} animation on GameObject {1}: There is no Graphic component.", type, gameObject.name);
						return false;
					}

					break;
				case Type.Alpha:
					if (canvasGroup == null)
					{
						Debug.LogErrorFormat("[UIAnimation] Cannot play {0} animation on GameObject {1}: There is no CanvasGroup component.", type, gameObject.name);
						return false;
					}

					break;
			}

			return true;
		}

		private IEnumerator PlayAnimationAfterDelay(float delay)
		{
			yield return new WaitForSeconds(delay);

			PlayAnimation();
		}

		private void PlayAnimation()
		{
			// Set as playing
			isPlaying	= true;
			elapsedTime	= 0;
		}

		private void UpdateAnimation(float time)
		{
			float	t	= GetLerpT(time);
			object	val	= GetValue(t);

			SetValue(val);
		}

		private float GetLerpT(float time)
		{
			float timeValue = (time > duration) ? duration : time;

			float t = (duration == 0) ? 1 : timeValue / duration;

			switch (style)
			{
				case Style.EaseIn:
					t = EaseIn(t);
					break;
				case Style.EaseOut:
					t = EaseOut(t);
					break;
				case Style.Custom:
					t = animationCurve.Evaluate(t);
					break;
			}

			return t;
		}

		private object GetValue(float t)
		{
			switch (type)
			{
				case Type.PositionX:
				case Type.PositionY:
				case Type.ScaleX:
				case Type.ScaleY:
				case Type.RotationZ:
				case Type.Width:
				case Type.Height:
				case Type.Alpha:
					return Mathf.LerpUnclamped(fromValue, toValue, t);
				case Type.Color:
					return UnityEngine.Color.Lerp(fromColor, toColor, t);
			}

			return null;
		}

		private void SetValue(object val)
		{
			switch (type)
			{
				case Type.PositionX:
					rectT.anchoredPosition = new Vector2((float)val, rectT.anchoredPosition.y);
					break;
				case Type.PositionY:
					rectT.anchoredPosition = new Vector2(rectT.anchoredPosition.x, (float)val);
					break;
				case Type.RotationZ:
					transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, (float)val);
					break;
				case Type.ScaleX:
					transform.localScale = new Vector3((float)val, transform.localScale.y, transform.localScale.z);
					break;
				case Type.ScaleY:
					transform.localScale = new Vector3(transform.localScale.x, (float)val, transform.localScale.z);
					break;
				case Type.Width:
					rectT.sizeDelta = new Vector2((float)val, rectT.sizeDelta.y);
					break;
				case Type.Height:
					rectT.sizeDelta = new Vector2(rectT.sizeDelta.x, (float)val);
					break;
				case Type.Alpha:
					canvasGroup.alpha = (float)val;
					break;
				case Type.Color:
					graphic.color = (Color)val;
					break;
			}
		}
		
		private void SwapValue<T>(ref T value1, ref T value2)
		{
			T temp = value1;
			value1 = value2;
			value2 = temp;
		}

		private float EaseOut(float t)
		{
			return 1.0f - (1.0f - t) * (1.0f - t) * (1.0f - t);
		}
		
		private float EaseIn(float t)
		{
			return t * t * t;
		}

		private void DestroyAnimation()
		{
			isDestroyed = true;

			Destroy(this);
		}

		#endregion

		#region Static Play Methods

		public static UIAnimation PositionX(RectTransform rectT, float to, float duration)
		{
			return CreateAnimation(Type.PositionX, rectT.gameObject, true, 0, to, duration);
		}

		public static UIAnimation PositionX(RectTransform rectT, float from, float to, float duration)
		{
			return CreateAnimation(Type.PositionX, rectT.gameObject, false, from, to, duration);
		}

		public static UIAnimation PositionY(RectTransform rectT, float to, float duration)
		{
			return CreateAnimation(Type.PositionY, rectT.gameObject, true, 0, to, duration);
		}

		public static UIAnimation PositionY(RectTransform rectT, float from, float to, float duration)
		{
			return CreateAnimation(Type.PositionY, rectT.gameObject, false, from, to, duration);
		}

		public static UIAnimation ScaleX(RectTransform rectT, float to, float duration)
		{
			return CreateAnimation(Type.ScaleX, rectT.gameObject, true, 0, to, duration);
		}

		public static UIAnimation ScaleX(RectTransform rectT, float from, float to, float duration)
		{
			return CreateAnimation(Type.ScaleX, rectT.gameObject, false, from, to, duration);
		}

		public static UIAnimation ScaleY(RectTransform rectT, float to, float duration)
		{
			return CreateAnimation(Type.ScaleY, rectT.gameObject, true, 0, to, duration);
		}

		public static UIAnimation ScaleY(RectTransform rectT, float from, float to, float duration)
		{
			return CreateAnimation(Type.ScaleY, rectT.gameObject, false, from, to, duration);
		}

		public static UIAnimation RotationZ(RectTransform rectT, float to, float duration)
		{
			return CreateAnimation(Type.RotationZ, rectT.gameObject, true, 0, to, duration);
		}

		public static UIAnimation RotationZ(RectTransform rectT, float from, float to, float duration)
		{
			return CreateAnimation(Type.RotationZ, rectT.gameObject, false, from, to, duration);
		}

		public static UIAnimation Width(RectTransform rectT, float from, float to, float duration)
		{
			return CreateAnimation(Type.Width, rectT.gameObject, false, from, to, duration);
		}

		public static UIAnimation Width(RectTransform rectT, float to, float duration)
		{
			return CreateAnimation(Type.Width, rectT.gameObject, true, 0, to, duration);
		}

		public static UIAnimation Height(RectTransform rectT, float from, float to, float duration)
		{
			return CreateAnimation(Type.Height, rectT.gameObject, false, from, to, duration);
		}

		public static UIAnimation Height(RectTransform rectT, float to, float duration)
		{
			return CreateAnimation(Type.Height, rectT.gameObject, true, 0, to, duration);
		}

		public static UIAnimation Alpha(CanvasGroup canvasGroup, float to, float duration)
		{
			return CreateAnimation(Type.Alpha, canvasGroup.gameObject, true, 0, to, duration);
		}

		public static UIAnimation Alpha(CanvasGroup canvasGroup, float from, float to, float duration)
		{
			return CreateAnimation(Type.Alpha, canvasGroup.gameObject, false, from, to, duration);
		}

		public static UIAnimation Color(Graphic graphic, Color to, float duration)
		{
			return CreateColorAnimation(graphic.gameObject, true, UnityEngine.Color.white, to, duration);
		}

		public static UIAnimation Color(Graphic graphic, Color from, Color to, float duration)
		{
			return CreateColorAnimation(graphic.gameObject, false, from, to, duration);
		}

		public static void SwapText(Text uiText, string text, float duration)
		{
			Color fromColor	= uiText.color;
			Color toColor	= uiText.color;

			fromColor.a	= 1f;
			toColor.a	= 0f;

			// Fade out the bundle text
			UIAnimation fadeOutAnim = UIAnimation.Color(uiText, fromColor, toColor, duration / 2f);

			fadeOutAnim.style = UIAnimation.Style.EaseOut;

			fadeOutAnim.OnAnimationFinished += (GameObject obj) => 
			{
				// Now that the text has faded out change the text
				uiText.text = text;

				fromColor.a	= 0f;
				toColor.a	= 1f;

				// Fade it back in
				UIAnimation fadeInAnim = UIAnimation.Color(uiText, fromColor, toColor, duration / 2f);

				fadeInAnim.style = UIAnimation.Style.EaseOut;

				fadeInAnim.Play();
			};

			fadeOutAnim.Play();
		}

		private static UIAnimation CreateAnimation(Type type, GameObject gameObject, bool useCurrentFrom, float from, float to, float duration)
		{
			UIAnimation uIAnimation = CreateUIAnimation(gameObject, type, duration, useCurrentFrom);

			if (!useCurrentFrom)
			{
				uIAnimation.fromValue = from;
			}

			uIAnimation.toValue = to;

			return uIAnimation;
		}

		private static UIAnimation CreateColorAnimation(GameObject gameObject, bool useCurrentFrom, Color from, Color to, float duration)
		{
			UIAnimation uIAnimation = CreateUIAnimation(gameObject, Type.Color, duration, useCurrentFrom);

			if (!useCurrentFrom)
			{
				uIAnimation.fromColor = from;
			}

			uIAnimation.toColor = to;

			return uIAnimation;
		}

		private static UIAnimation CreateUIAnimation(GameObject gameObject, Type type, float duration, bool useCurrentFrom)
		{
			UIAnimation uIAnimation = GetUIAnimation(gameObject, type);

			uIAnimation.duration		= duration;
			uIAnimation.useCurrentFrom	= useCurrentFrom;
			uIAnimation.destroyOnFinish	= true;

			return uIAnimation;
		}

		private static UIAnimation GetUIAnimation(GameObject gameObject, Type type)
		{
			UIAnimation[]	uIAnimations	= gameObject.GetComponents<UIAnimation>();
			UIAnimation		uIAnimation		= null;

			for (int i = 0; i < uIAnimations.Length; i++)
			{
				if (uIAnimations[i] != null && !uIAnimations[i].isDestroyed && uIAnimations[i].type == type)
				{
					uIAnimation = uIAnimations[i];

					break;
				}
			}

			if (uIAnimation == null)
			{
				uIAnimation			= gameObject.AddComponent<UIAnimation>();
				uIAnimation.type	= type;
			}

			return uIAnimation;
		}

		#endregion
	}