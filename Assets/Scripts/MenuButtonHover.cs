using UnityEngine;
using UnityEngine.EventSystems;

// Menambahkan IPointerExitHandler untuk mendeteksi saat mouse pergi
public class MenuButtonAudio : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Tarik objek 'Arrow' (panah) milik tombol ini ke kolom ini")]
    public GameObject cursorArrow;

    private void Start()
    {
        if (cursorArrow != null) cursorArrow.SetActive(false);
    }

    // Terpanggil saat tombol DIPILIH (Keyboard atau Mouse masuk)
    public void OnSelect(BaseEventData eventData)
    {
        ShowCursor();
    }

    // Terpanggil saat PINDAH ke tombol lain ATAU status pilihan dihapus
    public void OnDeselect(BaseEventData eventData)
    {
        HideCursor();
    }

    // Terpanggil saat MOUSE MENYOROT tombol
    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(this.gameObject);
    }

    // --- FITUR BARU: Terpanggil saat MOUSE PERGI meninggalkan tombol ---
    public void OnPointerExit(PointerEventData eventData)
    {
        // Jika mouse pergi, kita hapus status "Terpilih" dari EventSystem
        // Ini akan otomatis memicu fungsi OnDeselect() di atas dan menyembunyikan panah
        if (EventSystem.current.currentSelectedGameObject == this.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void ShowCursor()
    {
        if (cursorArrow != null) cursorArrow.SetActive(true);

        if (Time.timeSinceLevelLoad > 0.2f && AudioManager.Instance != null && AudioManager.Instance.sfxMenu != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxMenu);
        }
    }

    private void HideCursor()
    {
        if (cursorArrow != null) cursorArrow.SetActive(false);
    }
}