using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableItem2D : MonoBehaviour
{
    public float snapBackTime = 0.18f;

    Vector3 startPos;
    bool dragging;
    Camera cam;
    Collider2D col;
    Coroutine moveCo;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();
        startPos = transform.position;
    }

    void Update()
    {
        if (cam == null) return;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = transform.position.z;

        if (Input.GetMouseButtonDown(0))
        {
            // 用 OverlapPoint 检测是否点到自己
            Vector2 p = new Vector2(world.x, world.y);
            if (col.OverlapPoint(p))
            {
                dragging = true;
                if (moveCo != null) StopCoroutine(moveCo);
            }
        }

        if (dragging && Input.GetMouseButton(0))
        {
            transform.position = world;
        }

        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;
            if (moveCo != null) StopCoroutine(moveCo);
            moveCo = StartCoroutine(SnapBack());
        }
    }

    IEnumerator SnapBack()
    {
        Vector3 from = transform.position;
        Vector3 to = startPos;

        float t = 0f;
        while (t < snapBackTime)
        {
            t += Time.deltaTime;
            float p = t / snapBackTime;
            p = 1f - Mathf.Pow(1f - p, 3f);
            transform.position = Vector3.Lerp(from, to, p);
            yield return null;
        }

        transform.position = to;
    }
}
