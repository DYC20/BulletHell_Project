using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Security;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MenuNavigator : MonoBehaviour
{
    [SerializeField] private List<Button> buttons;
    [SerializeField] private RectTransform pointer;
    [SerializeField] private RectTransform cursor;

    [Header("Button Behavior")]
    [SerializeField, ColorUsage(false, true)] private Color highlightColor;
    [SerializeField] private float transitionTime = 0.15f;
    [SerializeField] private List<UnityEvent> selectionEvents;
    [SerializeField] private bool invokeSelectionEventOnEnable = false;

    private int previousIndex = -1;
    
    private Coroutine colorRoutine;
    private TMP_Text currentTMP;
    private Image currentImage;

    private Color originalTMPColor;
    private Color originalImageColor;

    private Material[] materials;
    private TMP_Text[] texts;

    private Image[] images;
    private Material[] imageMaterials;

    private bool[] tmpRout;
    private bool[] imageRout;

    private int currentIndex = 0;
    private bool initialized;

    //txt color name
    private static readonly int FaceColorID = Shader.PropertyToID("_FaceColor");

    //image color name
    private static readonly int ImageProxyColorID = Shader.PropertyToID("_Color");

    private void Awake()
    {
        Initialize();

    }

    private void OnEnable()
    {
        Initialize();
        
        currentIndex = 0;
        previousIndex = invokeSelectionEventOnEnable ? -1 : currentIndex;

        UpdateSelectionNextFrame();
    }
    private IEnumerator UpdateSelectionNextFrame()
    {
        yield return null;

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

        images = new Image[count];
        imageMaterials = new Material[count];

        tmpRout = new bool[count];
        imageRout = new bool[count];

        for (int i = 0; i < count; i++)
        {
            if (buttons[i] == null)
                continue;

            if (buttons[i].transform.childCount <= 1)
                continue;

            Transform childOne = buttons[i].transform.GetChild(1);

            TMP_Text tmp = childOne.GetComponent<TMP_Text>();

            if (tmp != null)
            {
                InitializeTMPRoute(i, tmp);
                AddHoverEvent(buttons[i], i);
                continue;
            }

            Image image = childOne.GetComponent<Image>();

            if (image != null)
            {
                InitializeImageRoute(i, image);
                AddHoverEvent(buttons[i], i);
                continue;
            }
        }

        bool foundAnyValidRoute = false;

        for (int i = 0; i < count; i++)
        {
            if (tmpRout[i] || imageRout[i])
            {
                foundAnyValidRoute = true;
                break;
            }
        }

        if (!foundAnyValidRoute)
        {
            {
                Debug.LogWarning($"{name}: No valid TMP/Image routes found");
                return;
            }
        }

        InitializeOriginalColors();

        initialized = true;
    }

    private void InitializeTMPRoute(int index, TMP_Text tmp)
    {
        tmpRout[index] = true;
        imageRout[index] = false;

        texts[index] = tmp;

        Material mat = new Material(tmp.fontSharedMaterial);
        mat.name = tmp.fontSharedMaterial.name + " Runtime Instance " + index;

        tmp.fontMaterial = mat;
        materials[index] = mat;

        tmp.color = Color.white;

        TouchTMPMaterial(tmp);
    }

    private void InitializeImageRoute(int index, Image image)
    {
        tmpRout[index] = false;
        imageRout[index] = true;

        images[index] = image;

        Material sourceMaterial = image.material;

        if (sourceMaterial == null)
        {
            Debug.LogWarning(image.name + " has no material.");
            return;
        }

        Material mat = new Material(sourceMaterial);
        mat.name = sourceMaterial.name + " Runtime Instance " + index;

        image.material = mat;
        imageMaterials[index] = mat;

        TouchImageMaterial(image);
        ResetAllVisuals();
    }

    private void InitializeOriginalColors()
    {
        originalTMPColor = Color.white;
        originalImageColor = Color.white;

        for (int i = 0; i < buttons.Count; i++)
        {
            if (tmpRout[i] && materials[i] != null && materials[i].HasProperty(FaceColorID))
            {
                originalTMPColor = materials[i].GetColor(FaceColorID);
                return;
            }

            if (imageRout[i] && imageMaterials[i] != null && imageMaterials[i].HasProperty(ImageProxyColorID))
            {
                originalImageColor = imageMaterials[i].GetColor(ImageProxyColorID);
                return;
            }
        }
    }

    private void Update()
    {
        if (Keyboard.current == null || buttons == null || buttons.Count == 0)
        {
            Debug.Log("keyboard = NULL / buttons = NULL");
              return;
              
        }
          

        if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
        {
            Debug.Log("Menu down pressed");
            currentIndex++;

            if (currentIndex >= buttons.Count)
                currentIndex = 0;

            UpdateSelection();
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
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
        {
            //Debug.LogWarning("Menu navigator called without initialized");
            return;
        }
        else
        {
          //  Debug.LogWarning("Menu navigator HAS initialized");
        }


        if (currentIndex < 0 || currentIndex >= buttons.Count)
        {
          //  Debug.LogWarning("Menu navigator called without currentIndex");
             return;
        }
        else
        {
           // Debug.LogWarning("Menu navigator called WITH currentIndex " + currentIndex);
        }
           

        RectTransform target = buttons[currentIndex].GetComponent<RectTransform>();

        // Move cursor and rotate pointer FIRST
        UpdatePointerAndCursor(target);

        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        ResetAllVisuals();

        currentTMP = null;
        currentImage = null;

        //BringButtonToFront();

        if (tmpRout[currentIndex])
        {
            currentTMP = texts[currentIndex];

            if (currentTMP != null)
                colorRoutine = StartCoroutine(ChangeTMPColor(currentIndex));
        }
        else if (imageRout[currentIndex])
        {
            currentImage = images[currentIndex];

            if (currentImage != null)
                colorRoutine = StartCoroutine(ChangeImageProxyColor(currentIndex));
        }
        
        if (currentIndex != previousIndex)
        {
            InvokeSelectionEvent();
            previousIndex = currentIndex;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(buttons[currentIndex].gameObject);
        }

    }
    /*
    private void BringButtonToFront()
    {
        buttons[currentIndex].gameObject.transform.SetAsLastSibling();
    }
    */
    private void UpdatePointerAndCursor(RectTransform target)
    {
        if (target == null)
            return;

        // Existing behavior: pointer looks at active button
        if (pointer != null)
        {
            Vector3 dir = target.position - pointer.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            pointer.rotation = Quaternion.Euler(0f, 0f, angle - 180f);
        }

        // New behavior: cursor moves to active button
        if (cursor != null)
        {
            cursor.position = target.position;
        }
    }

    private void ResetAllVisuals()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (tmpRout[i])
            {
                ResetTMPVisual(i);
            }
            else if (imageRout[i])
            {
                ResetImageVisual(i);
            }
        }
    }

    private void ResetTMPVisual(int index)
    {
        if (materials[index] == null)
            return;

        if (materials[index].HasProperty(FaceColorID))
            materials[index].SetColor(FaceColorID, originalTMPColor);

        if (texts[index] != null)
        {
            texts[index].fontMaterial = materials[index];
            texts[index].SetMaterialDirty();
            texts[index].SetVerticesDirty();
            TouchTMPMaterial(texts[index]);
        }
    }

    private void ResetImageVisual(int index)
    {
        SetImageMaterialProxyProperty(index, originalImageColor);
        SetImageComponentAlpha(index, 0f);
    }

    private IEnumerator ChangeTMPColor(int index)
    {
        Material mat = materials[index];
        TMP_Text tmp = texts[index];

        if (mat == null || tmp == null)
            yield break;

        Color targetColor = highlightColor;
        //targetColor.a = 1f;

        if (transitionTime <= 0f)
        {
            if (mat.HasProperty(FaceColorID))
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

            Color newColor = Color.Lerp(originalTMPColor, targetColor, t);
            newColor.a = 1f;

            if (mat.HasProperty(FaceColorID))
                mat.SetColor(FaceColorID, newColor);

            tmp.fontMaterial = mat;
            tmp.SetMaterialDirty();
            tmp.SetVerticesDirty();
            TouchTMPMaterial(tmp);

            yield return null;
        }

        if (mat.HasProperty(FaceColorID))
            mat.SetColor(FaceColorID, targetColor);

        tmp.fontMaterial = mat;
        tmp.SetMaterialDirty();
        tmp.SetVerticesDirty();
        TouchTMPMaterial(tmp);
    }

    private IEnumerator ChangeImageProxyColor(int index)
    {
        Image image = images[index];
        Material mat = imageMaterials[index];

        if (image == null || mat == null)
            yield break;

        Color targetColor = highlightColor;
        targetColor.a = 1f;

        float startAlpha = image.color.a;
        float targetAlpha = 1f;

        if (transitionTime <= 0f)
        {
            SetImageMaterialProxyProperty(index, targetColor);
            SetImageComponentAlpha(index, targetAlpha);
            yield break;
        }

        float time = 0f;

        while (time < transitionTime)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / transitionTime);

            Color newColor = Color.Lerp(originalImageColor, targetColor, t);
            newColor.a = 1f;

            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            SetImageMaterialProxyProperty(index, newColor);
            SetImageComponentAlpha(index, newAlpha);

            yield return null;
        }

        SetImageMaterialProxyProperty(index, targetColor);
        SetImageComponentAlpha(index, targetAlpha);
    }

    private void SetImageMaterialProxyProperty(int index, Color color)
    {
        if (images == null || imageMaterials == null)
            return;

        if (index < 0 || index >= images.Length)
            return;

        Image image = images[index];
        Material mat = imageMaterials[index];

        if (image == null || mat == null)
            return;

        if (mat.HasProperty(ImageProxyColorID))
        {
            mat.SetColor(ImageProxyColorID, color);
        }
        else if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", color);
        }
        else if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }
        else
        {
            image.color = color;
        }

        image.material = mat;
        image.SetMaterialDirty();
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

    private void TouchImageMaterial(Image image)
    {
        if (image == null)
            return;

        Material activeMaterial = image.material;

        if (activeMaterial == null)
            return;

        if (activeMaterial.HasProperty(ImageProxyColorID))
        {
            activeMaterial.GetColor(ImageProxyColorID);
        }
        else if (activeMaterial.HasProperty("_Color"))
        {
            activeMaterial.GetColor("_Color");
        }
        else if (activeMaterial.HasProperty("_BaseColor"))
        {
            activeMaterial.GetColor("_BaseColor");
        }
    }
    
    private void SetImageComponentAlpha(int index, float alpha)
    {
        if (images == null)
            return;

        if (index < 0 || index >= images.Length)
            return;

        Image image = images[index];

        if (image == null)
            return;

        Color imageColor = image.color;
        imageColor.a = alpha;
        image.color = imageColor;

        image.SetMaterialDirty();
    }
    
    private void InvokeSelectionEvent()
    {
        if (selectionEvents == null)
            return;

        if (currentIndex < 0 || currentIndex >= selectionEvents.Count)
            return;

        if (selectionEvents[currentIndex] == null)
            return;

        selectionEvents[currentIndex].Invoke();
    }

    private void OnDestroy()
    {
        if (materials != null)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                    Destroy(materials[i]);
            }
        }

        if (imageMaterials != null)
        {
            for (int i = 0; i < imageMaterials.Length; i++)
            {
                if (imageMaterials[i] != null)
                    Destroy(imageMaterials[i]);
            }
        }
    }
}