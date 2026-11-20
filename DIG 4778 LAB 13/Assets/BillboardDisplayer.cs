using UnityEngine;
public class BillboardDisplayer : MonoBehaviour
{
    [Header("References")]
    public ImageDownloader imageDownloader;

    [Header("Selection")]
    [Tooltip("Index of the image in ImageDownloader's list (0, 1, 2...)")]
    public int imageIndex = 0; 

    private Renderer objectRenderer; // Reference the Renderer on the object

    private void Awake()
    {
        // Pull the Renderer on awake
        objectRenderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        if (imageDownloader == null) return;

        // If downloads already finished display immediately
        if (imageDownloader.IsDownloadComplete)
        {
            DisplaySelectedImage();
        }
        else
        {
            // Subscribe so this method is invoked once downloads complete
            imageDownloader.OnDownloadsComplete += DisplaySelectedImage;
        }
    }

    private void OnDisable()
    {
        if (imageDownloader != null)
        {
            // Unsubscribe when the object is disabled
            imageDownloader.OnDownloadsComplete -= DisplaySelectedImage;
        }
    }

    public void DisplaySelectedImage()
    {
        if (imageIndex < 0 || imageIndex >= imageDownloader.downloadedImages.Count)
        {
            Debug.LogWarning($"Invalid imageIndex {imageIndex} on {gameObject.name}");
            return;
        }

        // Pull the stored texture via GetWebImage (no extra network call)
        imageDownloader.GetWebImage(imageIndex, (texture) =>
        {
            if (texture != null)
            {
                objectRenderer.material.mainTexture = texture;
            }
        });
    }
}