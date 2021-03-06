﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MPTDelayedCounter : MonoBehaviour
{
    public Text textDisplay;

    public float currentScore;
    private int targetScore;

    public float currentScale;
    public float maxScale;

    public float maxSpeed;
    public float speed;
    public float startDelay;
    public float duration;

    public bool canUpdate;
    public float canUpdateAt;
    public float finishUpdateAt;

    void Update()
    {
        if (canUpdateAt < Time.time)
        {
            canUpdate = true;
        }

        //if (Mathf.Abs(targetScore - currentScore) > 0.25f && canUpdate)
        //{
        //    if (Mathf.Abs(targetScore - currentScore) < 5)
        //    {
        //        currentScore = Mathf.Lerp(currentScore, targetScore, speed * Time.deltaTime);
        //    }
        //    else
        //    {
        //        currentScore += maxSpeed * Time.deltaTime;
        //    }
        if (canUpdate && currentScore != targetScore)
        {
            if (currentScore < targetScore)
            {
                currentScore = Mathf.Min(currentScore + speed * Time.deltaTime, targetScore);
            }
            currentScale = Mathf.Lerp(currentScale, maxScale, speed * Time.deltaTime);
        }
        else
        {
            canUpdate = false;
            currentScale = Mathf.Lerp(currentScale, 1.0f, speed * Time.deltaTime);
        }
        textDisplay.text = string.Format("{0}", Mathf.RoundToInt(currentScore));
        transform.localScale = Vector3.one * currentScale;
    }

    public void SetScore(int target)
    {
        targetScore = target;
        if (targetScore < currentScore)
        {
            currentScore = targetScore;
        }
        if (canUpdate == false && canUpdateAt < Time.time)
        {
            canUpdateAt = Time.time + startDelay;
        }
        speed = (targetScore - currentScore) / duration;
    }
}
