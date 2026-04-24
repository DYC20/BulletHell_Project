using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;


public class MenuNavigator : MonoBehaviour
{
    [SerializeField] private List<Button> buttons;
    
    [SerializeField] private RectTransform pointer;
    [SerializeField, ColorUsage(false,true)] private Color highlightColor;
    [SerializeField] private float transitionTime;
    
    private Coroutine colorRoutine;
    private TMP_Text currentTMP;
    
    private Color originalColor;
    private Material[] materials;
    private TMP_Text[] texts;

    private int currentIndex = 0;

    void Start()
    {
        int count = buttons.Count;

        texts = new TMP_Text[count];
        materials = new Material[count];

        for (int i = 0; i < count; i++)
        {
            TMP_Text tmp = buttons[i].transform.GetChild(1).GetComponent<TMP_Text>();
            texts[i] = tmp;

            Material mat = new Material(tmp.fontSharedMaterial);
            tmp.fontMaterial = mat;
            materials[i] = mat;

            tmp.color = Color.white;
        }

        originalColor = materials[0].GetColor("_FaceColor");

        UpdateSelection();
    }

    void Update()
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

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            buttons[currentIndex].onClick.Invoke();
        }
        
        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            buttons[currentIndex].onClick.Invoke();
        }
    }

    void UpdateSelection()
    {
        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        for (int i = 0; i < materials.Length; i++)
            materials[i].SetColor("_FaceColor", originalColor);

        currentTMP = texts[currentIndex];

        colorRoutine = StartCoroutine(ChangeColor(currentIndex));

        RectTransform target = buttons[currentIndex].GetComponent<RectTransform>();

        Vector3 dir = target.position - pointer.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        pointer.rotation = Quaternion.Euler(0f, 0f, angle - 180f);
    }

    IEnumerator ChangeColor(int index)
    {
        Material mat = materials[index];

        Color targetColor = highlightColor;
        targetColor.a = 1f;

        float time = 0f;

        while (time < transitionTime)
        {
            time += Time.deltaTime;
            float t = time / transitionTime;

            Color newColor = Color.Lerp(originalColor, targetColor, t);
            newColor.a = 1f;

            mat.SetColor("_FaceColor", newColor);

            yield return null;
        }

        mat.SetColor("_FaceColor", targetColor);
    }
}