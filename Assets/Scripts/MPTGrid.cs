﻿using UnityEngine;
using System.Collections.Generic;

public class MPTGrid : MonoBehaviour
{
    private static MPTGrid instance;
    public static MPTGrid Instance
    {
        get
        {
            return instance;
        }
    }

    public Collider2D coll;
    public int width;
    public int height;
    public MPTShape[,] shapesTab;

    public GameObject spawnText;

    void Start()
    {
        instance = this;
        coll = GetComponent<Collider2D>();
        shapesTab = new MPTShape[height * 2, width];
    }

    public void ResetGrid()
    {
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height * 2; ++j)
            {
                shapesTab[j, i] = null;
            }
        }
    }

    public void ShapeDropped(MPTShape shape)
    {
        float xStart = transform.position.x - width / 2.0f + 0.5f;
        float yStart = transform.position.y + height / 2.0f - 0.25f;

        int miniTrianglesCount = 0;
        
        for (int shapeX = 0; shapeX < width; ++shapeX)
        {
            for (int shapeY = 0; shapeY < height * 2; ++shapeY)
            {
                if (shapesTab[shapeY, shapeX] == shape)
                {
                    shapesTab[shapeY, shapeX] = null;
                }
            }
        }

        for (int shapeX = 0; shapeX < width; ++shapeX)
        {
            for (int shapeY = 0; shapeY < height * 2; ++shapeY)
            {
                Vector2 point = new Vector2(xStart + shapeX, yStart - shapeY * 0.5f);
                if (shape.polygonCollider.OverlapPoint(point))
                {
                    shapesTab[shapeY, shapeX] = shape;
                    ++miniTrianglesCount;
                }
            }
        }

        shape.miniTrianglesCount = miniTrianglesCount;

		if (MPTQuickSave.Instance.IsLoading)
		{
			return;
		}

		List<List<MPTShape>> squares = new List<List<MPTShape>>();

        for (int xNW = 0; xNW < width; ++xNW)
        {
            for (int yNW = 0; yNW < height; ++yNW)
            {
                for (int squareWidth = 1; squareWidth < width - xNW + 1; ++squareWidth)
                {
                    for (int squareHeight = 1; squareHeight < height - yNW + 1; ++squareHeight)
                    {
                        bool brokenSquare = false;
                        Dictionary<MPTShape, int> shapeToCount = new Dictionary<MPTShape, int>();
                        for (int squareX = 0; squareX < squareWidth; ++squareX)
                        {
                            for (int squareY = 0; squareY < squareHeight; ++squareY)
                            {
                                Vector2 miniSquarePos = new Vector2(xNW + squareX, yNW + squareY);
                                if (shapesTab[(int)miniSquarePos.y * 2, (int)miniSquarePos.x] == null || shapesTab[(int)miniSquarePos.y * 2 + 1, (int)miniSquarePos.x] == null)
                                {
                                    brokenSquare = true;
                                    break;
                                }
                                else
                                {
                                    MPTShape shape1 = shapesTab[(int)miniSquarePos.y * 2, (int)miniSquarePos.x];
                                    MPTShape shape2 = shapesTab[(int)miniSquarePos.y * 2 + 1, (int)miniSquarePos.x];

                                    if (shapeToCount.ContainsKey(shape1))
                                    {
                                        shapeToCount[shape1] = shapeToCount[shape1] + 1;
                                    }
                                    else
                                    {
                                        shapeToCount.Add(shape1, 1);
                                    }

                                    if (shapeToCount.ContainsKey(shape2))
                                    {
                                        shapeToCount[shape2] = shapeToCount[shape2] + 1;
                                    }
                                    else
                                    {
                                        shapeToCount.Add(shape2, 1);
                                    }
                                }
                            }
                            if (brokenSquare)
                            {
                                break;
                            }
                        }
                        if (brokenSquare)
                        {
                            continue;
                        }
                        if (shapeToCount.Count < 3)
                        {
                            continue;
                        }
                        squares.Add(new List<MPTShape>());
                        foreach (KeyValuePair<MPTShape, int> kvp in shapeToCount)
                        {
                            if (kvp.Key.miniTrianglesCount != kvp.Value)
                            {
                                brokenSquare = true;
                                break;
                            }
                            else
                            {
                                squares[squares.Count - 1].Add(kvp.Key);
                            }
                        }
                        if (brokenSquare)
                        {
                            squares.RemoveAt(squares.Count - 1);
                        }
                    }
                }
            }
        }

        List<MPTShape> finalSquare = new List<MPTShape>();

        foreach (List<MPTShape> square in squares)
        {
            if (square.Count > finalSquare.Count)
            {
                finalSquare = square;
            }
        }
        if (finalSquare.Count >= 3)
        {
            MPTSpawner.Instance.SquareDone();
        }
        else
        {
			MPTSpawner.Instance.SpawnNew();
            return;
        }


        int shapeToKeep = Random.Range(0, finalSquare.Count);

        int multiplier = finalSquare.Count;
        int highestMultiplier = 0;
        /*
        foreach (MPTShape currentShape in finalSquare)
        {
            multiplier += currentShape.multiplier;
            highestMultiplier = Mathf.Max(highestMultiplier, currentShape.multiplier);
        }*/

        int shapeToKeepPosition = shapeToKeep;
        /*shapeToKeep = -1;
        
        int multipliedShape = Random.Range(0, MPTSpawner.Instance.spawnedShapes.Length);

        MPTSpawner.Instance.spawnedShapes[multipliedShape].GetComponent<MPTShape>().multiplier = highestMultiplier + 1;
        GameObject spawnTextMultiplier = Instantiate<GameObject>(spawnText);
        spawnTextMultiplier.GetComponent<TextMesh>().text = "x " + (highestMultiplier + 2);
        spawnTextMultiplier.transform.position = MPTSpawner.Instance.spawnedShapes[multipliedShape].transform.position + Vector3.back;*/
		if (MPTInteractiveTutoManager.Instance.GetKeepableShape() != "")
		{
			foreach (MPTShape currentShape in finalSquare)
			{
				bool shouldKeepThisShape = currentShape.name == MPTInteractiveTutoManager.Instance.GetKeepableShape();
                currentShape.Consume(multiplier, shouldKeepThisShape);

				if (shouldKeepThisShape)
				{
					MPTSpawner.Instance.SetNextShape(currentShape);
					currentShape.SetMultiplier(highestMultiplier + 1);
					currentShape.points = multiplier;
					currentShape.UpdateColor();
				}
				else
				{
					MPTSquareDoneAnimation anim = currentShape.gameObject.AddComponent<MPTSquareDoneAnimation>();
					anim.targetPosition = finalSquare[shapeToKeepPosition].transform.position + Vector3.forward;
					anim.targetScale = Vector3.zero;
					anim.animationTime = 1.0f;
				}
				--shapeToKeep;
			}

		}
		else
		{
			foreach (MPTShape currentShape in finalSquare)
			{
				bool shouldKeepThisShape = shapeToKeep == 0;
                currentShape.Consume(multiplier, shouldKeepThisShape);

				if (shouldKeepThisShape)
				{
					MPTSpawner.Instance.SetNextShape(currentShape);
					currentShape.SetMultiplier(highestMultiplier + 1);
					currentShape.points = multiplier;
					currentShape.UpdateColor();
				}
				else
				{
					MPTSquareDoneAnimation anim = currentShape.gameObject.AddComponent<MPTSquareDoneAnimation>();
					anim.targetPosition = finalSquare[shapeToKeepPosition].transform.position + Vector3.forward;
					anim.targetScale = Vector3.zero;
					anim.animationTime = 1.0f;
				}
				--shapeToKeep;
			}

		}

        int multiplierColorId = Mathf.Clamp(multiplier - 1, 0, MPTGameManager.Instance.multiplierColors.Count - 1);

        GameObject amazingPrefabToSpawn = null;
        for (int amazingPrefabId = 0; amazingPrefabId < finalSquare.Count && amazingPrefabId < MPTGameManager.Instance.amazingPrefabs.Count; ++amazingPrefabId)
        {
            if (MPTGameManager.Instance.amazingPrefabs[amazingPrefabId] != null)
            {
                amazingPrefabToSpawn = MPTGameManager.Instance.amazingPrefabs[amazingPrefabId];
            }
        }
        if (amazingPrefabToSpawn != null)
        {
            GameObject amazingPrefab = (GameObject)Instantiate(amazingPrefabToSpawn, new Vector3(transform.position.x, transform.position.y, finalSquare[0].transform.position.z - 2), Quaternion.identity);
            TextMesh textMesh = amazingPrefab.GetComponentInChildren<TextMesh>();
            textMesh.color = MPTGameManager.Instance.multiplierColors[multiplierColorId];
            textMesh.text = string.Format(textMesh.text, multiplier);
        }
	}
}
