using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AlphaHitTest : MonoBehaviour
{
    public float alphaThreshold = 0.1f;

    void Start()
    {
        // Memerintahkan Unity: "Kalau gambarnya transparan, tembusin aja kliknya!"
        GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
    }
}