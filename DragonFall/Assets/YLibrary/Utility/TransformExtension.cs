using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;



public static class TransformExtensions
{
    #pragma warning disable
    static Vector2 tempVec2 = new Vector2();
    static Vector3 tempVec3 = new Vector3();
    static Vector4 tempVec4 = new Vector4();
    static Color32 tempColor = new Color32();
    static Color tempColorF = new Color();
    #pragma warning restore      


    public static void SetGlobalPosition(this Transform t, Vector3 pos)
    {
        t.position = pos;
    }

    public static void SetGlobalPosition(this Transform t, float x, float y, float z)
    {
        tempVec3.Set(x, y, z);
        t.position = tempVec3;
    }

    public static void GetGlobalPosition(this Transform t, out float x, out float y, out float z)
    {
        x = t.position.x;
        y = t.position.y;
        z = t.position.z;
    }

    public static void SetPosition(this Transform t, float x, float y, float z)
    {
        t.localPosition = new Vector3(x, y, z);
    }

    public static void SetPositionX(this Transform t, float val)
    {
        tempVec3 = t.localPosition;
        tempVec3.x = val;
        t.localPosition = tempVec3;
    }

    public static void SetPositionY(this Transform t, float val)
    {
        tempVec3 = t.localPosition;
        tempVec3.y = val;
        t.localPosition = tempVec3;
    }

    public static void SetPositionZ(this Transform t, float val)
    {
        tempVec3 = t.localPosition;
        tempVec3.z = val;
        t.localPosition = tempVec3;
    }

    public static void GetPosition(this Transform t, out float x, out float y, out float z)
    {
        x = t.localPosition.x;
        y = t.localPosition.y;
        z = t.localPosition.z;
    }

    public static void SetScale(this Transform t, float x, float y, float z)
    {
        t.localScale = new Vector3(x, y, z);
    }

    public static void SetScale(this Transform t, float scale)
    {
        t.localScale = new Vector3(scale, scale, scale);
    }

    public static void Reset(this Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localScale = Vector3.one;
        t.localRotation = Quaternion.identity;
    }

    public static void GetUISize(this Transform t, out float x, out float y)
    {
        RectTransform rt = t.GetCachedComponent<RectTransform>();
        if (rt != null)
        {
            x = rt.rect.width;
            y = rt.rect.height;
            return;
        }
        x = 0;
        y = 0;
    }
    public static void SetUISize(this Transform t, float x, float y)
    {
        RectTransform rt = t.GetCachedComponent<RectTransform>();
        if (rt != null)
        {
            tempVec2.Set(x, y);
            rt.sizeDelta = tempVec2;
        }
    }

    public static void GetTextSize(this Transform t, out float x, out float y)
    {
        var text = t.GetCachedComponent<Text>();
        if (text == null)
        {
            x = 0;
            y = 0;
            return;
        }
        y = text.preferredHeight;
        x = text.preferredWidth;
    }


    public static void ScaleToSize(this Transform t, float w)
    {
        float x;
        float y;
        t.GetUISize(out x, out y);
        if (x != 0)
        {
            t.SetScale(w / x);
        }
    }


    public static void GetScale(this Transform t, out float x, out float y, out float z)
    {
        x = t.localScale.x;
        y = t.localScale.y;
        z = t.localScale.z;
    }

    public static void SetGlobalRotation(this Transform t, float x, float y, float z)
    {
        t.rotation = Quaternion.Euler(x, y, z);
    }

