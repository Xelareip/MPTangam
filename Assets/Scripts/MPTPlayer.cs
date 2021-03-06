﻿using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MPTPlayer
{
    private static MPTPlayer _instance;
    public static MPTPlayer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MPTPlayer();
            }
            return _instance;
        }
    }

    private int _bestScore = 0;
    private int _gamesPlayed = 0;
    private bool _tutoDone = false;

    private MPTPlayer()
    {
        Load();
    }

    public int GetBestScore()
    {
        return _bestScore;
    }

    public void UpdateBestScore(int score)
    {
        if (score > _bestScore)
        {
            _bestScore = score;
            Save();
        }
    }

    public void NewGameStarted()
    {
        ++_gamesPlayed;
    }

    public int GetGamesStarted()
    {
        return _gamesPlayed;
    }

    public bool GetTutoDone()
    {
        return _tutoDone;
    }

    public void SetTutoDone(bool isDone)
    {
        _tutoDone = isDone;
        Save();
    }

    private void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/Save.dat") == false)
        {
            return;
        }
        StreamReader reader = new StreamReader(Application.persistentDataPath + "/Save.dat");
        string saveString = reader.ReadToEnd();
        reader.Close();
        saveString = saveString.Replace("\r", "");
        string[] saveLines = saveString.Split('\n');

        foreach (string line in saveLines)
        {
            string[] lineSplit = line.Split(':');
            switch(lineSplit[0])
            {
                case "bestScore":
                    _bestScore = int.Parse(lineSplit[1]);
                    break;
                case "gamesPlayed":
                    _gamesPlayed = int.Parse(lineSplit[1]);
                    break;
                case "tutoDone":
                    _tutoDone = bool.Parse(lineSplit[1]);
                    break;
                default:
                    break;
            }
        }
        
    }
    
    public void Save()
    {
        string saveString = "";

        saveString += "bestScore:" + _bestScore + "\n";
        saveString += "gamesPlayed:" + _gamesPlayed + "\n";
        saveString += "tutoDone:" + _tutoDone + "\n";

        Directory.GetParent(Application.persistentDataPath).Create();
        StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/Save.dat");
        writer.Write(saveString);
        writer.Close();
    }
}
