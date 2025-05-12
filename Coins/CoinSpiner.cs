using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpiner : MonoBehaviour
{
    public static CoinSpiner Instance { get; private set; }

    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float floatAmplitude = 0.1f;
    [SerializeField] private float floatFrequency = 4f;
    private List<Transform> coinsTransformsList = new List<Transform>();
    private float rotationAngle;
    private float floatOffset;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoin(Transform coinTransform)
    {
        if (!coinsTransformsList.Contains(coinTransform))
        {
            coinsTransformsList.Add(coinTransform);
        }
    }

    public void RemoveCoin(Transform coinTransform)
    {
        if (coinsTransformsList.Contains(coinTransform))
        {
            coinsTransformsList.Remove(coinTransform);
        }
    }

    void Update()
    {
        rotationAngle += rotationSpeed * Time.deltaTime;
        floatOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        foreach (Transform coin in coinsTransformsList)
        {
            coin.localRotation = Quaternion.Euler(0, rotationAngle, 0);
            coin.localPosition = new Vector3(coin.localPosition.x, floatOffset, coin.localPosition.z);
        }
    }
}
