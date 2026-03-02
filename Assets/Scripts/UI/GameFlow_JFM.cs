using UnityEngine;

public class GameFlow_JFM : MonoBehaviour
{
    public static bool CanDrag { get; private set; } = false;

    public void LockDrag() => CanDrag = false;
    public void UnlockDrag() => CanDrag = true;
}