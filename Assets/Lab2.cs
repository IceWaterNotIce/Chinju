using UnityEngine;
using UnityEngine.InputSystem;

public class Lab2 : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.UI.Click.performed += ctx => OnClick(ctx);
        inputActions.UI.Point.performed += ctx => OnPointerMove(ctx);
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void OnClick(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = context.ReadValue<Vector2>();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
        Debug.Log("Click Position: " + worldPosition);
    }

    void OnPointerMove(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = context.ReadValue<Vector2>();
        Debug.Log("Pointer Position: " + mousePosition);
    }
}