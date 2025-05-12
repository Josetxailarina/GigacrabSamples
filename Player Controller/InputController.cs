using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour


{
    public static event Action OnXPressed;
    public static event Action OnXReleased;
    public static event Action OnSquarePressed;
    public static event Action OnStartPressed;
    public static event Action OnStartPressedOnShop;


    // ---------   BUTTONS SETS ON PLAYER INPUTS ACTIONS  ------------

    public void XButton(InputAction.CallbackContext callBack)
    {
        if (callBack.performed )
        {
            OnXPressed?.Invoke();
        }
        else if (callBack.canceled )
        {
            OnXReleased?.Invoke();
        }
    }
   

    public void SquareButton(InputAction.CallbackContext callBack)
    {
        if (callBack.performed && GameManager.state == GameState.Play)
        {
            OnSquarePressed?.Invoke();
        }
    }

    public void StartButton(InputAction.CallbackContext callBack)
    {
        if (callBack.performed )
        {
            if (GameManager.state == GameState.Menu || GameManager.state == GameState.Play)
            {
                OnStartPressed?.Invoke();

            }
            else if (GameManager.state == GameState.Shop)
            {
                OnStartPressedOnShop?.Invoke();
            }
        }
    }
}
