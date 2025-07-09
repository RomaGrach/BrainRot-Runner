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

        
        public bool isPurchased;          // Состояние покупки (синхронизируется с saves.skins)
    }

    [Header("Настройка магазина")]
    public ShopItem[] items;             // Все товары

    [Header("Персонаж")]
    public Renderer characterRenderer;   // У модели минимум 1 материал

    private int selectedIndex = -1;      // Индекс выбранного скина

    // Ссылка на данные сохранений YG
    private SavesYG saves => YG2.saves;

    private void Start()
    {
        // Данные уже загружены автоматически в YG2.saves

        // Подгоним длину массива skins
        if (saves.skins == null || saves.skins.Length != items.Length)
            System.Array.Resize(ref saves.skins, items.Length);

        // Инициализируем UI и состояния
        for (int i = 0; i < items.Length; i++)
        {
            var it = items[i];
            it.isPurchased = saves.skins[i];

            if (it.nameText != null)
                it.nameText.text = it.itemName;
            if (it.priceText != null)
                it.priceText.text = it.price.ToString();
            if (it.itemIcon != null && it.itemIconTexture != null)
                it.itemIcon.texture = it.itemIconTexture;
            if (it.actionButtonText != null)
                it.actionButtonText.text = it.isPurchased ? "Выбрать" : "Купить";
        }

        // Восстанавливаем последний выбранный скин
        selectedIndex = Mathf.Clamp(saves.NowSkin, 0, items.Length - 1);
        ApplySelection(selectedIndex);

        // Устанавливаем кнопку у выбранного скина в состояние "Выбрано"
        if (selectedIndex >= 0 && selectedIndex < items.Length && items[selectedIndex].actionButtonText != null)
        {
            items[selectedIndex].actionButtonText.text = "Выбрано";
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
            // Покупка
            if (saves.coins >= item.price)
            {
                saves.coins -= item.price;
                item.isPurchased = true;
                saves.skins[idx] = true;
                item.actionButtonText.text = "Выбрать";
                YG2.SaveProgress();
            }
            else
            {
                Debug.LogWarning($"Недостаточно монет для покупки «{item.itemName}»");
            }
        }
        else
        {
            // Сброс предыдущего
            if (selectedIndex >= 0 && selectedIndex < items.Length)
                items[selectedIndex].actionButtonText.text = "Выбрать";

            // Выбор нового
            selectedIndex = idx;
            saves.NowSkin = idx;
            item.actionButtonText.text = "Выбрано";
            ApplySelection(idx);
            YG2.SaveProgress();
        }
    }

    /// <summary>
    /// Применяет выбранный материал к модели и обновляет UI.
    /// </summary>
    private void ApplySelection(int idx)
    {
        // Сбросить "Выбрать" у всех, кроме idx
        for (int i = 0; i < items.Length; i++)
        {
            if (i != idx && items[i].isPurchased && items[i].actionButtonText != null)
                items[i].actionButtonText.text = "Выбрать";
        }

        // Меняем первый материал у модели
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
