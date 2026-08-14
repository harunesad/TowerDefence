using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Combat;
using TowerDefence.Core;
using TowerDefence.Data;

namespace TowerDefence.UI
{
    public class HeroStatsPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI heroSideText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private TextMeshProUGUI abilityDescText;
        [SerializeField] private TextMeshProUGUI abilityCooldownText;

        [Header("Background Blocker")]
        [SerializeField] private GameObject blockerPanel;

        [Header("Behavior")]
        [SerializeField] private Vector2 panelOffset = new Vector2(0f, 30f);

        private HeroData currentData;
        private HeroUnit currentHero;

        private void Awake()
        {
            if (blockerPanel != null)
            {
                Graphic g = blockerPanel.GetComponent<Graphic>();
                if (g == null) g = blockerPanel.AddComponent<Image>();
                g.color = new Color(0, 0, 0, 0.001f);
                g.raycastTarget = true;
                Button btn = blockerPanel.GetComponent<Button>();
                if (btn == null) btn = blockerPanel.AddComponent<Button>();
                btn.onClick.AddListener(Hide);
                btn.transition = Selectable.Transition.None;
            }
            Hide();
        }

        public void Show(RectTransform anchor, HeroData data, HeroUnit liveHero = null)
        {
            if (panelRoot == null || data == null) return;

            currentData = data;
            currentHero = liveHero;

            if (blockerPanel != null) blockerPanel.SetActive(true);
            panelRoot.SetActive(true);

            // Position the panel next to / above the anchor
            RectTransform selfRT = GetComponent<RectTransform>();
            if (selfRT == null) selfRT = gameObject.AddComponent<RectTransform>();
            RectTransform panelRT = panelRoot.GetComponent<RectTransform>();
            Canvas canvas = GetComponentInParent<Canvas>();

            if (anchor != null && canvas != null)
            {
                Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;
                Vector3 worldPoint;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    (RectTransform)canvas.transform,
                    RectTransformUtility.WorldToScreenPoint(cam, anchor.position),
                    cam,
                    out worldPoint);

                panelRT.pivot = new Vector2(0f, 0f);
                panelRoot.transform.position = worldPoint;
                panelRT.anchoredPosition += new Vector2(anchor.rect.width * 0.5f + 10f, panelOffset.y);
                ClampToScreen(canvas, panelRT);
            }

            PopulateData();
        }

        private void ClampToScreen(Canvas canvas, RectTransform rt)
        {
            Vector2 size = rt.rect.size;
            Vector3 pos = rt.position;
            RectTransform canvasRT = (RectTransform)canvas.transform;
            Vector3[] corners = new Vector3[4];
            canvasRT.GetWorldCorners(corners);
            
            float minX = corners[0].x;
            float maxX = corners[2].x - size.x * canvasRT.localScale.x;
            float minY = corners[0].y;
            float maxY = corners[1].y - size.y * canvasRT.localScale.y;
            
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            rt.position = pos;
        }

        private void PopulateData()
        {
            if (currentData == null) return;

            if (heroNameText != null) heroNameText.text = currentData.displayName;

            if (heroSideText != null)
            {
                heroSideText.text = currentData.side == Side.Light ? "LIGHT" : "DARK";
                heroSideText.color = currentData.side == Side.Light
                    ? new Color(1f, 0.9f, 0.5f)
                    : new Color(0.7f, 0.4f, 1f);
            }

            if (statsText != null)
            {
                float hp = 100f, dmg = 10f, atkR = 2f, atkSpd = 1f, mSpd = 3f;
                string tier = "Tier 1";
                UnitData ud = currentData.unitData;
                if (ud != null)
                {
                    hp = ud.maxHealth;
                    dmg = ud.attackDamage;
                    atkR = ud.attackRange;
                    atkSpd = ud.attackRate;
                    mSpd = ud.moveSpeed;
                    int t = (ud.spawnCost >= 900) ? 4 : (ud.spawnCost >= 400 ? 3 : (ud.spawnCost >= 180 ? 2 : 1));
                    tier = $"Tier {t}";
                }

                if (currentHero != null)
                {
                    hp = currentHero.GetMaxHealth();
                    dmg = currentHero.GetDamage();
                    statsText.text =
                        $"<color=#FFD24D>{tier}</color>\n" +
                        $"HP: {currentHero.GetHealth():F0}/{hp:F0}\n" +
                        $"DMG: {dmg:F0}   ATK: {atkSpd:F2}/s\n" +
                        $"RNG: {atkR:F1}   SPD: {mSpd:F1}";
                }
                else
                {
                    statsText.text =
                        $"<color=#FFD24D>{tier}</color>\n" +
                        $"HP: {hp:F0}\n" +
                        $"DMG: {dmg:F0}   ATK: {atkSpd:F2}/s\n" +
                        $"RNG: {atkR:F1}   SPD: {mSpd:F1}";
                }
            }

            if (abilityNameText != null)
                abilityNameText.text = $"<color=#FFD24D>[{currentData.abilityType}]</color> {currentData.abilityName}";

            if (abilityDescText != null)
                abilityDescText.text = currentData.abilityDescription;

            if (abilityCooldownText != null)
            {
                if (currentHero != null)
                {
                    float rem = currentHero.AbilityCooldownRemaining;
                    abilityCooldownText.text = rem <= 0f
                        ? "<color=#4DFF85>READY</color>"
                        : $"<color=#FF8080>CD: {rem:F1}s / {currentData.abilityCooldown:F1}s</color>";
                }
                else
                {
                    abilityCooldownText.text = $"CD: {currentData.abilityCooldown:F1}s";
                }
            }
        }

        private void Update()
        {
            if (panelRoot != null && panelRoot.activeSelf && currentHero != null)
            {
                PopulateData();
            }
        }

        public void Hide()
        {
            if (blockerPanel != null) blockerPanel.SetActive(false);
            if (panelRoot != null) panelRoot.SetActive(false);
            currentData = null;
            currentHero = null;
        }

        public bool IsVisible => panelRoot != null && panelRoot.activeSelf;
    }
}
