﻿using UnityEngine;
using System.Collections.Generic;

public class MINI_TRIANGLE_POS
{
    public static int N = 1;
    public static int E = 2;
    public static int S = 4;
    public static int W = 8;
};

public class TRIANGLE_POS
{
    public static int NW = 9;
    public static int NE = 3;
    public static int SE = 6;
    public static int SW = 12;
};

public class MPTShape : MonoBehaviour
{
    public GameObject nextRotationPrefab;

    public Sprite mainSprite;
    public PolygonCollider2D polygonCollider;
    public MPTDraggable draggable;
    SpriteRenderer spriteRenderer;
    public float weight;
    public int multiplier;

    private float width;
    private float height;
    private int[,] miniTriangles;
    public bool isOnGrid;
    public bool isFullyInGrid;
    public bool canDrop;
    public bool hasBeenDropped;
    public int miniTrianglesCount;

    public Vector3 initialScale;

    void Start()
    {
        draggable = GetComponent<MPTDraggable>();
        draggable.beenDragged += AfterDrag;
        draggable.beenDropped += AfterDrop;
        /*
        mainSprite = GetComponent<SpriteRenderer>().sprite;
        width = mainSprite.bounds.extents.x;
        height = mainSprite.bounds.extents.y;
        miniTriangles = new int[(int)height, (int)width];

        Texture2D spriteTexture = mainSprite.texture;

        float spriteWidthPx = spriteTexture.width;
        float spriteHeightPx = spriteTexture.height;

        for (int currentX = 0; currentX < width; ++currentX)
        {
            for (int currentY = 0; currentY < height; ++currentY)
            {
                miniTriangles[currentX, currentY] = 0;
                int pxX = Mathf.RoundToInt(spriteWidthPx * (currentX + 0.5f) / width);
                int pxY = Mathf.RoundToInt(spriteHeightPx * (currentY + 0.25f) / height);
                Color pxColor = spriteTexture.GetPixel(pxX, pxY);
                if (pxColor.a != 0.0f)
                {
                    miniTriangles[currentX, currentY] |= MINI_TRIANGLE_POS.N;
                }

                pxX = Mathf.RoundToInt(spriteWidthPx * (currentX + 0.5f) / width);
                pxY = Mathf.RoundToInt(spriteHeightPx * (currentY + 0.75f) / height);
                pxColor = spriteTexture.GetPixel(pxX, pxY);
                if (pxColor.a != 0.0f)
                {
                    miniTriangles[currentX, currentY] |= MINI_TRIANGLE_POS.S;
                }

                pxX = Mathf.RoundToInt(spriteWidthPx * (currentX + 0.75f) / width);
                pxY = Mathf.RoundToInt(spriteHeightPx * (currentY + 0.5f) / height);
                pxColor = spriteTexture.GetPixel(pxX, pxY);
                if (pxColor.a != 0.0f)
                {
                    miniTriangles[currentX, currentY] |= MINI_TRIANGLE_POS.E;
                }

                pxX = Mathf.RoundToInt(spriteWidthPx * (currentX + 0.25f) / width);
                pxY = Mathf.RoundToInt(spriteHeightPx * (currentY + 0.5f) / height);
                pxColor = spriteTexture.GetPixel(pxX, pxY);
                if (pxColor.a != 0.0f)
                {
                    miniTriangles[currentX, currentY] |= MINI_TRIANGLE_POS.W;
                }
            }
        }



        List<Vector2> colliderPoints = new List<Vector2>();
        Vector2 lastPoint = new Vector2(-1, -1);
        for (int i = 0; i < height && lastPoint.x < 0; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                int triangle = miniTriangles[i, j];
                if (triangle != 0)
                {
                    lastPoint = new Vector2(i, j);
                    if (triangle == TRIANGLE_POS.SE)
                    {
                        colliderPoints.Add(new Vector2(j + 1, i));
                    }
                    else
                    {
                        colliderPoints.Add(new Vector2(j, i));
                    }
                    break;
                }
            }
        }*/

        MPTShapeManager.Instance.RegisterShape(this);
        //polygonCollider.points;
    }

    void Update()
    {
        UpdateColor();
        if (MPTGrid.Instance != null && MPTGrid.Instance.coll.IsTouching(polygonCollider))
        {
            isOnGrid = true;
        }
        else
        {
            isOnGrid = false;
        }
        CheckCanDrop();
    }

    public void Init()
    {
        initialScale = transform.localScale;
        transform.localScale = initialScale / 2.0f;
        hasBeenDropped = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void CheckCanDrop()
    {
        canDrop = true;
        foreach (MPTShape current in MPTShapeManager.Instance.listOfShapes)
        {
            if (current == this)
            {
                continue;
            }
            else
            {
                if (current.polygonCollider.IsTouching(polygonCollider))
                {
                    canDrop = false;
                    break;
                }
            }
        }
    }

    public void AfterDrag()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, draggable.initialPos.z - 1);
        transform.localScale = initialScale;
        UpdateColor();

        isFullyInGrid = true;
        foreach (Vector2 point in polygonCollider.points)
        {
            Vector2 localPos = transform.rotation * new Vector2(
                (polygonCollider.offset.x + point.x) * transform.localScale.x,
                (polygonCollider.offset.y + point.y) * transform.localScale.y);
            Vector2 worldPointPos = new Vector2(
                transform.position.x,
                transform.position.y) + localPos;
            if (MPTGrid.Instance.coll.OverlapPoint(worldPointPos) == false)
            {
                isFullyInGrid = false;
                break;
            }
        }

