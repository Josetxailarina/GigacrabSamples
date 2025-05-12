using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddCoinMoveOnVisible : MonoBehaviour
{
    [SerializeField] private Transform coinParentTransform;
    void OnBecameVisible()
    {
        CoinSpiner.Instance.AddCoin(coinParentTransform);
    }

    void OnBecameInvisible()
    {
        CoinSpiner.Instance.RemoveCoin(coinParentTransform);
    }
    private void OnDisable()
    {
        CoinSpiner.Instance.RemoveCoin(coinParentTransform);
    }
}
