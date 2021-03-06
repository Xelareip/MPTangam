﻿using UnityEngine;
using System.Collections;

public class MPTKeepShapeAnimation : MonoBehaviour
{
	private static int animationsHappening = 0;

	public static int GetAnimationsCount()
	{
		return animationsHappening;
	}

    public float animationTime;
    
    public Vector3 targetPosition;
    private Vector3 initialPosition;
    public Vector3 targetScale;
    private Vector3 initialScale;

    private float startTime;

	void Awake()
	{
		++animationsHappening;
	}

	void OnDestroy()
	{
		--animationsHappening;
	}

	void Start ()
    {
        initialPosition = transform.position;
        initialScale = transform.localScale;
        startTime = Time.time;
    }

    void Update()
    {
        if ((Time.time - startTime) > animationTime)
        {
			transform.position = targetPosition;
			transform.localScale = targetScale;
            Destroy(this);
            return;
        }
        transform.position = initialPosition + (targetPosition - initialPosition) * (Time.time - startTime) / animationTime;
        transform.localScale = initialScale + (targetScale - initialScale) * (Time.time - startTime) / animationTime;
    }
}
