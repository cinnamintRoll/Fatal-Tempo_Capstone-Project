using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using System.Net;
using UnityEngine.Windows;
using BNG;
using Unity.VisualScripting;

public class SliceObject : MonoBehaviour
{
    [SerializeField] private Collector _collector;
    public Transform bladeStart;
    public Transform bladeEnd;
    public float minSliceSpeed = 1.5f;
    public VelocityEstimator velocityEstimator;
    private InputBridge input;
 
    public Material crossSectionMaterial;

    public BNG.ControllerHand HandSide = ControllerHand.Right;

    public float cutForce = 2000f;

    public LayerMask sliceableLayer;

    public float rayExtension = 0.01f;

    // Slice sound effect
    public AudioClip sliceSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SoundPlayer soundPlayer;
    private Vector3 previousBladeStart;
    private Vector3 previousBladeEnd;
    public float bladeRadius = 0.01f;
    // Range for random pitch
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;
    private bool isEmulator;
    void Start()
    {
        if(audioSource == null)
        audioSource = gameObject.AddComponent<AudioSource>();

        previousBladeStart = bladeStart.position;
        previousBladeEnd = bladeEnd.position;
        isEmulator = !GameManager.Instance.VREmulator.HMDIsActive;
    }

    private void Awake()
    {
        input = InputBridge.Instance;
    }

    void FixedUpdate()
    {
        Debug.DrawLine(previousBladeStart, previousBladeEnd, Color.green);
        Debug.DrawLine(bladeStart.position, bladeEnd.position, Color.red);
        Debug.DrawLine(previousBladeStart, previousBladeEnd, Color.green);
        Debug.DrawLine(bladeStart.position, bladeEnd.position, Color.red);
        Vector3 currentStart = bladeStart.position;
        Vector3 currentEnd = bladeEnd.position;

        Vector3 previousStart = previousBladeStart;
        Vector3 previousEnd = previousBladeEnd;

        // Midpoint movement for direction and distance
        Vector3 movementDirection = ((currentStart + currentEnd) * 0.5f) - ((previousStart + previousEnd) * 0.5f);
        float movementDistance = movementDirection.magnitude;

        if (movementDistance > 0f)
        {
            Vector3 directionNormalized = movementDirection.normalized;

            // CapsuleCast from previous blade line to current blade line
            if (Physics.CapsuleCast(previousStart, previousEnd, bladeRadius, directionNormalized,
                out RaycastHit hit, movementDistance + rayExtension, sliceableLayer))
            {
                GameObject sliceableObject = hit.collider.gameObject;
                StartCoroutine(SliceAsync(sliceableObject));
            }
        }

        previousBladeStart = currentStart;
        previousBladeEnd = currentEnd;
    }



    IEnumerator SliceAsync(GameObject sliceableObject)
    {
        GameObject originalobject = sliceableObject;
        if (sliceableObject != null)
        {
            Vector3 swordVelocity = Vector3.zero;

            if (isEmulator)
            {
                swordVelocity = velocityEstimator.GetVelocityEstimate();
                velocityEstimator.enabled = false;
            }
            else
            {
                swordVelocity = InputBridge.Instance.GetControllerVelocity(HandSide);
            }
            

            float speed = swordVelocity.magnitude;

            if (speed < minSliceSpeed)
            {
                yield break; // Too slow to slice
            }


            SliceSegment segment = sliceableObject.GetComponent<SliceSegment>();
            if (segment != null)
            {
                if(_collector)
                _collector.CollectItem(segment.SegmentValue);
            }

            SkinnedMeshRenderer skinnedRenderer = sliceableObject.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedRenderer != null)
            {
                Mesh bakedMesh = new Mesh();
                bakedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                skinnedRenderer.BakeMesh(bakedMesh);
                bakedMesh.RecalculateNormals();
                bakedMesh.RecalculateBounds();

                GameObject bakedObject = new GameObject(sliceableObject.name + "_Baked");
                bakedObject.transform.SetPositionAndRotation(skinnedRenderer.transform.position, skinnedRenderer.transform.rotation);
                bakedObject.transform.localScale = skinnedRenderer.transform.localScale;

                MeshFilter meshFilter = bakedObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = bakedMesh;

                MeshRenderer meshRenderer = bakedObject.AddComponent<MeshRenderer>();
                meshRenderer.materials = skinnedRenderer.sharedMaterials;
                bakedObject.layer = sliceableObject.layer;
                bakedObject.transform.parent = null; 

                sliceableObject.SetActive(false);
                sliceableObject = bakedObject;
            }
        }
        yield return new WaitForEndOfFrame(); 
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(bladeEnd.position - bladeStart.position, velocity);
        planeNormal.Normalize();

