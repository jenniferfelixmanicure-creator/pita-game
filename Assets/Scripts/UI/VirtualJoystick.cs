using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Joystick virtual para controle mobile com suporte a toque dinâmico.
/// </summary>
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Configurações")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    [SerializeField] private float maxRadius = 80f;
    [SerializeField] private bool dynamicPosition = true;

    public Vector2 Direction { get; private set; }
    public float Magnitude => Direction.magnitude;
    public bool IsPressed { get; private set; }

    private Vector2 startPos;
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        startPos = background.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData data)
    {
        IsPressed = true;
        if (dynamicPosition && canvas)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                data.position,
                canvas.worldCamera,
                out Vector2 localPos
            );
            background.anchoredPosition = localPos;
        }
        background.gameObject.SetActive(true);
        OnDrag(data);
    }

    public void OnDrag(PointerEventData data)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, data.position, canvas.worldCamera, out Vector2 localPos)) return;

        localPos = Vector2.ClampMagnitude(localPos, maxRadius);
        handle.anchoredPosition = localPos;
        Direction = localPos / maxRadius;
    }

    public void OnPointerUp(PointerEventData data)
    {
        IsPressed = false;
        handle.anchoredPosition = Vector2.zero;
        Direction = Vector2.zero;

        if (dynamicPosition)
        {
            background.anchoredPosition = startPos;
            background.gameObject.SetActive(false);
        }
    }
}
