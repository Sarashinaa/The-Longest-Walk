
# The Longest Walk

The Longest Walk adalah prototipe naratif dan eksplorasi, sebuah perjalanan singkat melintasi koridor-koridor kenangan di mana pemain bertemu NPC dengan kisah-kisah kecil yang mengungkap suasana dunia. https://sarashinaa.itch.io/the-longest-walk

## Fitur utama

- Cari Anomali: amati, cermati, ingat semua environment.
- NPC dengan fragmen cerita: interaksi singkat yang menambah konteks dunia.
- Atmosfer audio-visual: musik latar dan efek lingkungan untuk membangun suasana.

## Pengalaman yang diharapkan

Pemain diundang untuk bergerak perlahan, mengamati, dan mendengarkan, mengumpulkan potongan-potongan cerita alih-alih menyelesaikan misi tradisional. Game ini cocok untuk eksperimen naratif dan pembuatan prototipe suasana.

## Cara membuka proyek (singkat)

1. Buka Unity Hub dan tambahkan folder proyek ini (folder yang berisi `Assets/` dan `ProjectSettings/`).
2. Buka dengan Unity Editor (direkomendasikan 2022.3.62f3).

## Kontrol singkat

- Bergerak: WASD atau panah
- Interaksi: E

## Struktur singkat

- `Assets/` - aset permainan (scene, audio, sprites, prefab)
- `Assets/Scripts/` - logika C# (NPC, movement, game systems)
- `ProjectSettings/` - pengaturan Unity

Lihat [Assets/Scripts](Assets/Scripts) untuk kode sumber.

## Sinopsis Scene (singkat)

  sebuah koridor atau lantai yang tampak aman pada pandangan pertama, dipenuhi objek sehari-hari (kayu, botol, kardus, koran). Pada beberapa siklus, sebuah anomali muncul sebagai NPC, mengubah suasana menjadi tegang. Scene ini menjadi titik utama eksplorasi pemain dan pengenalan mekanik anomali.

## Daftar NPC (singkat)

- NPC (anomali): NPC ini dikonfigurasi sebagai anomali dalam `GameManager` dan dapat memicu rutinitas jumpscare yang mereset siklus permainan. Biasanya bergerak dengan `NpcMovement` (pergerakan ke kanan) dan dapat memiliki `LethalAnomaly` untuk mendeteksi tabrakan dengan pemain dan memicu efek suara/serangan spesifik.
- NPC normal: entitas non-berbahaya yang berfungsi sebagai latar/ornamen di scene Safe Floor, tidak memicu jumpscare.

Catatan: aset sprite dan animasi untuk NPC berada di `Assets/anomali agresif/` dan `Assets/Sprites/inilahMy/` (nama file seperti `npc anomali walk.png`, `npc walk.piskel`).