    public static void GetGlobalRotation(this Transform t, out float x, out float y, out float z)
    {
        Vector3 v = t.rotation.eulerAngles;
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public static void SetRotation(this Transform t, float x, float y, float z)
    {
        t.localRotation = Quaternion.Euler(x, y, z);
    }

    public static void GetRotation(this Transform t, out float x, out float y, out float z)
    {
        Vector3 v = t.localRotation.eulerAngles;
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public static void SetLookDir(this Transform t, float x, float y, float z)
    {
        tempVec3.Set(x, y, z);
        t.localRotation = Quaternion.LookRotation(tempVec3);

    }

    public static void SetLookAt(this Transform t, float x, float y, float z)
    {
        tempVec3.Set(x, y, z);
        t.LookAt(tempVec3);
    }

    public static void SetActive(this Transform t, bool visible)
    {
        if (t == null)
        {
            Y_Debug.LogRed("Transform.SetActive() throw new System.NullReferenceException()");
//                 throw new System.NullReferenceException();
            return;
        }
        if (t.gameObject.activeSelf != visible)
        {
            t.gameObject.SetActive(visible);
        }
    }
    public static bool IsActive(this Transform t)
    {
        return t == null ? false : t.gameObject.activeSelf;
    }

    public static bool AddChild(this Transform t, Transform obj)
    {
        obj.SetParent(t, false);
        return true;
    }

    public static bool AddChild(this Transform t, Transform obj, string name)
    {
        Transform child = t.Find(name);
        if (child)
        {
            obj.SetParent(child, false);
            return true;
        }
        return false;
    }

    public static int SetLayer(this Transform t, int layerName)
    {
        int oldLayer = t.gameObject.layer;
        t.gameObject.layer = layerName;
        return oldLayer;
    }

    public static int SetLayer(this Transform t, int layerName, bool changeChildren)
    {
        int oldLayer = t.gameObject.layer;
        t.gameObject.layer = layerName;
        if (changeChildren)
        {
            foreach (Transform child in t)
            {
                SetLayer(child, layerName, changeChildren);
            }
        }
        return oldLayer;
    }

    public static void SetPivotX(this Transform t, float x)
    {
        RectTransform rectT = t as RectTransform;
        tempVec2.x = x;
        tempVec2.y = rectT.pivot.y;
        rectT.pivot = tempVec2;
    }

    public static void SetPivotY(this Transform t, float y)
    {
        RectTransform rectT = t as RectTransform;
        tempVec2.x = rectT.pivot.x;
        tempVec2.y = y;
        rectT.pivot = tempVec2;
    }

    public static void SetPivot(this Transform t, float x, float y)
    {
        RectTransform rectT = t as RectTransform;
        tempVec2.x = x;
        tempVec2.y = y;
        rectT.pivot = tempVec2;
    }

    public static void ResetPRS(this Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localEulerAngles = Vector3.zero;
        t.localScale = Vector3.one;
    }

    public static void ClearAllChild(this Transform t)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            GameObject.Destroy(t.GetChild(i).gameObject);
        }
    }

    public static float GetHeightByRaycast(this Transform t, float x, float z, int layerMask)
    {
        Vector3 origin = new Vector3(x, 1000, z);
        RaycastHit hit;
        Ray ray = new Ray(origin, Vector3.down);
        if (Physics.Raycast(ray, out hit, 1500f, layerMask))
        {
            return hit.point.y;
        }
        return -9999f;
    }

    public static T GetNeastComponentInParent<T>(this Transform t) where T : Component
    {
        if (t.gameObject.activeInHierarchy)
        {
            return t.GetCachedComponentInParent<T>(includeInactive: false);
        }
        else
        {
            return t.GetCachedComponentInParent<T>(includeInactive: true);
        }
    }

    public static void SetGray(this Transform t, bool isSetGray)
    {
        Material mat = null;

        Image[] imgArr = t.GetCachedComponentsInChildren<Image>(true);
        if (imgArr != null && imgArr.Length > 0)
        {
            // if (!isSetGray)
            // {
            //     mat = MaterialLoad.main.m_mat_ui_default;
            // }
            // else
            // {
            //     mat = MaterialLoad.main.m_mat_ui_gray;
            // }
            if (mat == null) return;
            foreach (var img in imgArr)
                img.material = mat;
        }
    }

    public static void BindCameraTexture(this Transform t, Transform camera_tf)
    {
        var img = t.GetCachedComponent<RawImage>();
        if (img == null)
        {
            return;
        }

        var camera = camera_tf.GetCachedComponent<Camera>();
        if (camera == null)
        {
            return;
        }
        img.texture = camera.targetTexture;
    }

    public static void SetNativeSize(this Transform t)
    {
        Image img = t.GetCachedComponent<Image>();
        if (img)
        {
            img.SetNativeSize();
        }
    }