        SlicedHull slicedObject = null;
        if (sliceableObject != null)
        {
            slicedObject = sliceableObject.Slice(bladeEnd.position, planeNormal, crossSectionMaterial);
        }

        if (slicedObject != null)
        {
            input.VibrateController(0.5f, 1f, 0.05f, HandSide);

            PlaySliceSound();

            GameObject upperHull = slicedObject.CreateUpperHull(sliceableObject, crossSectionMaterial);
            GameObject lowerHull = slicedObject.CreateLowerHull(sliceableObject, crossSectionMaterial);

            if (upperHull == null || lowerHull == null)
            {
                Debug.LogWarning("Sliced hulls were null.");
                yield break;
            }

            Debug.Log("Upper Hull has renderer: " + upperHull.GetComponent<MeshRenderer>());
            Debug.Log("Lower Hull has renderer: " + lowerHull.GetComponent<MeshRenderer>());

            upperHull.AddComponent<DespawnAfterSlice>();
            lowerHull.AddComponent<DespawnAfterSlice>();

            SetupHullObject(upperHull, sliceableObject);
            SetupHullObject(lowerHull, sliceableObject);

            Destroy(originalobject);
            Destroy(sliceableObject);
        }
        else
        {
            if (sliceableObject != null)
            {
                Debug.LogWarning($"Slicing failed for {sliceableObject.name}");
            }
            else
            {
                Debug.LogWarning($"Slicing failed for object");
            }
        }

    }


    private void SetupHullObject(GameObject hull, GameObject originalObject)
    {
        hull.transform.SetPositionAndRotation(originalObject.transform.position, originalObject.transform.rotation);
        hull.transform.localScale = originalObject.transform.localScale;

        try
        {
            MeshFilter mf = hull.GetComponent<MeshFilter>();
            Mesh mesh = mf.sharedMesh;
            int triangleCount = mesh.triangles.Length / 3;

            if (triangleCount <= 255)
            {
                MeshCollider meshCol = hull.AddComponent<MeshCollider>();
                meshCol.sharedMesh = mesh;
                meshCol.convex = true;
            }
            else
            {
                Debug.LogWarning($"Mesh too complex for convex collider ({triangleCount} triangles). Using fitted BoxCollider.");
                AddFittedBoxCollider(hull, mesh);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Convex MeshCollider failed on {hull.name}. Falling back to fitted BoxCollider.\n{ex.Message}");
            Destroy(hull.GetComponent<MeshCollider>());

            Mesh mesh = hull.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh != null)
                AddFittedBoxCollider(hull, mesh);
            else
                hull.AddComponent<BoxCollider>(); // fallback if mesh missing
        }

        Rigidbody rb = hull.AddComponent<Rigidbody>();   

        rb.AddExplosionForce(cutForce, originalObject.transform.position, 1f);
    }
    private void PlaySliceSound()
    {
        if (audioSource == null)
            return;

        audioSource.pitch = Random.Range(minPitch, maxPitch);

        if (soundPlayer != null)
        {
            soundPlayer.PlayRandomSound();
            return;   
        }
   
        if(sliceSound != null)
        audioSource.PlayOneShot(sliceSound);
    }

    private void AddFittedBoxCollider(GameObject obj, Mesh mesh)
    {
        BoxCollider box = obj.AddComponent<BoxCollider>();

        // Get local bounds of the mesh
        Bounds bounds = mesh.bounds;

        // Apply mesh bounds to the BoxCollider in local space
        box.center = bounds.center;
        box.size = bounds.size;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (bladeStart != null && bladeEnd != null)
        {
            Gizmos.color = Color.cyan;

            Vector3 start = bladeStart.position;
            Vector3 end = bladeEnd.position;

            // Draw spheres at each end to represent the capsule ends
            Gizmos.DrawWireSphere(start, bladeRadius);
            Gizmos.DrawWireSphere(end, bladeRadius);

            // Approximate capsule sides by drawing radial lines between the spheres
            Vector3 dir = (end - start).normalized;
            Vector3 up = Vector3.up;
            if (Vector3.Dot(dir, up) > 0.99f) // If aligned with up, choose another axis
            {
                up = Vector3.right;
            }

            up = Vector3.Cross(dir, up).normalized * bladeRadius;

            int segments = 12;
            for (int i = 0; i < segments; i++)
            {
                float angle = (360f / segments) * i;
                Quaternion rot = Quaternion.AngleAxis(angle, dir);
                Vector3 offset = rot * up;

                Gizmos.DrawLine(start + offset, end + offset);
            }
        }
    }
#endif

}
