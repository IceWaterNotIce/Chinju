using UnityEngine;
using UnityEngine.InputSystem;

public class Test1 : MonoBehaviour
{
    private InputSystem_Actions _inputActions;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        
        // Bind actions to methods
        _inputActions.UI.Click.started += ctx => StartAction();
        _inputActions.UI.Click.performed += ctx => PerformAction(ctx);
        _inputActions.UI.Click.canceled += ctx => CancelAction();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    private void StartAction()
    {
        Debug.Log("Action Started");
        // Implement your start logic here
    }

    private void PerformAction(InputAction.CallbackContext context)
    {
        Debug.Log("Action Performed");
        // Implement your perform logic here
        Debug.Log("Mouse Position: " + context.ReadValue<Vector2>());
        Debug.Log(context);
    }

    private void CancelAction()
    {
        Debug.Log("Action Canceled");
        // Implement your cancel logic here
    }
}