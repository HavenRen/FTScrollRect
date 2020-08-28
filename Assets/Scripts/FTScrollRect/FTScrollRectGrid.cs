using System.Collections.Generic;
using UnityEngine;

namespace FT
{
    public class FTScrollRectGrid : FTScrollRectBase
    {
        public FTNodeBase dhCell;
        public float spacing;
        public float padding;
        public int preLineCount = 4;
        public FTDirection direction;

        Stack<FTNodeBase> cellPool = new Stack<FTNodeBase>();
        List<FTNodeBase> activeCells = new List<FTNodeBase>();

        float lineSize;
        float cellSize;
        int instantiateCount;
        int cellCount;
        int topFillCount;
        int bottomFillCount;

        void Awake()
        {
            lineSize = direction == FTDirection.Vertical ? dhCell.Height + spacing : dhCell.Width + spacing;
            cellSize = direction == FTDirection.Vertical ? dhCell.Width + padding : dhCell.Height + padding;
            if (dhCell.gameObject.activeSelf)
            {
                dhCell.gameObject.SetActive(false);
            }
        }

        int GetTotalLines(int cellCount)
        {
            if (cellCount % preLineCount == 0)
            {
                return cellCount / preLineCount;
            }
            return cellCount / preLineCount + 1;
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
                ContentTrans.sizeDelta = new Vector2(ContentTrans.sizeDelta.x, lineSize * GetTotalLines(count) - spacing);
            }
            else
            {
                ContentTrans.sizeDelta = new Vector2(lineSize * GetTotalLines(count) - spacing, ContentTrans.sizeDelta.y);
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
                ContentTrans.sizeDelta = new Vector2(ContentTrans.sizeDelta.x, lineSize * GetTotalLines(count) - spacing);
            }
            else
            {
                ContentTrans.sizeDelta = new Vector2(lineSize * count - spacing, ContentTrans.sizeDelta.y);
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

            if (topFillCount < preLineCount && TryAddTop())
            {
                return true;
            }

            cell = activeCells[activeCells.Count - 1];
            delta = direction == FTDirection.Vertical ?
                cell.AnchoredPosition.y - lineSize + ContentTrans.anchoredPosition.y :
                cell.AnchoredPosition.x - lineSize + ContentTrans.anchoredPosition.x;
            if (delta > -ViewPortTrans.sizeDelta.y && TryAddBottom())
            {
                return true;
            }

            if (bottomFillCount < preLineCount && TryAddBottom())
            {
                return true;
            }

            if (activeCells.Count <= preLineCount)
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
            topFillCount = 1;
            RefreshCell(cell.dataIndex, cell.objIndex);
            cell.CachedRectTransform.anchoredPosition = Vector2.zero;
            activeCells.Add(cell);
            cell.gameObject.SetActive(true);
        }

        bool TryAddTop()
        {
            var topIndex = activeCells[0].dataIndex;
            if (topIndex == 0 && (topFillCount == preLineCount || activeCells.Count == cellCount))
            {
                return false;
            }

            var cell = GetCell();
            if (topFillCount == preLineCount)
            {
                topFillCount = 0;
                cell.dataIndex = topIndex - preLineCount;
            }
            else
            {
                cell.dataIndex = topIndex + topFillCount;
            }

            RefreshCell(cell.dataIndex, cell.objIndex);
            var x = direction == FTDirection.Vertical ? topFillCount * cellSize : -GetLineIndex(cell.dataIndex) * lineSize;
            var y = direction == FTDirection.Vertical ? -GetLineIndex(cell.dataIndex) * lineSize : topFillCount * cellSize;
            cell.CachedRectTransform.anchoredPosition = new Vector2(x, y);
            cell.gameObject.SetActive(true);
            topFillCount++;

            if (topFillCount == activeCells.Count)
            {
                activeCells.Add(cell);
            }
            else
            {
                activeCells.Insert(topFillCount - 1, cell);
            }

            return true;
        }

        bool TryAddBottom()
        {
            var downIndex = activeCells[activeCells.Count - 1].dataIndex;
            if (downIndex + 1 == cellCount)
            {
                return false;
            }

            if (bottomFillCount == preLineCount)
            {
                bottomFillCount = 0;
            }

            var cell = GetCell();
            cell.dataIndex = downIndex + 1;
            RefreshCell(cell.dataIndex, cell.objIndex);
            var x = direction == FTDirection.Vertical ? bottomFillCount * cellSize : -GetLineIndex(cell.dataIndex) * lineSize;
            var y = direction == FTDirection.Vertical ? -GetLineIndex(cell.dataIndex) * lineSize : bottomFillCount * cellSize;
            cell.CachedRectTransform.anchoredPosition = new Vector2(x, y);
            activeCells.Add(cell);
            cell.gameObject.SetActive(true);
            bottomFillCount++;
            return true;
        }

        int GetLineIndex(int cellIndex)
        {
            return cellIndex / preLineCount;
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

        bool NeedPool(FTNodeBase cell)
        {
            if (activeCells.IndexOf(cell) == 0)
            {
                var d1 = direction == FTDirection.Vertical ?
                    cell.AnchoredPosition.y - lineSize + ContentTrans.anchoredPosition.y :
                    cell.AnchoredPosition.x - lineSize + ContentTrans.anchoredPosition.x;
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