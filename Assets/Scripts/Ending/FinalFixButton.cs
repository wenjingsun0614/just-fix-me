using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class FinalFixButton : MonoBehaviour
{
    public Collider2D dropZoneCollider;
    public string endingSceneName = "final_achievement_scene";

    public float snapToCenterTime = 0.15f;
    public Transform bubbleCenter;

    private Camera cam;
    private Collider2D col;
    private SpriteRenderer sr;

    private bool dragging;
    private Vector3 dragOffset;
    private Vector3 homePos;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        homePos = transform.position;
    }

    void Update()
    {
        if (cam == null) return;
        if (!GameFlow_JFM.CanDrag) return;
        if (sr != null && !sr.enabled) return;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = transform.position.z;
        Vector2 p = new Vector2(world.x, world.y);

        if (Input.GetMouseButtonDown(0))
        {
            if (col != null && col.enabled && col.OverlapPoint(p))
            {
                dragging = true;
                dragOffset = transform.position - world;
            }
        }

        if (dragging && Input.GetMouseButton(0))
        {
            transform.position = world + dragOffset;
        }

        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;

            Vector2 itemCenter = col.bounds.center;
            bool inZone = (dropZoneCollider != null) && dropZoneCollider.OverlapPoint(itemCenter);

            if (inZone)
            {
                StartCoroutine(FinishSequence());
            }
            else
            {
                transform.position = homePos;
            }
        }
    }

    IEnumerator FinishSequence()
    {
        GameFlow_JFM.LockDrag();

        if (bubbleCenter != null)
        {
            float t = 0f;
            Vector3 start = transform.position;
            while (t < snapToCenterTime)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / snapToCenterTime);
                transform.position = Vector3.Lerp(start, bubbleCenter.position, p);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.3f);

        SceneManager.LoadScene(endingSceneName);
    }
}