    public static void SetSprite(this Transform t, Sprite sp, bool nativeSize = true, bool resetPivot = false)
    {
        // 统一走缓存：避免每帧/频繁调用时重复 GetComponent
        var img = t.GetCachedComponent<Image>();
        if (img != null)
        {
            img.sprite = sp;
            if (nativeSize)
            {
                img.SetNativeSize();
            }

            if (resetPivot) // 重新设置锚点
            {
                RectTransform rt = t as RectTransform;
                if (rt)
                {
                    tempVec2.x = sp.pivot.x / rt.sizeDelta.x;
                    tempVec2.y = sp.pivot.y / rt.sizeDelta.y;
                    rt.pivot = tempVec2;
                }
            }
        }

        var spriteRenderer = t.GetCachedComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sp;
        }

        var spriteMask = t.GetCachedComponent<SpriteMask>();
        if (spriteMask != null)
        {
            spriteMask.sprite = sp;
        }
    }

    public static void SetSprite(this Transform t, string path, string name, bool nativeSize = true)
    {
        Image img = t.GetCachedComponent<Image>();
        if (img != null)
        {
            path = path + "/" + name;
            img.sprite = ResourceManager.Instance.LoadRes<Sprite>(path);
            if (nativeSize)
            {
                img.SetNativeSize();
            }
        }
        SpriteRenderer spriteRenderer = t.GetCachedComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            path = path + "/" + name;
            spriteRenderer.sprite = ResourceManager.Instance.LoadRes<Sprite>(path);
        }

        SpriteMask spriteMask = t.GetCachedComponent<SpriteMask>();
        if (spriteMask != null)
        {
            path = path + "/" + name;
            spriteMask.sprite = ResourceManager.Instance.LoadRes<Sprite>(path);
        }
    }


    public static Sprite GetSprite(this Transform t)
    {
        Image img = t.GetCachedComponent<Image>();
        if (img != null)
        {
            return img.sprite;
        }
        SpriteRenderer sprite = t.GetCachedComponent<SpriteRenderer>();
        if (sprite)
        {
            return sprite.sprite;
        }

        SpriteMask spriteMask = t.GetCachedComponent<SpriteMask>();
        if (spriteMask)
        {
            return spriteMask.sprite;
        }
        return null;
    }


    public static void SetPercent(this Transform t, float val)
    {
        Image img = t.GetCachedComponent<Image>();
        if (img != null)
        {
            img.fillAmount = val;
        }
    }

    public static float GetPercent(this Transform t)
    {
        Image img = t.GetCachedComponent<Image>();
        if (img != null)
        {
            return img.fillAmount;
        }
        return 0.0f;
    }

    //图片上超过这个alpha值的地方才会接收点击事件(图片设置上面要勾选Read/Write Enable)
    public static void SetAlphaHit(this Transform t, float alpha)
    {
        Image img = t.GetCachedComponent<Image>();
        if (img != null)
        {
            img.alphaHitTestMinimumThreshold = alpha;
        }
    }

    public static void SetText(this Transform t, string str)
    {
        Text txt = t.GetCachedComponent<Text>();
        if (txt != null)
        {
            if (txt.text != str)
                txt.text = str;
            return;
        }

        TextMeshProUGUI pro = t.GetCachedComponent<TextMeshProUGUI>();
        if (pro != null)
        {
            if (pro.text != str)
                pro.text = str;
            return;
        }

        // ImageText imgtxt = t.GetComponent<ImageText>();
        // if (imgtxt != null)
        // {
        //     if (imgtxt.text != str)
        //         imgtxt.text = str;
        //     return;
        // }

        InputField input = t.GetCachedComponent<InputField>();
        if (input != null)
        {
            if (input.text != str)
                input.text = str;
            return;
        }

        TMP_InputField tMP_InputField = t.GetCachedComponent<TMP_InputField>();
        if (tMP_InputField != null)
        {
            if (tMP_InputField.text != str)
                tMP_InputField.text = str;
            return;
        }
    }

    public static void SetColor(this Transform t, Color color)
    {
        Text txt = t.GetCachedComponent<Text>();
        if (txt != null)
            txt.color = color;
        Image image = t.GetCachedComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }

        TextMeshProUGUI textMeshProUGUI = t.GetCachedComponent<TextMeshProUGUI>();
        if (textMeshProUGUI != null)
        {
            textMeshProUGUI.color = color;
        }
    }

    public static Color GetColor(this Transform t)
    {
        Text txt = t.GetCachedComponent<Text>();
        if (txt)
            return txt.color;

        return Color.white;
    }

    public static string GetText(this Transform t)
    {
        InputField input = t.GetCachedComponent<InputField>();
        if (input != null)
        {
            return input.text;
        }

        TMP_InputField tMP_InputField = t.GetCachedComponent<TMP_InputField>();
        if (tMP_InputField != null)
        {
            return tMP_InputField.text;
        }

        Text txt = t.GetCachedComponent<Text>();
        if (txt != null)
        {
            return txt.text;
        }

        // ImageText imgtxt = t.GetComponent<ImageText>();
        // if (imgtxt != null)
        // {
        //     return imgtxt.text;
        // }
        return "";
    }

    public static void SetFontSize(this Transform t, int sz)
    {
        Text txt = t.GetCachedComponent<Text>();
        if (txt != null)
        {
            txt.fontSize = sz;
        }
    }

    public static void SetTextOutline(this Transform t, byte r, byte g, byte b)
    {
        Text txt = t.GetCachedComponent<Text>();
        if (txt != null)
        {
            Outline ol = t.GetCachedComponent<Outline>();
            if (ol == null)
            {
                ol = t.gameObject.AddComponent<Outline>();
                t.CacheComponent(ol);
            }

            tempColor.r = r;
            tempColor.g = g;
            tempColor.b = b;
            tempColor.a = 255;
            ol.effectColor = tempColor;
        }
    }



    public static void SetTextAlignment(this Transform t, byte alignment)
    {
        Text txt = t.GetCachedComponent<Text>();
        if (txt != null)
        {
            txt.alignment = (TextAnchor)alignment;
        }
    }


    public static void ActivateInputField(this Transform t)
    {
        InputField input = t.GetCachedComponent<InputField>();
        if (input != null)
        {
            input.ActivateInputField();
        }
    }

    // public static void AddValueChangedListener(this Transform t, ClickEventTriggerListener.EventBoolDelegate action)
    // {   
    //     Toggle sel = t.GetComponent<Toggle>();
    //     if (sel != null)
    //     {
    //         sel.onValueChanged.RemoveAllListeners();
    //         sel.onValueChanged.AddListener((bool val) =>
    //         {
    //             action(val);
    //         });
    //     }
    // }

    public static void SetSelected(this Transform t, bool val)
    {
        Toggle tg = t.GetCachedComponent<Toggle>();
        if (tg)
        {
            tg.isOn = val;
        }
    }

    public static bool IsSelected(this Transform t)
    {
        Toggle tg = t.GetCachedComponent<Toggle>();
        if (tg)
        {
            return tg.isOn;
        }
        return false;
    }

    #region Slider
    public static void SetSliderValue(this Transform t, float val)
    {
        Slider s = t.GetCachedComponent<Slider>();
        if (s != null)
            s.value = val;
    }

    public static void SetSliderMinValueByTime(this Transform t, float val_0, float val, float time)
    {
        Slider s = t.GetCachedComponent<Slider>();
        if (s != null)
        {
            // 确保Slider初始值为0
            s.value = val_0;

            // 使用DOTween动画
            s.DOValue(val, time)
                .SetEase(Ease.Linear) // 设置缓动类型（这里使用线性）
                .OnComplete(() =>
                {

                });
        }
    }

    public static float GetSliderValue(this Transform t)
    {
        Slider s = t.GetCachedComponent<Slider>();
        return s != null ? s.value : 0f;


    }
    public static void SetSliderMinValue(this Transform t, float val)
    {
        Slider s = t.GetCachedComponent<Slider>();
        if (s != null)
            s.minValue = val;
    }

    public static void SetSliderMaxValue(this Transform t, float val)
    {
        Slider s = t.GetCachedComponent<Slider>();
        if (s != null)
            s.maxValue = val;
    }

    // public static void AddSliderValueChangedListener(this Transform t, ClickEventTriggerListener.EventFloatDelegate action)
    // {
    //     Slider s = t.GetComponent<Slider>();
    //     if (s != null)
    //     {
    //         s.onValueChanged.RemoveAllListeners();
    //         s.onValueChanged.AddListener((float val) =>
    //         {
    //             action(val);
    //         });
    //     }
    // }
    #endregion



    public static void PlaySound(this Transform t, string key)
    {
        AudioManager.Instance.PlaySoundTem(key);
    }

}
