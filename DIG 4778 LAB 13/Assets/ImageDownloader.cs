using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ImageDownloader : MonoBehaviour
{
    [Header("Image URLs")]
    public List<string> imageUrls = new List<string>();
    [Header("Downloaded & Stored Images")]
    public List<Texture2D> downloadedImages = new List<Texture2D>();

    public event Action OnDownloadsComplete;

    public bool IsDownloadComplete;

    void Start()
    {
        StartCoroutine(DownloadAllImages());
    }

    private IEnumerator DownloadAllImages()
    {
        downloadedImages.Clear();
        IsDownloadComplete = false;

        foreach (string url in imageUrls)
        {
            // Yield until each image finishes downloading before moving to the next
            yield return StartCoroutine(GetWebImage(url, (texture) =>
            {
                downloadedImages.Add(texture);
            }));
        }
        IsDownloadComplete = true;
        Debug.Log($"All {downloadedImages.Count} images downloaded and stored.");

        // Notify all listening billboards that the images are ready
        OnDownloadsComplete?.Invoke();
    }
    // Checks if the image is already stored in downloadedImages before downloading
    public void GetWebImage(int index, Action<Texture2D> callback)
    {
        if (index >= 0 && index < downloadedImages.Count && downloadedImages[index] != null)
        {
            // Return the already-stored image without downloading again
            callback?.Invoke(downloadedImages[index]);
        }
        else if (index >= 0 && index < imageUrls.Count)
        {
            // Fallback: download it if not stored yet
            StartCoroutine(GetWebImage(imageUrls[index], callback));
        }
    }

    private IEnumerator GetWebImage(string url, Action<Texture2D> callback)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Failed to download {url}: {request.error}");
                callback?.Invoke(null);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                callback?.Invoke(texture);
            }
        }
    }
}
