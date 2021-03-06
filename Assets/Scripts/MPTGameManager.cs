﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class MPTGameManager : MonoBehaviour
{
    private static MPTGameManager instance;
    public static MPTGameManager Instance
    {
        get
        {
            return instance;
        }
    }

	public Text dropsLeftText;

    public GameObject fancyScorePrefab;
    public List<GameObject> amazingPrefabs;

    public MPTDelayedCounter scoreText;
    public Text bestScoreText;
    public Text scoreFinalText;
    public Text bestScoreFinalText;
    public Text trashPriceText;
    public GameObject looseScreen;
    public GameObject debugUI;
    public GameObject startUI;
    public GameObject infoUI;
    public GameObject confirmRestartUI;
    public GameObject tutoUI;
    public Transform debugPosition;
    public Transform gamePosition;
    public GameObject debugWeightModel;
    public int score;
    public int currentTopScore;
    public int trashPrice;

    public bool isPaused = false;

    public Color cantDropColor;
    public List<Color> multiplierColors;

	public int maxDropsLeft;
	public int dropsLeft;

	public Coroutine checkSpotsCoroutine;
	public GameObject endCloseWarning;

	void Start()
    {
        instance = this;
        score = 0;
        trashPrice = 1;
        UpdateScoreText();
    }

    public void StartGame()
    {
        isPaused = false;
		Destroy(startUI);
		UpdateAlmostEnd(false);
		ResetDropsLeft();
        if (MPTPlayer.Instance.GetTutoDone() == false)
        {
			MPTInteractiveTutoManager.Instance.StartTuto("Tutorial0");
		}
		else if (MPTQuickSave.Instance.HasSave())
		{
			MPTQuickSave.Instance.QuickLoad();
			return;
		}

		MPTPlayer.Instance.NewGameStarted();
		MPTSpawner.Instance.SpawnNew();
		MPTSpawner.Instance.SpawnNew();
		MPTSpawner.Instance.SpawnNew();
	}
    
    public void Loose()
    {
        Resume();
        isPaused = true;
        int shapeId = MPTShapeManager.Instance.listOfShapes.Count - 1;
        while (shapeId >= 0)
        {
            MPTShape shape = MPTShapeManager.Instance.listOfShapes[shapeId];
            if (shape.hasBeenDropped)
            {
                shape.Consume(1, false);
                MPTSquareDoneAnimation anim = shape.gameObject.AddComponent<MPTSquareDoneAnimation>();
                anim.targetScale = Vector3.zero;
                anim.animationTime = 1.0f;
            }
            --shapeId;
        }
		MPTQuickSave.Instance.Erase();
        StartCoroutine(ShowLooseScreenIn(4.0f));
    }

    IEnumerator ShowLooseScreenIn(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPaused = true;
        looseScreen.SetActive(true);
    }

    public void ShapeConsumed(MPTShape shape, int multiplier)
    {
        score += multiplier * shape.points;
        if (score > currentTopScore)
        {
            currentTopScore = score;
        }
        UpdateScoreText();
    }

    public void UpdateScoreText()
    {
        MPTPlayer.Instance.UpdateBestScore(score);
        UpdateBestScoreText();
        scoreText.SetScore(score);
        scoreFinalText.text = "" + score;
}

    public void UpdateBestScoreText()
    {
        bestScoreText.text = "Best : " + MPTPlayer.Instance.GetBestScore();
        bestScoreFinalText.text = "" + MPTPlayer.Instance.GetBestScore();
}

    public void UpdateTrashText()
    {
        trashPriceText.text = "Trash : " + trashPrice;
    }

    public void IncreaseTrashPrice()
    {
        trashPrice *= 1; //Test crado pour laisser toujours le cost a 1
        UpdateTrashText();
    }

    public void DecreaseTrashPrice()
    {
        if (trashPrice > 1)
        {
            --trashPrice;
            UpdateTrashText();
        }
    }

    public void TrashShape()
    {
        score -= trashPrice;
        if (score < 0)
        {
            Loose();
        }
        else
        {
            IncreaseTrashPrice();
            UpdateScoreText();
            MPTShapeManager.Instance.UnregisterShape(MPTSpawner.Instance.currentShape);
            Destroy(MPTSpawner.Instance.currentShape.gameObject);
            MPTSpawner.Instance.SpawnNew();
        }
    }

    public void Restart()
    {
        MPTPlayer.Instance.NewGameStarted();
        score = 0;
        trashPrice = 1;
        UpdateScoreText();
        MPTSpawner.Instance.ResetSquaresDone();
        MPTGrid.Instance.ResetGrid();
        foreach(MPTShape shape in MPTShapeManager.Instance.listOfShapes)
        {
            Destroy(shape.gameObject);
        }
        MPTShapeManager.Instance.listOfShapes.Clear();
        MPTSpawner.Instance.Clear();
        MPTSpawner.Instance.SpawnNew();
        MPTSpawner.Instance.SpawnNew();
        MPTSpawner.Instance.SpawnNew();
        looseScreen.SetActive(false);
        Resume();
    }

    public void ToggleDebugMode()
    {
        if (debugUI.activeInHierarchy)
        {
            debugUI.SetActive(false);
            for (int i = 0; i < debugUI.transform.childCount; ++i)
            {
                MPTWeightButton weightButton = debugUI.transform.GetChild(i).GetComponent<MPTWeightButton>();
                MPTShape shape = MPTSpawner.Instance.spawnables[i].GetComponent<MPTShape>();
                shape.weight = weightButton.GetWeight();
                Destroy(debugUI.transform.GetChild(i).gameObject);
            }
        }
        else
        {
            debugUI.SetActive(true);
            for (int i = 0; i < MPTSpawner.Instance.spawnables.Count; ++i)
            {
                GameObject go = MPTSpawner.Instance.spawnables[i];
                MPTShape shape = go.GetComponent<MPTShape>();
                GameObject debugWeight = GameObject.Instantiate(debugWeightModel);
                MPTWeightButton weightButton = debugWeight.GetComponent<MPTWeightButton>();
                weightButton.SetWeight(shape.weight);
                weightButton.SetName(go.name);
                debugWeight.transform.SetParent(debugUI.transform, false);
                RectTransform tr = (RectTransform)debugWeight.transform;
                float height = tr.offsetMax.y - tr.offsetMin.y;
                (debugWeight.transform as RectTransform).offsetMin = new Vector2(0.0f, -height * i);
                (debugWeight.transform as RectTransform).offsetMax = new Vector2(0.0f, -height * (i + 1));
            }
        }
    }

    public void ShowConfirmRestart()
    {
		if (isPaused || MPTInteractiveTutoManager.Instance.GetCanRestart() == false)
		{
			return;
		}
        isPaused = true;
        confirmRestartUI.SetActive(true);
    }

    public void ConfirmRestart()
    {
        confirmRestartUI.SetActive(false);
        Loose();
    }

    public void ShowTuto()
    {
		MPTPlayer.Instance.SetTutoDone(false);

		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		/*
		isPaused = true;
        tutoUI.SetActive(true);
        infoUI.SetActive(false);
		*/
    }

    public void ShowInfo()
    {
        isPaused = true;
        infoUI.SetActive(true);
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume()
    {
        isPaused = false;
        confirmRestartUI.SetActive(false);
        tutoUI.SetActive(false);
        infoUI.SetActive(false);
    }

    public void SendEmail()
    {
        string email = "thestonegoatgames@gmail.com";
        string subject = MyEscapeURL("In game contact");
        string body = MyEscapeURL("");
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }

	public void UpdateAlmostEnd(bool active)
	{
		endCloseWarning.SetActive(active);
	}

	public void LowerDropsLeft()
	{
		--dropsLeft;

        if (MPTInteractiveTutoManager.Instance.currentTuto == null)
		{
			dropsLeftText.text = "" + dropsLeft + " drops left";
		}
		if (dropsLeft <= 0)
		{
			dropsLeftText.text = "";
			Loose();
		}
	}

	public void ResetDropsLeft()
	{
		dropsLeftText.text = "";
		dropsLeft = maxDropsLeft;
	}

    string MyEscapeURL (string url)
    {
        return WWW.EscapeURL(url).Replace("+", "%20");
    }
    
    void OnDestroy()
    {
#if !UNITY_EDITOR
        Analytics.CustomEvent("GameOver", new Dictionary<string, object>
        {
            { "TopScore", MPTPlayer.Instance.GetBestScore() },
            { "GamesPlayed", MPTPlayer.Instance.GetGamesStarted() }
        });
#endif
    }
}
