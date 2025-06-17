using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeatIndicator : MonoBehaviour
{
    [Header("Beat Visual Settings")]
    [SerializeField] private GameObject halfCirclePrefab;
    [SerializeField] private int poolSize = 20;
    [SerializeField] private float startSpacing = 200f;
    [SerializeField] private float endSpacing = 0f;
    [SerializeField] private Transform spawnParent;
    [SerializeField] private int beatsVisibleAhead = 4;
    [SerializeField] private float EarlyLateThreshold = 0.04f;
    [Range(0f, 1f)]
    [SerializeField] private float alphaStartThreshold = 0.25f;
    [SerializeField] private float lingerFadeDuration = 0.5f;
    [SerializeField] private float indicationFadeDuration = 1f;
    [SerializeField] private float indicationDelay = 0.2f;
    [SerializeField, Tooltip("Perfect timing window in milliseconds")]
    private float perfectWindow = 100f; // e.g., ±50 ms is perfect timing

    [SerializeField, Tooltip("Max timing window in milliseconds for scoring/indication")]
    private float maxWindow = 300f;
    [Header("Timing Colors")]
    [SerializeField] private Color tooEarlyColor = Color.red;
    [SerializeField] private Color perfectColor = Color.green;
    [SerializeField] private Color lateColor = Color.yellow;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<BeatVisual> activeVisuals = new List<BeatVisual>();

    private AudioSource music => MusicManager.Instance?.musicSource;
    private float secondsPerBeat => MusicManager.Instance?.secondsPerBeat ?? 0.5f;
    private float offset => MusicManager.Instance?.musicOffset ?? 0f;

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject half = Instantiate(halfCirclePrefab, spawnParent);
            SetAlpha(half, 0f);
            ResetColor(half);
            half.SetActive(false);
            pool.Enqueue(half);
        }

        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnKillEnemy.AddListener(OnKillEnemyHandler);
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.OnIntervalPassed.AddListener(SyncBeatVisuals);
        }

    }

    private void OnDestroy()
    {
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnKillEnemy.RemoveListener(OnKillEnemyHandler);
        }
    }

    private void SyncBeatVisuals()
    {
        float currentTime = music.time + offset;
        float currentBeat = currentTime / secondsPerBeat;

        foreach (var vis in activeVisuals)
        {
            if (vis.isStopped) continue;

            float beatTime = vis.beatTime;
            float t = Mathf.InverseLerp(vis.spawnTime, beatTime, currentTime);
            t = Mathf.Clamp01(t);

            // Adjust positions based on the perfect timing interval
            vis.obj.transform.localPosition = Vector3.Lerp(vis.startPos, vis.endPos, t);
        }
    }

    private void OnKillEnemyHandler(Vector3 _)
    {
        if (music == null || !music.isPlaying) return;

        float currentTime = music.time + offset;
        float currentIntervalSec = secondsPerBeat;
        float timeSinceLastPulse = currentTime % currentIntervalSec;

        float rawOffset = timeSinceLastPulse;
        if (rawOffset > currentIntervalSec / 2f)
            rawOffset -= currentIntervalSec;

        float distanceSec = rawOffset;
        float absDistanceSec = Mathf.Abs(distanceSec);

        float beatIndex = Mathf.Round(currentTime / secondsPerBeat);
        float closestBeatTime = beatIndex * secondsPerBeat;

        BeatVisual left = null, right = null;
        for (int i = 0; i < activeVisuals.Count; i++)
        {
            var vis = activeVisuals[i];
            if (vis.isStopped || !Mathf.Approximately(vis.beatTime, closestBeatTime)) continue;

            if (vis.obj.transform.localRotation.eulerAngles.y == 180f)
                left = vis;
            else if (vis.obj.transform.localRotation.eulerAngles.y == 0f)
                right = vis;

            if (left != null && right != null) break;
        }

        if (left == null || right == null) return;

        float distanceMS = distanceSec * 1000f;

        Color color;

        if (Mathf.Abs(distanceMS) <= perfectWindow)
        {
            color = perfectColor;
        }
        else if (distanceMS < -perfectWindow)
        {
            // Too early, beyond perfect window but within maxWindow
            color = tooEarlyColor;
        }
        else if (distanceMS > perfectWindow)
        {
            // Too late, beyond perfect window but within maxWindow
            color = lateColor;
        }
        else
        {
            // Optional fallback, but normally covered above
            color = perfectColor;
        }

        SetColor(left.obj, color);
        SetColor(right.obj, color);
        StartCoroutine(ShowIndicationCoroutine(left, right));
    }

    private void Update()
    {
        if (music == null || !music.isPlaying) return;

        float currentTime = music.time + offset;
        float currentBeat = currentTime / secondsPerBeat;

        for (int i = 0; i < beatsVisibleAhead; i++)
        {
            int beatToSpawn = Mathf.FloorToInt(currentBeat) + i;
            float beatTime = beatToSpawn * secondsPerBeat;

            if (!activeVisuals.Exists(v => Mathf.Approximately(v.beatTime, beatTime)))
                SpawnMovingHalves(beatTime);
        }

        for (int i = activeVisuals.Count - 1; i >= 0; i--)
        {
            BeatVisual vis = activeVisuals[i];
            if (vis.isStopped) continue;

            float t = Mathf.InverseLerp(vis.spawnTime, vis.beatTime, currentTime);
            t = Mathf.Clamp01(t);
            vis.obj.transform.localPosition = Vector3.Lerp(vis.startPos, vis.endPos, t);

            Image img = vis.obj.GetComponentInChildren<Image>();
            if (img != null)
            {
                float alpha = t < 1f
                    ? Mathf.Clamp01(Mathf.InverseLerp(alphaStartThreshold, 1f, t))
                    : Mathf.Clamp01(1f - ((currentTime - vis.beatTime) / lingerFadeDuration));

                img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);

                if (alpha <= 0f && t >= 1f)
                {
                    vis.obj.SetActive(false);
                    pool.Enqueue(vis.obj);
                    activeVisuals.RemoveAt(i);
                }
            }
        }
    }

    void SpawnMovingHalves(float beatTime)
    {
        float moveDuration = secondsPerBeat * beatsVisibleAhead;
        float spawnTime = beatTime - moveDuration;
        float currentTime = music.time + offset;
        if (currentTime > beatTime) return;

        GameObject left = GetFromPool();
        GameObject right = GetFromPool();
        if (left == null || right == null) return;

        left.SetActive(true);
        right.SetActive(true);
        SetAlpha(left, 0f);
        SetAlpha(right, 0f);
        ResetColor(left);
        ResetColor(right);

        left.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        right.transform.localRotation = Quaternion.identity;
        left.transform.localScale = right.transform.localScale = Vector3.one;

        Vector3 leftStart = new Vector3(-startSpacing, 0f, 0f);
        Vector3 leftEnd = new Vector3(-endSpacing * 0.5f, 0f, 0f);
        Vector3 rightStart = new Vector3(startSpacing, 0f, 0f);
        Vector3 rightEnd = new Vector3(endSpacing * 0.5f, 0f, 0f);

        left.transform.localPosition = leftStart;
        right.transform.localPosition = rightStart;

        activeVisuals.Add(new BeatVisual(left, spawnTime, beatTime, leftStart, leftEnd));
        activeVisuals.Add(new BeatVisual(right, spawnTime, beatTime, rightStart, rightEnd));
    }

    GameObject GetFromPool()
    {
        return pool.Count > 0 ? pool.Dequeue() : null;
    }

    private void SetAlpha(GameObject obj, float alpha)
    {
        var img = obj.GetComponentInChildren<Image>();
        if (img != null) img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
    }

    private void ResetColor(GameObject obj)
    {
        var img = obj.GetComponentInChildren<Image>();
        if (img != null) img.color = Color.white;
    }

    private void SetColor(GameObject obj, Color color)
    {
        var img = obj.GetComponentInChildren<Image>();
        if (img != null) img.color = new Color(color.r, color.g, color.b, img.color.a);
    }

    private IEnumerator ShowIndicationCoroutine(BeatVisual left, BeatVisual right)
    {
        left.isStopped = true;
        right.isStopped = true;

        yield return new WaitForSeconds(indicationDelay);

        float timer = 0f;
        while (timer < indicationFadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / indicationFadeDuration);
            SetAlpha(left.obj, alpha);
            SetAlpha(right.obj, alpha);
            yield return null;
        }

        SetAlpha(left.obj, 0f);
        SetAlpha(right.obj, 0f);
        left.obj.SetActive(false);
        right.obj.SetActive(false);

        activeVisuals.Remove(left);
        activeVisuals.Remove(right);
        pool.Enqueue(left.obj);
        pool.Enqueue(right.obj);
    }

    private class BeatVisual
    {
        public GameObject obj;
        public float spawnTime;
        public float beatTime;
        public Vector3 startPos;
        public Vector3 endPos;
        public bool isStopped = false;

        public BeatVisual(GameObject obj, float spawnTime, float beatTime, Vector3 startPos, Vector3 endPos)
        {
            this.obj = obj;
            this.spawnTime = spawnTime;
            this.beatTime = beatTime;
            this.startPos = startPos;
            this.endPos = endPos;
        }
    }
}
