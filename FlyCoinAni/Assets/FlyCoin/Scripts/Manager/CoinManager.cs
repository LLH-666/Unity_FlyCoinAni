using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinManager : SingletonComponent<CoinManager>
{
    #region Inspector Variables

    [SerializeField] private Text coinsText;
    [SerializeField] private RectTransform animateTo;
    [SerializeField] private RectTransform animationContainer;
    [SerializeField] private RectTransform coinPrefab;
    [SerializeField] private float animationDuration;
    [SerializeField] private float delayBetweenCoins;

    #endregion

    #region Member Variables

    private ObjectPool coinPool;

    #endregion

    #region Unity Methods

    private void Start()
    {
        coinPool = new ObjectPool(coinPrefab.gameObject, 1, animationContainer);
    }

    #endregion

    #region Public Methods

    public void PlayFlyCoin(int coin, RectTransform coinPrizeIcon)
    {
        //获得当前硬币数量和奖励金额
        int coinsAwarded = coin / 10;
        int coinsAmountFrom = GameManager.Instance.CoinAmount;

        //马上给硬币，但不要更新文字。这样一来，应用程序退出，玩家就得到了硬币
        //但我们不想更新文本，直到动画发生在完整的弹出窗口
        GameManager.Instance.IncreaseCoinAmount(coin);

        //在给了硬币之后，得到硬币的数量
        int coinsAmountTo = GameManager.Instance.CoinAmount;

        //动画
        List<RectTransform> fromPositions = new List<RectTransform>();

        for (int i = 0; i < coinsAwarded; i++)
        {
            fromPositions.Add(coinPrizeIcon);
        }

        AnimateCoins(coinsAmountFrom, coinsAmountTo, fromPositions);
    }

    /// <summary>
    /// 增加硬币的数量
    /// </summary>
    private void SetCoinsText(int coins)
    {
        coinsText.text = coins.ToString();
    }

    /// <summary>
    /// 为硬币容器设置硬币动画
    /// </summary>
    private void AnimateCoins(int fromCoinAmount, int toCoinAmount, List<RectTransform> fromRects)
    {
        //播放音效
        //SoundManager.Instance.Play("coin-awarded");

        for (int i = 0; i < fromRects.Count; i++)
        {
            // 分别设置每个硬币的动画，当硬币到达硬币容器时，设置文本硬币数量
            int setCoinsTextTo = (int) Mathf.Lerp(fromCoinAmount, toCoinAmount, (i + 1) * 1.0f / fromRects.Count);

            AnimateCoin(fromRects[i], (float) i * delayBetweenCoins, setCoinsTextTo);
        }
    }

    #endregion

    #region Private Methods

    private void AnimateCoin(RectTransform coinRectTransform, float startDelay, int setCoinAmountTextTo)
    {
        RectTransform coinToAnimate = coinPool.GetObject<RectTransform>();

        UIAnimation.DestroyAllAnimations(coinToAnimate.gameObject);

        // 需要将coinToAnimate的比例设置为与coinRectTransform相同的比例
        coinToAnimate.SetParent(coinRectTransform.parent, false);
        coinToAnimate.sizeDelta = coinRectTransform.sizeDelta;
        coinToAnimate.localScale = coinRectTransform.localScale;
        coinToAnimate.anchoredPosition = coinRectTransform.anchoredPosition;
        coinToAnimate.SetParent(animationContainer);

        Vector2 animateToPosition = SwitchToRectTransform(animateTo, animationContainer);

        // 指定硬币的x位置上的动画
        PlayAnimation(UIAnimation.PositionX(coinToAnimate, animateToPosition.x, animationDuration), startDelay);

        // 指定硬币的y位置上的动画
        PlayAnimation(UIAnimation.PositionY(coinToAnimate, animateToPosition.y, animationDuration), startDelay);

        // 指定硬币的x缩放上的动画
        PlayAnimation(UIAnimation.ScaleX(coinToAnimate, 1, animationDuration), startDelay);

        // 指定硬币的y缩放上的动画
        PlayAnimation(UIAnimation.ScaleY(coinToAnimate, 1, animationDuration), startDelay);

        // 指定硬币宽度上的动画
        PlayAnimation(UIAnimation.Width(coinToAnimate, animateTo.sizeDelta.x, animationDuration), startDelay);

        // 指定硬币长度上的动画
        PlayAnimation(UIAnimation.Height(coinToAnimate, animateTo.sizeDelta.y, animationDuration), startDelay);

        StartCoroutine(WaitThenSetCoinsText(setCoinAmountTextTo, animationDuration + startDelay));
    }

    /// <summary>
    /// 设置并播放硬币的UIAnimation
    /// </summary>
    private void PlayAnimation(UIAnimation anim, float startDelay)
    {
        anim.style = UIAnimation.Style.EaseOut;
        anim.startDelay = startDelay;
        anim.startOnFirstFrame = true;

        anim.OnAnimationFinished += (GameObject target) => { coinPool.ReturnObjectToPool(target); };

        anim.Play();
    }

    private IEnumerator WaitThenSetCoinsText(int coinAmount, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        SetCoinsText(coinAmount);
    }

    private Vector2 SwitchToRectTransform(RectTransform from, RectTransform to)
    {
        Vector2 localPoint;
        Vector2 fromPivotDerivedOffset = new Vector2(from.rect.width * from.pivot.x + from.rect.xMin,
            from.rect.height * from.pivot.y + from.rect.yMin);
        Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);

        screenP += fromPivotDerivedOffset;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenP, null, out localPoint);

        Vector2 pivotDerivedOffset = new Vector2(to.rect.width * to.pivot.x + to.rect.xMin,
            to.rect.height * to.pivot.y + to.rect.yMin);

        return localPoint - pivotDerivedOffset;
    }

    #endregion
}