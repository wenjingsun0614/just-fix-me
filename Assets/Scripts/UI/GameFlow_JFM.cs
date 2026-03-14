using UnityEngine;

public class GameFlow_JFM : MonoBehaviour
{
    public static bool CanDrag { get; private set; } = false;

    public static void LockDrag()
    {
        CanDrag = false;
    }

    public static void UnlockDrag()
    {
        CanDrag = true;
    }
}