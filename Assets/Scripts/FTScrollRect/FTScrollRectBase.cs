using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FT
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    public abstract class FTScrollRectBase : MonoBehaviour
    {
        bool contentInited = false;
        RectTransform contentTrans;
        protected RectTransform ContentTrans
        {
            get
            {
                if (!contentInited)
                {
                    contentTrans = GetComponent<ScrollRect>().content;
                    if (!ReferenceEquals(contentTrans, null))
                    {
                        contentInited = true;
                    }
                }
                return contentTrans;
            }
        }

        bool viewportTransInited = false;
        RectTransform viewportTrans;
        protected RectTransform ViewPortTrans
        {
            get
            {
                if (!viewportTransInited)
                {
                    viewportTrans = GetComponent<ScrollRect>().viewport;
                    if (!ReferenceEquals(viewportTrans, null))
                    {
                        viewportTransInited = true;
                    }
                }
                return viewportTrans;
            }
        }

        List<FTNodeBase> cells = new List<FTNodeBase>();
        Action<FTNodeBase> InitAction;
        protected Action<int, FTNodeBase> RefreshAction;

        public void SetInitAction(Action<FTNodeBase> action)
        {
            InitAction = action;
        }

        public void SetRefreshAction(Action<int, FTNodeBase> action)
        {
            RefreshAction = action;
        }

        protected void RefreshCell(int dataIndex, int objIndex)
        {
            var cell = cells[objIndex];
            RefreshAction?.Invoke(dataIndex, cell);
        }

        protected void OnInstantiateCell(FTNodeBase cell)
        {
            InitAction?.Invoke(cell);
            cells.Add(cell);
        }

        protected abstract void TryFullFill();

        public abstract void Refill(int count);

        public abstract void SetCount(int count);

        public abstract void Refresh();

        public abstract void Clear();

        void Update()
        {
            TryFullFill();
        }
    }
}