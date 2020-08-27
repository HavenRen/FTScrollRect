using System.Collections.Generic;
using UnityEngine;

namespace FT
{
    public class FTScrollRectCommon : FTScrollRectBase
    {
        public FTCellBase dhCell;
        public float spacing;
        public FTDirection direction;

        Stack<FTCellBase> cellPool = new Stack<FTCellBase>();
        List<FTCellBase> activeCells = new List<FTCellBase>();

        float cellSize;
        int instantiateCount;
        int cellCount;

        void Awake()
        {
            cellSize = direction == FTDirection.Vertical ? dhCell.Height + spacing : dhCell.Width + spacing;
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
            if (direction == FTDirection.Vertical)
            {
                ContentTrans.sizeDelta = new Vector2(ContentTrans.sizeDelta.x, cellSize * count - spacing);
            }
            else
            {
                ContentTrans.sizeDelta = new Vector2(cellSize * count - spacing, ContentTrans.sizeDelta.y);
            }
            TryFullFill();
        }

        public override void SetCount(int count)
        {
            cellCount = count;
            if (count < activeCells.Count)
            {
                for (int i = activeCells.Count - 1; i > count - 1; i--)
                {
                    Pool(activeCells[i]);
                }
            }

            if (direction == FTDirection.Vertical)
            {
                ContentTrans.sizeDelta = new Vector2(ContentTrans.sizeDelta.x, cellSize * count - spacing);
            }
            else
            {
                ContentTrans.sizeDelta = new Vector2(cellSize * count - spacing, ContentTrans.sizeDelta.y);
            }
        }

        public override void Refresh()
        {
            int count = activeCells.Count;
            for (int i = 0; i < count; i++)
            {
                var cell = activeCells[i];
                RefreshCell(cell.dataIndex, cell.objIndex);
            }
        }

        public override void Clear()
        {
            while (activeCells.Count != 0)
            {
                Pool(activeCells[0]);
            }
            ContentTrans.anchoredPosition = Vector2.zero;
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
                cell.AnchoredPosition.y - cellSize + ContentTrans.anchoredPosition.y :
                cell.AnchoredPosition.x - cellSize + ContentTrans.anchoredPosition.x;
            if (delta > -ViewPortTrans.sizeDelta.y && TryAddBottom())
            {
                return true;
            }

            if (activeCells.Count <= 1)
            {
                return false;
            }

            if (NeedPool(activeCells[0]))
            {
                Pool(activeCells[0]);
                return true;
            }

            if (NeedPool(activeCells[activeCells.Count - 1]))
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
            activeCells.Add(cell);
            cell.gameObject.SetActive(true);
        }

        bool TryAddBottom()
        {
            var downIndex = activeCells[activeCells.Count - 1].dataIndex;
            if (downIndex + 1 == cellCount)
            {
                return false;
            }

            var cell = GetCell();
            cell.dataIndex = downIndex + 1;
            RefreshCell(cell.dataIndex, cell.objIndex);
            var pos = direction == FTDirection.Vertical ?
                new Vector2(0.0f, (-downIndex - 1) * cellSize) :new Vector2((-downIndex - 1) * cellSize, 0.0f);
            cell.CachedRectTransform.anchoredPosition = pos;
            activeCells.Add(cell);
            cell.gameObject.SetActive(true);
            return true;
        }

        bool TryAddTop()
        {
            var topIndex = activeCells[0].dataIndex;
            if (topIndex == 0)
            {
                return false;
            }

            var cell = GetCell();
            cell.dataIndex = topIndex - 1;
            RefreshCell(cell.dataIndex, cell.objIndex);
            var pos = direction == FTDirection.Vertical ?
                new Vector2(0.0f, -(topIndex - 1) * cellSize) :new Vector2(-(topIndex - 1) * cellSize, 0.0f);
            cell.CachedRectTransform.anchoredPosition = pos;
            activeCells.Insert(0, cell);
            cell.gameObject.SetActive(true);
            return true;
        }

        FTCellBase GetCell()
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
                var newCell = t.GetComponent<FTCellBase>();
                newCell.objIndex = instantiateCount;
                OnInstantiateCell(newCell);
                instantiateCount++;
                return newCell;
            }
        }

        void Pool(FTCellBase cell)
        {
            activeCells.Remove(cell);
            cell.gameObject.SetActive(false);
            cellPool.Push(cell);
        }

        bool NeedPool(FTCellBase cell)
        {
            if (activeCells.IndexOf(cell) == 0)
            {
                var d1 = direction == FTDirection.Vertical ?
                    cell.AnchoredPosition.y - cellSize + ContentTrans.anchoredPosition.y :
                    cell.AnchoredPosition.x - cellSize + ContentTrans.anchoredPosition.x;
                if (d1 > 0.01f)
                {
                    return true;
                }
            }

            var d2 = direction == FTDirection.Vertical ?
                cell.AnchoredPosition.y + ContentTrans.anchoredPosition.y :
                cell.AnchoredPosition.x + ContentTrans.anchoredPosition.x;
            var d3 = direction == FTDirection.Vertical ?
                -ViewPortTrans.sizeDelta.y : -ViewPortTrans.sizeDelta.x;
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