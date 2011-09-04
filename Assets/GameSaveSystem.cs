using UnityEngine;
using System;
using System.Collections;

public static class GameSaveSystem {
	
	//Event fired on Save
	//
	public delegate void SaveHandler();
	public static event SaveHandler OnGameSave;
	
	//List that holds all the save data till we save to disk
	public static ArrayList LevelList=new ArrayList();
	
	public static void NewGame()
	{
		LevelList.Clear();
		PlayerPrefs.DeleteAll();
	}
	
	public static void Continue()
	{
		Application.LoadLevel(PlayerPrefs.GetInt("LastSavedLevel"));
	}
	
	//This method should be called when you want to save the object. For example, OnDestroy()
	public static void SaveObjectState(string Name,SaveObject.States State)
	{
		//When we add and object, fist we try to find it on the current list to avoid duplicity errors
		bool Finded = false;
		foreach (SaveObject savedObject in LevelList)
		{
			if(savedObject.Name == Name)
			{
				Finded = true;
				savedObject.State = State;
				break;
			}
		}
		if(!Finded)
		{
			//No finded on the list, so add it
			int Level = Application.loadedLevel;
			SaveObject newSaveObject = new SaveObject(Name,State,Level);
			
			LevelList.Add(newSaveObject);
		}
	}
	
	/*			
	 Load Level should be called OnLevelLoaded or in object scene start event.
	 It would load from the disk and from the current memory list and Destroy all the finded objects market as Destroyed
	 or call the SavedAsActive method on the object. Your object should be ready to this call.
	 */
	
	public static void LoadLevel()
	{
		SaveObject[] FromDisk = LoadLevelFromDisk(Application.loadedLevel);
		ArrayList FromDiskList=new ArrayList();
		bool alreadyLoaded = false;
		if(LevelList.Count > 0)
		{
			for(int i=0;i< FromDisk.Length;i++)
			{
				alreadyLoaded = false;
				foreach (SaveObject savedObject in LevelList)
				{
					if(FromDisk[i].Name == savedObject.Name)
					{
						alreadyLoaded = true;
					}
				}
				if(!alreadyLoaded)
				{
					FromDiskList.Add(FromDisk[i]);
				}
					
			}
			foreach (SaveObject savedObject in FromDiskList)
			{
				LevelList.Add(savedObject);
			}
				
		}
		else
		{
			for(int i=0;i< FromDisk.Length;i++)
			{
				FromDiskList.Add(FromDisk[i]);
			}
			LevelList = FromDiskList;
		}
			
		foreach (SaveObject savedObject in LevelList)
		{
			if(savedObject.Level == Application.loadedLevel)
			{
				GameObject LoadedGO = GameObject.Find(savedObject.Name);
				if(LoadedGO)
				{
					if(savedObject.State == SaveObject.States.Destroyed)
					{
						GameObject.Destroy(LoadedGO);
					}
					else if(savedObject.State == SaveObject.States.Active)
					{
						LoadedGO.SendMessage("SavedAsActive");
					}
				}
			}
		}
	}
	
	// Call this method when you want to save the memory list to the disk
	public static bool SaveLevelToDisk(int Level)
    {
		if(OnGameSave != null)
		{
			OnGameSave();
		}
		
        if (LevelList.Count == 0) return false;
		
		ArrayList tempList=new ArrayList();
		foreach (SaveObject savedObject in LevelList)
		{
			if(savedObject.Level == Level)
			{
				tempList.Add(savedObject);
			}
		}
			
		
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for(int i=0;i<tempList.Count - 1;i++)
		{
			SaveObject savedObject = (SaveObject)tempList[i];
			for(int j=0; j < 3; j++)
			{
				if(j==0)
				{
					sb.Append(savedObject.Name).Append("|");
				}
				else if(j==1)
				{
					sb.Append(savedObject.State.ToString()).Append("|");
				}
				else if(j==2)
				{
					sb.Append(savedObject.Level).Append("|");
				}
					
			}
            
		}
		if(tempList.Count > 0)
		{
			SaveObject currentSave = (SaveObject)tempList[tempList.Count - 1];
			
			for(int j=0; j < 3; j++)
			{
				if(j==0)
				{
					sb.Append(currentSave.Name).Append("|");
				}
				else if(j==1)
				{
					sb.Append(currentSave.State.ToString()).Append("|");
				}
				else if(j==2)
				{
					sb.Append(currentSave.Level);
				}
					
			}
			
		}

        try
        {
            PlayerPrefs.SetString(Level.ToString(), sb.ToString());
        }
        catch (Exception e)
        {
            return false;
        }
		int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
		PlayerPrefs.SetInt("SaveTime",unixTime);
        return true;
    }
	
	// Call this method to save to disk the Level to continue on Load
	// Not included on SaveToDisk so you could call it anytime
	
	public static void SetLastLoadedLevel()
	{
		PlayerPrefs.SetInt("LastSavedLevel",Application.loadedLevel);
	}
	
	// This method is used on LoadLevel
	// Used to read all the data stored on PlayerPrefabs and return SaveObjects
	public static SaveObject[] LoadLevelFromDisk(int Level)
    {
        if (PlayerPrefs.HasKey(Level.ToString()))
        {
            string[] stringArray = PlayerPrefs.GetString(Level.ToString()).Split("|"[0]);
            SaveObject[] SaveObjectArray = new SaveObject[stringArray.Length/3];
			if(stringArray.Length/3 > 0)
			{
				int objectIndex = 0;
				int SavedObjectArrayIndex = -1;
            	for (int i = 0; i < stringArray.Length; i++)
				{
					if(objectIndex == 0)
					{
						SavedObjectArrayIndex++;
						SaveObjectArray[SavedObjectArrayIndex] = new SaveObject("0",SaveObject.States.Active,0);
						SaveObjectArray[SavedObjectArrayIndex].Name = stringArray[i];
						objectIndex = 1;
					}
					else if(objectIndex == 1)
					{
						if(stringArray[i] == "Active")
						{
							SaveObjectArray[SavedObjectArrayIndex].State = SaveObject.States.Active;
						}
						else if(stringArray[i] == "Destroyed")
						{
							SaveObjectArray[SavedObjectArrayIndex].State = SaveObject.States.Destroyed;
						}
						objectIndex = 2;
					}
					else if(objectIndex == 2)
					{
						SaveObjectArray[SavedObjectArrayIndex].Level = Convert.ToInt32(stringArray[i]);
						objectIndex = 0;
					}
				}
        	    return SaveObjectArray;
			}
        }
        return new SaveObject[0];
    }
	
	// Clear memory list
	public static void ClearSaved()
	{
		LevelList.Clear();
	}
	
	
	
}

// SaveObject class represent each object state with a unique name, an state and the Level it was saved
public class SaveObject {

	public string Name;
	public States State;
	public enum States{
		Active,
		Destroyed
	}
	public int Level;
	
	public SaveObject(string Name,States State,int Level)
	{
		this.Name = Name;
		this.State = State;
		this.Level = Level;
	}
}

