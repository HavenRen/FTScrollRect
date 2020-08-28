using System.Collections.Generic;
using UnityEngine;

namespace FT
{
    public class FTScrollRectMultSize : FTScrollRectBase
    {
        public FTNodeBase dhCell;
        public float spacing;
        public FTDirection direction;

        Stack<FTNodeBase> cellPool = new Stack<FTNodeBase>();
        List<FTNodeBase> activeCells = new List<FTNodeBase>();

        int instantiateCount;
        int cellCount;

        void Awake()
        {
            if (dhCell.gameObject.activeSelf)
            {
                dhCell.gameObject.SetActive(false);
            }
        }

        public override void Refill(int count)
        {
            cellCount = count;
            while (activeCells.Count != 0)
            {
                Pool(activeCells[0]);
            }
            ContentTrans.anchoredPosition = Vector2.zero;
            ContentTrans.sizeDelta = direction == FTDirection.Vertical ?
                new Vector2(ContentTrans.sizeDelta.x, 0) : new Vector2(0, ContentTrans.sizeDelta.y);
            TryFullFill();
        }

        public override void SetCount(int totalCount)
        {
            cellCount = totalCount;
            if (totalCount < activeCells.Count)
            {
                for (int i = activeCells.Count - 1; i > totalCount - 1; i--)
                {
                    Pool(activeCells[i]);
                }
            }
        }

