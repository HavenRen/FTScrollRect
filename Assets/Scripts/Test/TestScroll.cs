using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using FT;
using DG.Tweening;

public class Delayed
{
    public float time;
    public float delay;
    public Action action;
}

public class TestScroll : MonoBehaviour
{
    Delayed delayed;

    public int tweenCount;

    void Start()
    {
        var datas = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            var word = i + 1;
            datas.Add(word.ToString());
        }

        var ftScrollRect = GetComponent<FTScrollRectBase>();

        ftScrollRect.SetInitAction((cell) =>
        {
            var testCell = cell as TestCell;
            if (delayed != null)
            {
                testCell.canvasGroup.alpha = 0;
                var tween = testCell.canvasGroup.DOFade(1f, 0.25f);
                tween.SetEase(Ease.InOutCirc);
            }
        });

        ftScrollRect.SetRefreshAction((index, cell) =>
        {
            var testCell = cell as TestCell;
            testCell.Refresh(datas[index]);
        });

        ftScrollRect.Refill(0);

        var current = 0;
        StartDelay(0.1f, () =>
        {
            current++;
            ftScrollRect.SetCount(current + 1);
            if (current == tweenCount)
            {
                StopDelay();
                ftScrollRect.SetCount(datas.Count);
            }
        });
    }

    void StartDelay(float time, Action action)
    {
        delayed = new Delayed
        {
            time = Time.time + time,
            action = action,
            delay = time,
        };
    }

    void StopDelay()
    {
        delayed = null;
    }

    public void Update()
    {
        if (delayed != null)
        {
            if (delayed.time <= Time.time)
            {
                delayed.time = Time.time + delayed.delay;
                delayed.action();
            }
        }
    }
}
