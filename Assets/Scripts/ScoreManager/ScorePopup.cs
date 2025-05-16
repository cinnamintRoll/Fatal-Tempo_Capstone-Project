using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ScorePopup : MonoBehaviour
{
    [SerializeField] private Text scoreText;

    public void SetText(string text)
    {
        scoreText.text = text;
    }

    public void setlocation(Vector3 location)
    {
        transform.position = location;
    }

    public void Delete()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        //Debug.Log(this.transform.position);
    }
}
