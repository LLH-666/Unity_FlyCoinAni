using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonComponent<GameManager>
{
    [SerializeField] private RectTransform _prizeCoin;
    
    public int CoinAmount { get; private set; }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CoinManager.Instance.PlayFlyCoin(50, _prizeCoin);
        }
        if (Input.GetMouseButtonDown(1))
        {
            CoinManager2.Instance.AnimateCoins(50, _prizeCoin, 100);
        }
    }


    public void IncreaseCoinAmount(int coinNum)
    {
        CoinAmount += coinNum;
    }
    
}
