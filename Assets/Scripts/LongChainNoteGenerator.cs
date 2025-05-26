#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;
using System.Collections.Generic;

[ExecuteAlways]
public class LongChainNoteGenerator : MonoBehaviour
{
    public SplineContainer splineContainer;
    public GameObject mainSegmentPrefab;
    public GameObject subSegmentPrefab;
    public GameObject endSegmentPrefab;
    public int numberOfSubSegments = 5;
    [Range(0f, 1f)] public float startSpacing = 0.1f;  // distance after main segment (t=0)
    [Range(0f, 1f)] public float endSpacing = 0.1f;    // distance before end segment (t=1)

    // Cache previous knot data to detect changes
    private List<Vector3> previousKnotPositions = new List<Vector3>();
    private List<Quaternion> previousKnotRotations = new List<Quaternion>();

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            EnsureSpline();
            CacheSplineData();
            GenerateChain();
            EditorApplication.update += EditorUpdate;
        }
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            EnsureSpline();
            CacheSplineData();
            GenerateChain();
        }
    }

    private void EditorUpdate()
    {
        if (splineContainer == null)
            return;

        if (HasSplineChanged())
        {
            CacheSplineData();
            GenerateChain();
            EditorUtility.SetDirty(this);
        }
    }

    private void CacheSplineData()
    {
        previousKnotPositions.Clear();
        previousKnotRotations.Clear();

        var spline = splineContainer.Spline;
        for (int i = 0; i < spline.Count; i++)
        {
            Vector3 worldPos = splineContainer.transform.TransformPoint(spline[i].Position);
            previousKnotPositions.Add(worldPos);

            Vector3 tangent = spline.EvaluateTangent(Mathf.Clamp01((float)i / (spline.Count - 1)));
            Quaternion rot = tangent.sqrMagnitude > 0.001f ? Quaternion.LookRotation(tangent) : Quaternion.identity;
            previousKnotRotations.Add(rot);
        }
    }

    private bool HasSplineChanged()
    {
        var spline = splineContainer.Spline;

        if (spline.Count != previousKnotPositions.Count)
            return true;

        for (int i = 0; i < spline.Count; i++)
        {
            Vector3 worldPos = splineContainer.transform.TransformPoint(spline[i].Position);

            if (Vector3.Distance(worldPos, previousKnotPositions[i]) > 0.0001f)
                return true;

            Vector3 tangent = spline.EvaluateTangent(Mathf.Clamp01((float)i / (spline.Count - 1)));
            Quaternion rot = tangent.sqrMagnitude > 0.001f ? Quaternion.LookRotation(tangent) : Quaternion.identity;

            if (Quaternion.Angle(rot, previousKnotRotations[i]) > 0.1f)
                return true;
        }
        return false;
    }

    private void EnsureSpline()
    {
        if (splineContainer == null)
        {
            GameObject splineGO = new GameObject("Spline", typeof(SplineContainer));
            splineGO.transform.SetParent(transform, false);
            splineContainer = splineGO.GetComponent<SplineContainer>();

            var spline = new Spline();
            spline.Add(new BezierKnot(new Vector3(0, 0, 0)));
            spline.Add(new BezierKnot(new Vector3(0, 0, -3)));
            splineContainer.Spline = spline;
        }
    }

    public void GenerateChain()
    {
        if (mainSegmentPrefab == null || subSegmentPrefab == null || endSegmentPrefab == null)
        {
            Debug.LogWarning("Assign all three prefabs: mainSegmentPrefab, subSegmentPrefab, endSegmentPrefab.");
            return;
        }

        if (splineContainer == null)
            return;

        Spline splineData = splineContainer.Spline;
        int totalSegments = numberOfSubSegments + 2; // main + subs + end

        // Clamp spacings so they don't overlap
        float clampedStartSpacing = Mathf.Clamp01(startSpacing);
        float clampedEndSpacing = Mathf.Clamp01(endSpacing);

        if (clampedStartSpacing + clampedEndSpacing > 0.9f)
        {
            float total = clampedStartSpacing + clampedEndSpacing;
            clampedStartSpacing = clampedStartSpacing / total * 0.9f;
            clampedEndSpacing = clampedEndSpacing / total * 0.9f;
        }

        for (int i = 0; i < totalSegments; i++)
        {
            string segmentName;
            GameObject prefab;

            if (i == 0)
            {
                segmentName = "MainSegment";
                prefab = mainSegmentPrefab;
            }
            else if (i == totalSegments - 1)
            {
                segmentName = "EndSegment";
                prefab = endSegmentPrefab;
            }
            else
            {
                segmentName = $"SubSegment_{i}";
                prefab = subSegmentPrefab;
            }

            Transform existing = transform.Find(segmentName);

            float t;

            if (i == 0)
            {
                t = 0f;
            }
            else if (i == totalSegments - 1)
            {
                t = 1f;
            }
            else
            {
                float subSegmentCount = numberOfSubSegments - 1;
                if (subSegmentCount <= 0)
                    subSegmentCount = 1;

                float segmentIndex = i - 1;
                t = Mathf.Lerp(clampedStartSpacing, 1f - clampedEndSpacing, segmentIndex / subSegmentCount);
            }

            // Evaluate local position and tangent relative to splineContainer transform
            Vector3 localPos = splineData.EvaluatePosition(t);
            Vector3 localTangent = splineData.EvaluateTangent(t);
            Quaternion localRot = localTangent.sqrMagnitude > 0.001f ? Quaternion.LookRotation(localTangent) : Quaternion.identity;

            if (existing == null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.SetParent(transform);
                instance.name = segmentName;
                // Set local position and rotation relative to this GameObject
                instance.transform.localPosition = localPos;
                instance.transform.localRotation = localRot;
            }
            else
            {
                existing.localPosition = localPos;
                existing.localRotation = localRot;
            }
        }

        // Cleanup any extra segments
        var toKeep = new HashSet<string>();
        toKeep.Add("MainSegment");
        toKeep.Add("EndSegment");
        for (int i = 1; i <= numberOfSubSegments; i++)
            toKeep.Add($"SubSegment_{i}");

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<SplineContainer>() != null)
                continue;

            if (!toKeep.Contains(child.name))
            {
                var destroyTarget = child.gameObject;
                EditorApplication.delayCall += () =>
                {
                    if (destroyTarget != null)
                        DestroyImmediate(destroyTarget);
                };
            }
        }
    }
}
#endif
