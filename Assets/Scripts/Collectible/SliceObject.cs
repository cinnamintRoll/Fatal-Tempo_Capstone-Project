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

    // Range for random pitch
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    void Start()
    {
        if(audioSource == null)
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Awake()
    {
        input = InputBridge.Instance;
    }

        void FixedUpdate()
    {
        Debug.DrawLine(bladeStart.position, bladeEnd.position, Color.red);

        if (Physics.Linecast(bladeStart.position, bladeEnd.position, out RaycastHit hit, sliceableLayer))
        {
            GameObject sliceableObject = hit.collider.gameObject;
            StartCoroutine(SliceAsync(sliceableObject));
        }
    }

    IEnumerator SliceAsync(GameObject sliceableObject)
    {
        GameObject originalobject = sliceableObject;
        if (sliceableObject != null)
        {
            Vector3 swordVelocity = velocityEstimator.GetVelocityEstimate();
            float speed = swordVelocity.magnitude;

            if (speed < minSliceSpeed)
            {
                yield break; 
            }

            SliceSegment segment = sliceableObject.GetComponent<SliceSegment>();
            if (segment != null)
            {
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

        hull.AddComponent<MeshCollider>().convex = true;  
        Rigidbody rb = hull.AddComponent<Rigidbody>();   

        rb.AddExplosionForce(cutForce, originalObject.transform.position, 1f);
    }
    private void PlaySliceSound()
    {
        if (sliceSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);

            audioSource.PlayOneShot(sliceSound);
        }
    }
}
