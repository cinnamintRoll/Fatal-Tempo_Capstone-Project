using UnityEngine;

public class PopupRatingDisplay : MonoBehaviour
{
    [Header("Rating Objects")]
    [SerializeField] private GameObject greatObject;
    [SerializeField] private GameObject perfectObject;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string fadeInTrigger = "Play";


    private void Start()
    {
        
    }

    public void Play()
    {
        PlayAnimation(fadeInTrigger);
    }

    public void ShowRating(string rating)
    {
        if (greatObject != null) greatObject.SetActive(false);
        if (perfectObject != null) perfectObject.SetActive(false);

        switch (rating.ToLower())
        {
            case "great":
                if (greatObject != null) greatObject.SetActive(true);
                break;
            case "perfect":
                if (perfectObject != null) perfectObject.SetActive(true);
                break;
        }
    }

    public void PlayAnimation(string trigger)
    {
        if (animator != null && !string.IsNullOrEmpty(trigger))
        {
            animator.SetTrigger(trigger);
        }
    }

    public void SetLocation(Vector3 location)
    {
        transform.position = location;
    }

    public void Delete()
    {
        Destroy(gameObject);
    }
}
