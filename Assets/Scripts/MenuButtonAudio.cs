using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonAudio : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        PlayHoverSound();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHoverSound();
    }

    private void PlayHoverSound()
    {
        // Mencegah bunyi tumpang-tindih saat game baru pertama kali di-load
        if (Time.timeSinceLevelLoad < 0.2f) return; 

        if (AudioManager.Instance != null && AudioManager.Instance.sfxMenu != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxMenu);
        }
    }
}