        public override void Refresh()
        {
            int count = activeCells.Count;
            for (int i = 0; i < count; i++)
            {
                var cell = activeCells[i];
                RefreshCell(cell.dataIndex, cell.objIndex);
                var size = direction == FTDirection.Vertical ? cell.Height + spacing : cell.Width + spacing;
                for (int j = i + 1; j < count; j++)
                {
                    if (direction == FTDirection.Vertical)
                    {
                        activeCells[j].AnchoredPosition = new Vector2(activeCells[j].AnchoredPosition.x, activeCells[j].AnchoredPosition.y - size);
                    }
                    else
                    {
                        activeCells[j].AnchoredPosition = new Vector2(activeCells[j].AnchoredPosition.x - size, activeCells[j].AnchoredPosition.y);
                    }
                }

                if (direction == FTDirection.Vertical)
                {
                    ContentTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ContentTrans.sizeDelta.y + size);
                }
                else
                {
                    ContentTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ContentTrans.sizeDelta.x + size);
                }
            }
        }

        public override void Clear()
        {
            while (activeCells.Count != 0)
            {
                Pool(activeCells[0]);
            }
            ContentTrans.anchoredPosition = Vector2.zero;
            ContentTrans.sizeDelta = direction == FTDirection.Vertical ?
                new Vector2(ContentTrans.sizeDelta.x, 0) : new Vector2(0, ContentTrans.sizeDelta.y);
        }

        protected override void TryFullFill()
        {
            int i = 0;
            while (i < cellCount && CheckBorder())
            {
                i++;
            }
        }

        bool CheckBorder()
        {
            if (activeCells.Count == 0)
            {
                TryStartAdd();
                return false;
            }

            var cell = activeCells[0];
            var delta = direction == FTDirection.Vertical ?
                cell.AnchoredPosition.y + ContentTrans.anchoredPosition.y :
                cell.AnchoredPosition.x + ContentTrans.anchoredPosition.x;
            if (delta < 0 && TryAddTop())
            {
                return true;
            }

            cell = activeCells[activeCells.Count - 1];
            delta = direction == FTDirection.Vertical ?
                cell.AnchoredPosition.y - cell.Height - spacing + ContentTrans.anchoredPosition.y :
                cell.AnchoredPosition.x - cell.Width - spacing + ContentTrans.anchoredPosition.x;
            if (delta > -ViewPortTrans.sizeDelta.y && TryAddBottom())
            {
                return true;
            }

            if (activeCells.Count <= 1)
            {
                return false;
            }

            if (NeedPull(activeCells[0]))
            {
                Pool(activeCells[0]);
                return true;
            }

            if (NeedPull(activeCells[activeCells.Count - 1]))
            {
                Pool(activeCells[activeCells.Count - 1]);
                return true;
            }

            return false;
        }

        void TryStartAdd()
        {
            if (cellCount == 0)
            {
                return;
            }
            var cell = GetCell();
            cell.dataIndex = 0;
            RefreshCell(cell.dataIndex, cell.objIndex);
            cell.CachedRectTransform.anchoredPosition = Vector2.zero;
            if (direction == FTDirection.Vertical)
            {
                ContentTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ContentTrans.sizeDelta.y + cell.Height + spacing);
            }
            else
            {
                ContentTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ContentTrans.sizeDelta.x + cell.Width + spacing);
            }
            activeCells.Add(cell);
            cell.gameObject.SetActive(true);
        }

        bool TryAddBottom()
        {
            var downCell = activeCells[activeCells.Count - 1];
            var downIndex = downCell.dataIndex;
            if (downIndex + 1 == cellCount)
            {
                return false;
            }

            var cell = GetCell();
            cell.dataIndex = downIndex + 1;
            RefreshCell(cell.dataIndex, cell.objIndex);

            Vector2 pos;
            if (direction == FTDirection.Vertical)
            {
                var d1 = downCell.AnchoredPosition.y;
                var d2 = downCell.Height + spacing;
                pos = new Vector2(0.0f, d1 - d2);
                cell.CachedRectTransform.anchoredPosition = pos;
                var d3 = cell.Height + spacing;
                if (ContentTrans.sizeDelta.y <= -cell.AnchoredPosition.y + d3 * 0.5f)
                {
                    ContentTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ContentTrans.sizeDelta.y + d3);
                }
            }
            else
            {
                var d1 = downCell.AnchoredPosition.x;
                var d2 = downCell.Width + spacing;
                pos = new Vector2(d1 - d2, 0.0f);
                cell.CachedRectTransform.anchoredPosition = pos;
                var d3 = cell.Width + spacing;
                if (ContentTrans.sizeDelta.x <= -cell.AnchoredPosition.x + d3 * 0.5f)
                {
                    ContentTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ContentTrans.sizeDelta.x + d3);
                }
            }

            activeCells.Add(cell);
            cell.gameObject.SetActive(true);
            return true;
        }

        bool TryAddTop()
        {
            var topCell = activeCells[0];
            var topIndex = topCell.dataIndex;
            if (topIndex == 0)
            {
                return false;
            }

            var cell = GetCell();
            cell.dataIndex = topIndex - 1;
            RefreshCell(cell.dataIndex, cell.objIndex);

            var d1 = direction == FTDirection.Vertical ?
                cell.Height + spacing : cell.Width + spacing;
            var pos = direction == FTDirection.Vertical ?
                new Vector2(0.0f, topCell.AnchoredPosition.y + d1) : new Vector2(topCell.AnchoredPosition.x + d1, 0.0f);
            cell.CachedRectTransform.anchoredPosition = pos;
            activeCells.Insert(0, cell);
            cell.gameObject.SetActive(true);
            return true;
        }

        FTNodeBase GetCell()
        {
            if (cellPool.Count > 0)
            {
                return cellPool.Pop();
            }
            else
            {
                var t = Instantiate(dhCell.CachedRectTransform);
                t.SetParent(ContentTrans);
                t.localScale = Vector3.one;
                t.anchoredPosition3D = Vector3.zero;
                var newCell = t.GetComponent<FTNodeBase>();
                newCell.objIndex = instantiateCount;
                OnInstantiateCell(newCell);
                instantiateCount++;
                return newCell;
            }
        }

        void Pool(FTNodeBase cell)
        {
            activeCells.Remove(cell);
            cell.gameObject.SetActive(false);
            cellPool.Push(cell);
        }

        bool NeedPull(FTNodeBase cell)
        {
            if (activeCells.IndexOf(cell) == 0)
            {
                var d1 = direction == FTDirection.Vertical ?
                    cell.AnchoredPosition.y - cell.Height - spacing + ContentTrans.anchoredPosition.y :
                    cell.AnchoredPosition.x - cell.Width - spacing + ContentTrans.anchoredPosition.x;
                if (d1 > 0.01f)
                {
                    return true;
                }
            }

            var d2 = direction == FTDirection.Vertical ?
                cell.AnchoredPosition.y + ContentTrans.anchoredPosition.y :
                cell.AnchoredPosition.x + ContentTrans.anchoredPosition.x;
            var d3 = direction == FTDirection.Vertical ? -ViewPortTrans.sizeDelta.y : -ViewPortTrans.sizeDelta.x;
            if (d2 < d3 - 0.01f)
            {
                return true;
            }

            if (cell.dataIndex >= cellCount)
            {
                return true;
            }

            return false;
        }
    }
}