        if (isFullyInGrid)
        {
            transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), transform.position.z);
        }
    }

    public bool CheckGridHasSpace()
    {
        float xStart = MPTGrid.Instance.transform.position.x - MPTGrid.Instance.width / 2.0f;
        float yStart = MPTGrid.Instance.transform.position.y + MPTGrid.Instance.height / 2.0f;

        for (int gridX = 0; gridX <= MPTGrid.Instance.width; ++gridX)
        {
            for (int gridY = 0; gridY <= MPTGrid.Instance.height; ++gridY)
            {
                Vector2 centerPosition = new Vector2(xStart + gridX, yStart - gridY);
                Vector2[] points = new Vector2[polygonCollider.points.Length];

                for (int pointId = 0; pointId < points.Length; ++pointId)
                {
                    Vector2 localPos = transform.rotation * new Vector2(
                        (polygonCollider.offset.x + polygonCollider.points[pointId].x) * initialScale.x,
                        (polygonCollider.offset.y + polygonCollider.points[pointId].y) * initialScale.y);
                    Vector2 loopWorldPointPos = localPos + centerPosition;
                    points[pointId] = loopWorldPointPos;
                }
                bool notInGrid = false;
                foreach (Vector2 point in points)
                {
                    if (MPTGrid.Instance.coll.OverlapPoint(point) == false)
                    {
                        notInGrid = true;
                        break;
                    }
                }
                if (notInGrid)
                {
                    continue;
                }

                bool shapeWasHit = false;

                foreach (MPTShape current in MPTShapeManager.Instance.listOfShapes)
                {
                    if (current == this || current.hasBeenDropped == false)
                    {
                        continue;
                    }
                    else
                    {
                        // current contains this
                        for (int i = 0; i < points.Length; ++i)
                        {
                            if (current.polygonCollider.OverlapPoint(points[i]))
                            {
                                shapeWasHit = true;
                            }
                        }
                        if (shapeWasHit)
                        {
                            break;
                        }

                        Vector2[] currentPoints = new Vector2[current.polygonCollider.points.Length];
                        for (int pointId = 0; pointId < currentPoints.Length; ++pointId)
                        {
                            Vector2 localPosCurrent = current.transform.rotation * new Vector2(
                                (current.polygonCollider.offset.x + current.polygonCollider.points[pointId].x) * current.transform.localScale.x,
                                (current.polygonCollider.offset.y + current.polygonCollider.points[pointId].y) * current.transform.localScale.y);
                            Vector2 worldPointPosCurrent = new Vector2(
                                current.transform.position.x,
                                current.transform.position.y) + localPosCurrent;
                            currentPoints[pointId] = worldPointPosCurrent;
                        }

                        if (MPTUtils.PolygonsIntersect(points, currentPoints))
                        {
                            shapeWasHit = true;
                        }
                        /*
                        //this contains current
                        for (int j = 0; j < currentPoints.Length; ++j)
                        {
                            if (polygonCollider.OverlapPoint(currentPoints[j] + new Vector2(transform.position.x, transform.position.y) - centerPosition))
                            {
                                shapeWasHit = true;
                                break;
                            }
                        }
                        if (shapeWasHit)
                        {
                            break;
                        }

                        // some intersections
                        for (int i = 0; i < points.Length && shapeWasHit == false; ++i)
                        {
                            for (int j = 0; j < currentPoints.Length; ++j)
                            {
                                if (MPTUtils.SegmentIntersect(points[i], points[(i + 1) % points.Length], currentPoints[j], currentPoints[(j + 1) % currentPoints.Length]))
                                {
                                    shapeWasHit = true;
                                    break;
                                }
                            }
                        }*/
                        if (shapeWasHit)
                        {
                            break;
                        }
                    }
                }
                if (shapeWasHit == false)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void AfterDrop()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, draggable.initialPos.z);
        CheckCanDrop();
        if (isOnGrid && isFullyInGrid && canDrop)
        {
            //Destroy(draggable);
            //draggable.enabled = false;
            draggable.Reinit();
            draggable.additionalColliders.Clear();
            hasBeenDropped = true;
            MPTSpawner.Instance.ShapeDropped(this.gameObject);
            //MPTSpawner.Instance.SpawnNew();
            MPTGrid.Instance.ShapeDropped(this);
        }
        else
        {
            transform.position = draggable.initialPos;
            if (draggable.initialPos == draggable.transform.parent.position)
            {
                transform.localScale = initialScale / 2.0f;
            }
            else
            {
                transform.localScale = initialScale;
            }
        }
    }

    public void Consume(int currentMultiplier, bool keepShape)
    {
        MPTGameManager.Instance.ShapeConsumed(this, currentMultiplier);
        GameObject scorePrefab = Instantiate(MPTGameManager.Instance.fancyScorePrefab);
        scorePrefab.GetComponent<TextMesh>().text = "+" + currentMultiplier;
        scorePrefab.GetComponent<TextMesh>().color = MPTGameManager.Instance.multiplierColors[currentMultiplier];
        scorePrefab.transform.position = transform.position;
        
        if (keepShape == false)
        {
            MPTShapeManager.Instance.UnregisterShape(this);
            //Destroy(gameObject);
        }/*
        else
        {
            multiplier += currentMultiplier;
        }*/
    }

    public void SetMultiplier(int newMultiplier)
    {
        if (newMultiplier < MPTGameManager.Instance.multiplierColors.Count)
        {
            multiplier = newMultiplier;
            spriteRenderer.color = MPTGameManager.Instance.multiplierColors[multiplier];
        }
    }

    public void UpdateColor()
    {
        if (((hasBeenDropped && draggable.currentlyDragged == false) || canDrop) && (isFullyInGrid || draggable.currentlyDragged == false))
        {
            spriteRenderer.color = MPTGameManager.Instance.multiplierColors[multiplier];
        }
        else
        {
            spriteRenderer.color = MPTGameManager.Instance.cantDropColor;
        }
    }
}
