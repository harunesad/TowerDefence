using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Core;
using TowerDefence.Combat;

namespace TowerDefence.UI
{
    /// <summary>
    /// Mobil/Cihaz üzerinde bölümleri tek tıkla rastgele başarıyla (1-3 yıldız) bitirip test etmeyi sağlayan debug bileşeni.
    /// Runtime'da HUDController tarafından otomatik oluşturulur.
    /// </summary>
    public class DebugAutoWinButton : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            RectTransform rt = GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();

            // Ekranın sağ-üst köşesinde, diğer butonların yanına güvenli bir alana yerleş
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-15f, -80f);
            rt.sizeDelta = new Vector2(110f, 40f);

            // Arka plan (Yarı saydam dikkat çekici koyu kehribar/altın)
            Image bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.85f, 0.45f, 0.05f, 0.75f);

            // Buton bileşeni
            button = gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.85f, 0.45f, 0.05f, 0.75f);
            colors.highlightedColor = new Color(1f, 0.6f, 0.1f, 0.95f);
            colors.pressedColor = new Color(0.6f, 0.3f, 0.0f, 1f);
            button.colors = colors;
            button.onClick.AddListener(OnAutoWinClicked);

            // Yazı (TMP)
            GameObject textObj = new GameObject("Label", typeof(RectTransform));
            textObj.transform.SetParent(transform, false);

            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "⚡ WIN";
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
        }

        private void OnAutoWinClicked()
        {
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
            return;
#else
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.GetCurrentState() == GameState.Victory || 
                GameManager.Instance.GetCurrentState() == GameState.Defeat)
            {
                return; // Zaten oyun bitti
            }

            Debug.Log("[DebugAutoWin] Test butonuna basıldı! Rastgele başarı hesaplanıyor...");

            // 1. Rastgele yıldız belirleme (1, 2 veya 3 yıldız)
            // %25 ihtimalle 1 yıldız, %35 ihtimalle 2 yıldız, %40 ihtimalle 3 yıldız
            int randomRoll = Random.Range(0, 100);
            int targetStars;
            if (randomRoll < 25) targetStars = 1;
            else if (randomRoll < 60) targetStars = 2;
            else targetStars = 3;

            // 2. Can simülasyonu: LevelResultUI yıldızı (livesRemaining / totalLives) formülüyle hesaplar:
            // lifePercent >= 1.0f -> 3 Yıldız
            // lifePercent >= 0.5f -> 2 Yıldız
            // lifePercent < 0.5f  -> 1 Yıldız
            if (LivesManager.Instance != null)
            {
                int maxLives = LivesManager.Instance.GetMaxLives();
                int simulatedLives;

                switch (targetStars)
                {
                    case 3:
                        simulatedLives = maxLives; // %100 can
                        break;
                    case 2:
                        simulatedLives = Mathf.Max(1, Mathf.RoundToInt(maxLives * Random.Range(0.55f, 0.85f))); // %55-%85 can
                        break;
                    case 1:
                    default:
                        simulatedLives = Mathf.Max(1, Mathf.RoundToInt(maxLives * Random.Range(0.15f, 0.40f))); // %15-%40 can
                        break;
                }

                LivesManager.Instance.SetCurrentLivesForDebug(simulatedLives);
                Debug.Log($"[DebugAutoWin] Hedeflenen Yıldız: {targetStars}★ | Simüle Edilen Can: {simulatedLives}/{maxLives}");
            }

            // 3. Sahadaki aktif düşmanları durdur/temizle
            Unit[] allUnits = FindObjectsOfType<Unit>();
            foreach (var unit in allUnits)
            {
                if (unit != null)
                {
                    unit.TakeDamage(999999);
                }
            }

            // 4. Zafer durumunu tetikle
            GameManager.Instance.ChangeState(GameState.Victory);

            // Test butonunu tekrar basılmaması için gizle/devre dışı bırak
            gameObject.SetActive(false);
#endif
        }
    }
}
