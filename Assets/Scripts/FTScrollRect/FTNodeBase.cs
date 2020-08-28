using UnityEngine;

namespace FT
{
    public class FTNodeBase : MonoBehaviour
    {
        public Vector2 AnchoredPosition
        {
            get
            {
                return CachedRectTransform.anchoredPosition;
            }
            set
            {
                CachedRectTransform.anchoredPosition = value;
            }
        }

        public Vector2 SizeDelta
        {
            get
            {
                return CachedRectTransform.sizeDelta;
            }
            set
            {
                CachedRectTransform.sizeDelta = value;
            }
        }

        public float Width
        {
            get
            {
                return CachedRectTransform.sizeDelta.x * CachedRectTransform.localScale.x;
            }
            set
            {
                CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
            }
        }

        public float Height
        {
            get
            {
                return CachedRectTransform.sizeDelta.y * CachedRectTransform.localScale.y;
            }
            set
            {
                CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
            }
        }

        bool rectTransformInited = false;
        RectTransform cachedRectTransform;
        public RectTransform CachedRectTransform
        {
            get
            {
                if (!rectTransformInited)
                {
                    cachedRectTransform = GetComponent<RectTransform>();
                    if (cachedRectTransform != null)
                    {
                        rectTransformInited = true;
                    }
                }
                return cachedRectTransform;
            }
        }

        [System.NonSerialized]
        public int dataIndex;

        [System.NonSerialized]
        public int objIndex;
    }
}
