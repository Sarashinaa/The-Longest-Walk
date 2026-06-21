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

    // --- FITUR BARU: Sapu Jagat ---
    // Dipanggil otomatis oleh Unity saat tombol/panel ini dimatikan (SetActive(false))
    // Ini mencegah bug panah nyangkut saat pindah menu!
    private void OnDisable()
    {
        HideCursor();
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

    // Terpanggil saat MOUSE PERGI meninggalkan tombol
    public void OnPointerExit(PointerEventData eventData)
    {
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
            // Mencegah error kalau audio dimainkan saat objek sedang mati
            if (gameObject.activeInHierarchy)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxMenu);
            }
        }
    }

    private void HideCursor()
    {
        if (cursorArrow != null) cursorArrow.SetActive(false);
    }
}