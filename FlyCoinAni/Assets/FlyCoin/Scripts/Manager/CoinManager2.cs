using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CoinManager2 : SingletonComponent<CoinManager2>
{
    #region Inspector Variables

    [SerializeField] private Text coinsText = null;
    [SerializeField] private RectTransform animationContainer = null;
    [SerializeField] private RectTransform animateToMarker = null;
    [SerializeField] private RectTransform coinPrefab = null;
    [SerializeField] private int amountPerCoin = 0;
    [SerializeField] private float animationDuration = 0;
    [SerializeField] private float explodeAnimationDuration = 0;
    [SerializeField] private float explodeForceOffset = 0;
    [SerializeField] private float delayBetweenCoins;

    #endregion

    #region Member Variables

    private ObjectPool coinPool;

    private int numCoinsAnimating;
    private int animCoinsAmount;

    #endregion

    #region Unity Methods

    private void Start()
    {
        coinPool = new ObjectPool(coinPrefab.gameObject, 1, animationContainer);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 增加硬币的数量
    /// </summary>
    public void SetCoinsText(int coins)
    {
        coinsText.text = coins.ToString();
    }

    /// <summary>
    /// 为硬币容器设置硬币动画
    /// </summary>
    public void AnimateCoins(int coinNum, RectTransform fromRect, float explodeForce,
        float startDelay = 0)
    {
        if (numCoinsAnimating == 0)
        {
            animCoinsAmount = GameManager.Instance.CoinAmount;
        }

        int numCoins = Mathf.CeilToInt((float) coinNum / amountPerCoin);

        numCoinsAnimating += numCoins;

        GameManager.Instance.IncreaseCoinAmount(coinNum);

        for (int i = 1; i <= numCoins; i++)
        {
            StartCoroutine(AnimateCoin(fromRect, startDelay, explodeForce));
        }

        //播放奖励音效
        //SoundManager.Instance.Play("coins-awarded", false, startDelay);
    }

    #endregion

    #region Private Methods

    private IEnumerator AnimateCoin(RectTransform fromRect, float startDelay, float explodeForce)
    {
        yield return new WaitForSeconds(startDelay);

        RectTransform coinToAnimate = coinPool.GetObject<RectTransform>(animationContainer);

        UIAnimation.DestroyAllAnimations(coinToAnimate.gameObject);

        coinToAnimate.anchoredPosition = SwitchToRectTransform(fromRect, animationContainer);
        coinToAnimate.sizeDelta = fromRect.sizeDelta;

        yield return ExplodeCoinOut(coinToAnimate, explodeForce);

        Vector2 toPosition = SwitchToRectTransform(animateToMarker, animationContainer);

        float duration = animationDuration + Random.Range(-0.1f, 0.1f);

        PlayAnimation(UIAnimation.PositionX(coinToAnimate, toPosition.x, duration));
        PlayAnimation(UIAnimation.PositionY(coinToAnimate, toPosition.y, duration));

        PlayAnimation(UIAnimation.Width(coinToAnimate, animateToMarker.sizeDelta.x, duration));
        PlayAnimation(UIAnimation.Height(coinToAnimate, animateToMarker.sizeDelta.y, duration));

        //播放金币到达目的地音效
        //SoundManager.Instance.Play("coin", false, duration - 0.1f);

        yield return new WaitForSeconds(duration);

        IncCoinTextForAnimation();
    }

    private IEnumerator ExplodeCoinOut(RectTransform coinToAnimate, float explodeForce)
    {
        Vector2 randDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        Vector2 toPosition = coinToAnimate.anchoredPosition +
                             randDir * (explodeForce + Random.Range(0, explodeForceOffset));

        UIAnimation anim;

        anim = UIAnimation.PositionX(coinToAnimate, toPosition.x,
            explodeAnimationDuration + Random.Range(-0.05f, 0.05f));
        anim.style = UIAnimation.Style.EaseOut;
        anim.Play();

        anim = UIAnimation.PositionY(coinToAnimate, toPosition.y,
            explodeAnimationDuration + Random.Range(-0.05f, 0.05f));
        anim.style = UIAnimation.Style.EaseOut;
        anim.Play();

        while (anim.IsPlaying)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Sets up and plays the UIAnimation for a coin
    /// </summary>
    private UIAnimation PlayAnimation(UIAnimation anim)
    {
        anim.style = UIAnimation.Style.EaseIn;

        anim.OnAnimationFinished += (GameObject target) => { coinPool.ReturnObjectToPool(target); };

        anim.Play();

        return anim;
    }

    private void IncCoinTextForAnimation()
    {
        numCoinsAnimating--;
        animCoinsAmount += amountPerCoin;
        
        if (numCoinsAnimating == 0 || animCoinsAmount > GameManager.Instance.CoinAmount)
        {
            SetCoinsText(GameManager.Instance.CoinAmount);
        }
        else
        {
            SetCoinsText(animCoinsAmount);
        }
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