using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

public class MenuNavigator : MonoBehaviour
{
    [SerializeField] private List<Button> buttons;
    [SerializeField] private RectTransform pointer;

    [Header("Button Behavior")]
    [SerializeField, ColorUsage(false, true)] private Color highlightColor;
    [SerializeField] private float transitionTime = 0.15f;

    private Coroutine colorRoutine;
    private TMP_Text currentTMP;

    private Color originalColor;
    private Material[] materials;
    private TMP_Text[] texts;

    private int currentIndex = 0;
    private bool initialized;

    private static readonly int FaceColorID = Shader.PropertyToID("_FaceColor");

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();

        currentIndex = 0;
        UpdateSelection();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        if (buttons == null || buttons.Count == 0)
            return;

        int count = buttons.Count;

        texts = new TMP_Text[count];
        materials = new Material[count];

        for (int i = 0; i < count; i++)
        {
            if (buttons[i] == null)
                continue;

            TMP_Text tmp = null;

            if (buttons[i].transform.childCount > 1)
            {
                Transform childOne = buttons[i].transform.GetChild(1);
                tmp = childOne.GetComponent<TMP_Text>();
            }

            if (tmp == null)
                tmp = buttons[i].GetComponentInChildren<TMP_Text>(true);

            if (tmp == null)
                continue;

            texts[i] = tmp;

            Material mat = new Material(tmp.fontSharedMaterial);
            mat.name = tmp.fontSharedMaterial.name + " Runtime Instance " + i;

            tmp.fontMaterial = mat;
            materials[i] = mat;

            tmp.color = Color.white;

            TouchTMPMaterial(tmp);

            AddHoverEvent(buttons[i], i);
        }

        if (materials[0] == null)
            return;

        originalColor = materials[0].GetColor(FaceColorID);

        initialized = true;
    }

    private void Update()
    {
        if (Keyboard.current == null || buttons == null || buttons.Count == 0)
            return;

        if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            currentIndex++;

            if (currentIndex >= buttons.Count)
                currentIndex = 0;

            UpdateSelection();
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
        {
            currentIndex--;

            if (currentIndex < 0)
                currentIndex = buttons.Count - 1;

            UpdateSelection();
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            buttons[currentIndex].onClick.Invoke();
        }
    }

    private void AddHoverEvent(Button button, int index)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;

        enterEntry.callback.AddListener((data) =>
        {
            currentIndex = index;
            UpdateSelection();
        });

        trigger.triggers.Add(enterEntry);
    }

    private void UpdateSelection()
    {
        if (!initialized)
            return;

        if (currentIndex < 0 || currentIndex >= buttons.Count)
            return;

        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null)
                continue;

            materials[i].SetColor(FaceColorID, originalColor);

            if (texts[i] != null)
            {
                texts[i].fontMaterial = materials[i];
                texts[i].SetMaterialDirty();
                texts[i].SetVerticesDirty();
                TouchTMPMaterial(texts[i]);
            }
        }

        currentTMP = texts[currentIndex];

        if (currentTMP == null)
            return;

        colorRoutine = StartCoroutine(ChangeColor(currentIndex));

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(buttons[currentIndex].gameObject);
        }

        RectTransform target = buttons[currentIndex].GetComponent<RectTransform>();

        if (pointer != null && target != null)
        {
            Vector3 dir = target.position - pointer.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            pointer.rotation = Quaternion.Euler(0f, 0f, angle - 180f);
        }
    }

    private IEnumerator ChangeColor(int index)
    {
        Material mat = materials[index];
        TMP_Text tmp = texts[index];

        if (mat == null || tmp == null)
            yield break;

        Color targetColor = highlightColor;
        targetColor.a = 1f;

        if (transitionTime <= 0f)
        {
            mat.SetColor(FaceColorID, targetColor);

            tmp.fontMaterial = mat;
            tmp.SetMaterialDirty();
            tmp.SetVerticesDirty();
            TouchTMPMaterial(tmp);

            yield break;
        }

        float time = 0f;

        while (time < transitionTime)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / transitionTime);

            Color newColor = Color.Lerp(originalColor, targetColor, t);
            newColor.a = 1f;

            mat.SetColor(FaceColorID, newColor);

            tmp.fontMaterial = mat;
            tmp.SetMaterialDirty();
            tmp.SetVerticesDirty();
            TouchTMPMaterial(tmp);

            yield return null;
        }

        mat.SetColor(FaceColorID, targetColor);

        tmp.fontMaterial = mat;
        tmp.SetMaterialDirty();
        tmp.SetVerticesDirty();
        TouchTMPMaterial(tmp);
    }

    private void TouchTMPMaterial(TMP_Text tmp)
    {
        if (tmp == null)
            return;

        Material activeMaterial = tmp.fontMaterial;

        if (activeMaterial != null && activeMaterial.HasProperty(FaceColorID))
        {
            activeMaterial.GetColor(FaceColorID);
        }
    }

    private void OnDestroy()
    {
        if (materials == null)
            return;

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null)
                Destroy(materials[i]);
        }
    }
}