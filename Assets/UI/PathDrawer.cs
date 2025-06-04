using UnityEngine;
using UnityEngine.InputSystem;

public class PathDrawer : Singleton<PathDrawer>
{
    private LineRenderer lineRenderer;
    private int positionCount;
    private InputSystem_Actions inputActions;


    private bool isDrawing = false;

    protected override void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.UI.Click.performed += ctx => Draw(ctx);
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    { 
        inputActions.Disable();
    }

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        positionCount = 0;
        lineRenderer.positionCount = positionCount;
    }

    private void Update()
    {
    }

    private void Draw(InputAction.CallbackContext context)
    {
        if (isDrawing)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
            AddPoint(worldPosition);
        }
        isDrawing = !isDrawing;
    }


    private void AddPoint(Vector3 point)
    {
        // Only add the point if it's new
        if (positionCount == 0 || Vector3.Distance(lineRenderer.GetPosition(positionCount - 1), point) > 0.1f)
        {
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(positionCount, point);
            positionCount++;
        }
        Debug.Log("Point added: " + point);
    }

    public void ClearPath()
    {
        lineRenderer.positionCount = 0;
        positionCount = 0;
    }
}