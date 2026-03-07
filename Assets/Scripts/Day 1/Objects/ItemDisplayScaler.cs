using UnityEngine;

[RequireComponent(typeof(DraggableItem2D))]
public class ItemDisplayScaler : MonoBehaviour
{
    [Header("Display Scale Multiplier")]
    [Tooltip("用于气泡内和病人身上显示时的缩放倍率")]
    public float displayScaleMultiplier = 0.7f;

    private DraggableItem2D drag;

    void Awake()
    {
        drag = GetComponent<DraggableItem2D>();
    }

    public float GetDisplayScaleMultiplier()
    {
        return displayScaleMultiplier;
    }
}