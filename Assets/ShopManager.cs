using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YG;  // PluginYG2 namespace

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        [Header("Данные предмета")]
        public string itemName;           // Название предмета
        public int price;                 // Цена
        public Material material;         // Материал для смены у модели

        [Header("Иконка")]
        public RawImage itemIcon;         // RawImage для иконки
        public Texture itemIconTexture;   // Текстура иконки (назначаете в инспекторе)

        [Header("Текстовые поля UI")]
        public TMP_Text nameText;         // Название (TextMeshPro)
        public TMP_Text priceText;        // Цена (TextMeshPro)
        public TMP_Text actionButtonText; // Текст на кнопке (TextMeshPro)

        [HideInInspector] public bool isPurchased;  // Состояние покупки (синхронизируется с saves.skins)
    }

    [System.Serializable]
    public class UpdateItem
    {
        [Header("Данные улучшения")]
        public string itemName;            // Название апгрейда
        public float initialDuration;        // Начальное значение (если в сохранении 0)
        public int upgradeIncrement;       // Фиксированный прирост длительности
        public int basePrice;              // Изначальная стоимость (если в сохранении 0)
        public float priceScale = 1.2f;    // Множитель роста цены

        [Header("Иконка и UI")]
        public RawImage itemIcon;          // RawImage для иконки
        public Texture itemIconTexture;    // Текстура иконки
        public TMP_Text nameText;          // Название в UI
        public TMP_Text valueText;         // Отображает saves.itemsDur[i]
        public TMP_Text priceText;         // Отображает saves.itemsDurPrise[i]
        public TMP_Text actionButtonText;  // Кнопка "Улучшить"
    }

    [Header("Настройка магазина")]
    public ShopItem[] items;              // Все товары

    [Header("Настройка улучшений")]
    public UpdateItem[] updateItems;      // Все апгрейды

    [Header("Персонаж")]
    public Renderer characterRenderer;    // У модели минимум 1 материал

    private int selectedIndex = -1;       // Индекс выбранного скина

    // Ссылка на данные сохранений YG
    private SavesYG saves => YG2.saves;

    private void Start()
    {
        // данные из YG2.saves уже загружены автоматически
        InitSkins();
        InitUpdates();
    }

    private void InitSkins()
    {
        if (saves.skins == null || saves.skins.Length != items.Length)
            System.Array.Resize(ref saves.skins, items.Length);

        for (int i = 0; i < items.Length; i++)
        {
            var it = items[i];
            it.isPurchased = saves.skins[i];

            if (it.nameText != null) it.nameText.text = it.itemName;
            if (it.priceText != null) it.priceText.text = it.price.ToString();
            if (it.itemIcon != null && it.itemIconTexture != null)
                it.itemIcon.texture = it.itemIconTexture;
            if (it.actionButtonText != null)
                it.actionButtonText.text = it.isPurchased ? "Выбрать" : "Купить";
        }

        selectedIndex = Mathf.Clamp(saves.NowSkin, 0, items.Length - 1);
        ApplySelection(selectedIndex);

        if (selectedIndex >= 0 && selectedIndex < items.Length &&
            items[selectedIndex].actionButtonText != null)
        {
            items[selectedIndex].actionButtonText.text = "Выбрано";
        }
    }

    private void InitUpdates()
    {
        if (saves.itemsDur == null || saves.itemsDur.Length != updateItems.Length)
            System.Array.Resize(ref saves.itemsDur, updateItems.Length);
        if (saves.itemsDurPrise == null || saves.itemsDurPrise.Length != updateItems.Length)
            System.Array.Resize(ref saves.itemsDurPrise, updateItems.Length);

        for (int i = 0; i < updateItems.Length; i++)
        {
            var ui = updateItems[i];


            ui.initialDuration = saves.itemsDur[i];
            ui.basePrice = saves.itemsDurPrise[i];

            int curDur = Mathf.RoundToInt(ui.initialDuration);
            int curPrice = ui.basePrice;

            if (ui.nameText != null) ui.nameText.text = ui.itemName;
            if (ui.valueText != null) ui.valueText.text = curDur.ToString();
            if (ui.priceText != null) ui.priceText.text = curPrice.ToString();
            if (ui.itemIcon != null && ui.itemIconTexture != null)
                ui.itemIcon.texture = ui.itemIconTexture;
            if (ui.actionButtonText != null)
                ui.actionButtonText.text = "Улучшить";
        }
    }

    /// <summary>
    /// Вызывается из UnityEvent кнопки, параметр — индекс товара.
    /// </summary>
    public void OnItemButtonPressed(float itemIndex)
    {
        int idx = Mathf.Clamp(Mathf.RoundToInt(itemIndex), 0, items.Length - 1);
        var item = items[idx];

        if (!item.isPurchased)
        {
            if (saves.coins >= item.price)
            {
                saves.coins -= item.price;
                item.isPurchased = true;
                saves.skins[idx] = true;
                item.actionButtonText.text = "Выбрать";
                YG2.SaveProgress();
                AudioManager.Instance.PlayPurchase();

            }
            else
            {
                Debug.LogWarning($"Недостаточно монет для покупки «{item.itemName}»");
            }
        }
        else
        {
            if (selectedIndex >= 0 && selectedIndex < items.Length)
                items[selectedIndex].actionButtonText.text = "Выбрать";

            selectedIndex = idx;
            saves.NowSkin = idx;
            item.actionButtonText.text = "Выбрано";
            ApplySelection(idx);
            YG2.SaveProgress();
        }
    }

    /// <summary>
    /// Вызывается из UnityEvent кнопки апгрейда, параметр — индекс апгрейда.
    /// </summary>
    /// <summary>
    /// Вызывается из UnityEvent кнопки апгрейда, параметр — индекс апгрейда.
    /// </summary>
    public void UpdateItems(float itemIndex)
    {
        int idx = Mathf.Clamp(Mathf.RoundToInt(itemIndex), 0, updateItems.Length - 1);
        var ui = updateItems[idx];

        // Текущее значение и цена берём из ui, а не из saves
        int currentValue = Mathf.RoundToInt(ui.initialDuration);
        int currentPrice = ui.basePrice;

        // Проверяем, хватает ли монет
        if (saves.coins < currentPrice)
        {
            Debug.LogWarning($"Недостаточно монет для улучшения «{ui.itemName}»");
            return;
        }

        // Списываем монеты
        saves.coins -= currentPrice;

        // Обновляем параметры апгрейда
        currentValue += ui.upgradeIncrement;
        currentPrice = Mathf.RoundToInt(currentPrice * ui.priceScale);

        // Записываем обратно в ui
        ui.initialDuration = currentValue;
        ui.basePrice = currentPrice;

        // Обновляем отображение в UI
        if (ui.valueText != null) ui.valueText.text = currentValue.ToString();
        if (ui.priceText != null) ui.priceText.text = currentPrice.ToString();
        if (ui.actionButtonText != null) ui.actionButtonText.text = "Улучшить";

        // Сохраняем результаты в saves
        saves.itemsDur[idx] = ui.initialDuration;
        saves.itemsDurPrise[idx] = ui.basePrice;
        YG2.SaveProgress();
    }


    /// <summary>
    /// Применяет выбранный материал к модели и обновляет UI.
    /// </summary>
    private void ApplySelection(int idx)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (i != idx && items[i].isPurchased && items[i].actionButtonText != null)
                items[i].actionButtonText.text = "Выбрать";
        }

        if (characterRenderer != null && idx >= 0 && idx < items.Length)
        {
            var mats = characterRenderer.materials;
            if (mats.Length > 0)
            {
                mats[0] = items[idx].material;
                characterRenderer.materials = mats;
            }
        }
    }
}
