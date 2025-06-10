using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class RotatingLevelSelector : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator selectorAnimator;

    [Header("UI Images")]
    [SerializeField] private Image previousImage;
    [SerializeField] private Image mainImage;
    [SerializeField] private Image nextImage;
    [SerializeField] private Image backImage;

    [Header("Album Selection")]
    [SerializeField] private AlbumData initialAlbum;

    private List<SongData> songDataList = new List<SongData>();
    private int currentIndex = 0;
    private int queuedDirection = 0;
    private bool isRotating = false;

    [Header("Currently Selected Song")]
    public SongData selectedSong;

    [Header("Events")]
    public UnityEvent onSongSelected;

    [ContextMenu("Rotate Left")]
    public void TestRotateLeft() => RotateLeft();

    [ContextMenu("Rotate Right")]
    public void TestRotateRight() => RotateRight();

    public void SelectAlbum(AlbumData album)
    {
        if (album == null || album.songs == null || album.songs.Count == 0)
        {
            Debug.LogWarning("Album is null or contains no songs.");
            return;
        }

        songDataList = album.songs;
        currentIndex = 0;
        selectedSong = songDataList[currentIndex];
        onSongSelected.Invoke();

        UpdateUIImages();
    }

    public void RotateLeft()
    {
        if (isRotating || songDataList.Count == 0) return;

        isRotating = true;
        queuedDirection = 1;
        selectorAnimator.SetTrigger("RotateLeft");
    }

    public void RotateRight()
    {
        if (isRotating || songDataList.Count == 0) return;

        isRotating = true;
        queuedDirection = -1;
        selectorAnimator.SetTrigger("RotateRight");
    }

    public void OnRotationComplete()
    {
        int count = songDataList.Count;
        if (count == 0) return;

        currentIndex = (currentIndex + queuedDirection + count) % count;
        selectedSong = songDataList[currentIndex];
        onSongSelected.Invoke();

        UpdateUIImages();
        isRotating = false;
    }

    private void UpdateUIImages()
    {
        int count = songDataList.Count;
        if (count == 0) return;

        // Helper local function to get song index safely and wrap around
        int GetIndex(int idx)
        {
            // Just wrap index into 0..count-1 range
            return ((idx % count) + count) % count;
        }

        // For fewer than 4 songs, fill duplicates as needed:
        // Use the available songs cyclically for the four images
        previousImage.sprite = songDataList[GetIndex(currentIndex - 1)].AlbumCover;
        mainImage.sprite = songDataList[GetIndex(currentIndex)].AlbumCover;
        nextImage.sprite = songDataList[GetIndex(currentIndex + 1)].AlbumCover;
        backImage.sprite = songDataList[GetIndex(currentIndex + 2)].AlbumCover;
    }

    public void SelectAlbumSong(SongData targetSong)
    {
        if (targetSong == null)
        {
            Debug.LogWarning("Target song is null.");
            return;
        }

        var albumDB = AlbumDatabase.Instance;
        if (albumDB == null || albumDB.allAlbums == null)
        {
            Debug.LogWarning("Album database not found in scene.");
            return;
        }

        foreach (AlbumData album in albumDB.allAlbums)
        {
            int index = album.songs.IndexOf(targetSong);
            if (index != -1)
            {
                SelectAlbum(album);
                currentIndex = index;
                selectedSong = targetSong;
                onSongSelected.Invoke();
                UpdateUIImages();
                return;
            }
        }

        Debug.LogWarning("Target song not found in any album.");
    }

    public void SelectSong()
    {
        StartCoroutine(SelectSongCoroutine(0.3f));
    }

    private IEnumerator SelectSongCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (songDataList.Count == 0) yield break;
        selectedSong = songDataList[currentIndex];
        onSongSelected.Invoke();
    }

    private void Start()
    {
        if (initialAlbum != null)
        {
            SelectAlbum(initialAlbum);
        }
    }
}
