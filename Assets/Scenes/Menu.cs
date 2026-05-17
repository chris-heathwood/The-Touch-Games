using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Assign row GameObjects in order in the Inspector
    public Transform[] rows;

    // Scene to load for each row — must match rows array length and order
    public string[] sceneNames;

    // Which row is centred at startup (0 = first row)
    public int startRow = 0;

    // Carousel tuning
    public float rowHeight = 3f;
    public float selectedScale = 1.2f;
    public float adjacentScale = 0.9f;
    public float farScale = 0.6f;
    public float lerpSpeed = 8f;
    public float multiButtonRowWidth = 6f; // total width of a comma-separated row

    // Fade
    public CanvasGroup fadeOverlay;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.3f;

    // Scroll state
    private float scrollOffset;
    private float targetOffset;
    private bool dragging;
    private float dragStartY;
    private float scrollAtDragStart;
    private float maxDragDistance;
    private const float tapThreshold = 0.2f;

    // Trackpad scroll (editor only)
    private bool wasScrolling;
    private float scrollStopTimer;
    private const float scrollStopDelay = 0.15f;

    // Fade state
    private float fadeTimer;
    private bool fadingOut;
    private float fadeOutTimer;
    private string pendingScene;

    private bool runningInEditor;

    void Start()
    {
        if (Application.isEditor) runningInEditor = true;
        scrollOffset = startRow;
        targetOffset = startRow;
        if (fadeOverlay != null) fadeOverlay.alpha = 1f;
    }

    Vector2 InputPos()
    {
        Vector2 screen = runningInEditor ? (Vector2)Input.mousePosition : Input.GetTouch(0).position;
        return Camera.main.ScreenToWorldPoint(screen);
    }

    bool InputBegan()
    {
        if (runningInEditor) return Input.GetMouseButtonDown(0);
        return Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began;
    }

    bool InputHeld()
    {
        if (runningInEditor) return Input.GetMouseButton(0);
        return Input.touchCount == 1;
    }

    bool InputEnded()
    {
        if (runningInEditor) return Input.GetMouseButtonUp(0);
        return Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended;
    }

    void Update()
    {
        // Fade in on load
        if (fadeOverlay != null && !fadingOut && fadeOverlay.alpha > 0f)
        {
            fadeTimer += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Clamp01(1f - fadeTimer / fadeInDuration);
        }

        // Fade out then load scene
        if (fadingOut)
        {
            fadeOutTimer += Time.deltaTime;
            if (fadeOverlay != null)
                fadeOverlay.alpha = Mathf.Clamp01(fadeOutTimer / fadeOutDuration);
            if (fadeOutTimer >= fadeOutDuration)
                SceneManager.LoadScene(pendingScene);
            return;
        }

        HandleTrackpadScroll();
        HandleTouchDrag();

        // Smooth scroll
        scrollOffset = Mathf.Lerp(scrollOffset, targetOffset, lerpSpeed * Time.deltaTime);

        // Position and scale rows
        for (int i = 0; i < rows.Length; i++)
        {
            float dist = i - scrollOffset;
            rows[i].position = new Vector3(0f, -dist * rowHeight, 0f);

            float absDist = Mathf.Abs(dist);
            float scale;
            if (absDist <= 0.5f)
                scale = Mathf.Lerp(selectedScale, adjacentScale, absDist * 2f);
            else
                scale = Mathf.Lerp(adjacentScale, farScale, Mathf.Clamp01(absDist - 0.5f));

            rows[i].localScale = Vector3.one * scale;
        }
    }

    void HandleTrackpadScroll()
    {
        if (!runningInEditor) return;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetOffset = Mathf.Clamp(targetOffset + scroll * 0.3f, 0f, rows.Length - 1);
            scrollStopTimer = 0f;
            wasScrolling = true;
        }
        else if (wasScrolling)
        {
            scrollStopTimer += Time.deltaTime;
            if (scrollStopTimer >= scrollStopDelay)
            {
                targetOffset = Mathf.Clamp(Mathf.Round(targetOffset), 0f, rows.Length - 1);
                wasScrolling = false;
            }
        }
    }

    void HandleTouchDrag()
    {
        if (InputBegan())
        {
            dragging = true;
            dragStartY = InputPos().y;
            scrollAtDragStart = targetOffset;
            maxDragDistance = 0f;
        }

        if (dragging && InputHeld())
        {
            float delta = InputPos().y - dragStartY;
            maxDragDistance = Mathf.Max(maxDragDistance, Mathf.Abs(delta));
            float scrollDir = runningInEditor ? -1f : 1f;
            targetOffset = Mathf.Clamp(scrollAtDragStart + scrollDir * delta / rowHeight, 0f, rows.Length - 1);
        }

        if (InputEnded())
        {
            dragging = false;
            if (maxDragDistance < tapThreshold)
                HandleTap(InputPos());
            else
                targetOffset = Mathf.Clamp(Mathf.Round(targetOffset), 0f, rows.Length - 1);
        }
    }

    void HandleTap(Vector2 worldPoint)
    {
        // Find which row is closest to the tap
        int tappedRow = 0;
        float minDist = float.MaxValue;
        for (int i = 0; i < rows.Length; i++)
        {
            float dist = Mathf.Abs(rows[i].position.y - worldPoint.y);
            if (dist < minDist) { minDist = dist; tappedRow = i; }
        }

        int centredRow = Mathf.RoundToInt(targetOffset);
        if (tappedRow == centredRow)
        {
            if (tappedRow >= sceneNames.Length || string.IsNullOrEmpty(sceneNames[tappedRow])) return;

            string[] subScenes = sceneNames[tappedRow].Split(',');
            if (subScenes.Length > 1)
            {
                // Use tap X position to pick which sub-button was hit
                float relX = worldPoint.x - rows[tappedRow].position.x;
                float sectionWidth = multiButtonRowWidth / subScenes.Length;
                float leftEdge = -(multiButtonRowWidth / 2f);
                int subIndex = Mathf.Clamp(Mathf.FloorToInt((relX - leftEdge) / sectionWidth), 0, subScenes.Length - 1);
                GoTo(subScenes[subIndex].Trim());
            }
            else
            {
                GoTo(sceneNames[tappedRow].Trim());
            }
        }
        else
        {
            // Scroll to the tapped row
            targetOffset = tappedRow;
        }
    }

    void GoTo(string scene)
    {
        pendingScene = scene;
        fadingOut = true;
        fadeOutTimer = 0f;
    }
}
