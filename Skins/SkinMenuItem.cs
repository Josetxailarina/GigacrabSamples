using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinMenuItem : MonoBehaviour
{
    [SerializeField] private GameObject textBlocked;
    [SerializeField] private GameObject textName;
    [SerializeField] private Image imagen;
    [SerializeField] private bool active= false;

    public void ActivateSkin() //CALLED FROM SKINMANAGER
    {
        if (!active)
        {
            textBlocked.SetActive(false);
            textName.SetActive(true);
            imagen.color = Color.white;
            active = true;
        }
    }
}
