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
    [SerializeField] public AlbumData CurrentAlbum;

    [Header("Lock Icons")]
    [SerializeField] private GameObject previousLockedIcon;
    [SerializeField] private GameObject mainLockedIcon;
    [SerializeField] private GameObject nextLockedIcon;
    [SerializeField] private GameObject backLockedIcon;


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
        CurrentAlbum = album;
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

        int GetIndex(int idx)
        {
            return ((idx % count) + count) % count;
        }

        // Get indices
        int prevIndex = GetIndex(currentIndex - 1);
        int currIndex = GetIndex(currentIndex);
        int nextIndex = GetIndex(currentIndex + 1);
        int backIndex = GetIndex(currentIndex + 2);

        // Update images
        previousImage.sprite = songDataList[prevIndex].AlbumCover;
        mainImage.sprite = songDataList[currIndex].AlbumCover;
        nextImage.sprite = songDataList[nextIndex].AlbumCover;
        backImage.sprite = songDataList[backIndex].AlbumCover;

        // Update locked icons (using 'locked' field)
        if (previousLockedIcon) previousLockedIcon.SetActive(songDataList[prevIndex].locked);
        if (mainLockedIcon) mainLockedIcon.SetActive(songDataList[currIndex].locked);
        if (nextLockedIcon) nextLockedIcon.SetActive(songDataList[nextIndex].locked);
        if (backLockedIcon) backLockedIcon.SetActive(songDataList[backIndex].locked);
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

    private void OnEnable()
    {
        if (initialAlbum != null)
        {
            SelectAlbum(initialAlbum);
        }
    }
}
