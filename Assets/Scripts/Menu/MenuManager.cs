using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using W02;

public class MenuManager : MonoBehaviour
{
	public GameObject MainMenu;
	public GameObject PauseMenu;
	public GameObject Player;
	public GameObject EventSystem;
	public GameObject UIStartButton;
	public GameObject UIRestartButton;
	public Vector3 restartPosition;

	public Transform respawnPoint;

    private void Start()
    {
		SetMainMenu();
	}
    private void Update()
	{
		if (InputManager.Instance.ExitButton)
		{
			//if (!PauseMenu.active)
			//	SetPauseMenu();
			//else
			//	StartButton();
			Player.transform.position = respawnPoint.position;
		}
	}

	private void SetMainMenu()
    {
		//Time.timeScale = 0f;
		EventSystem.GetComponent<EventSystem>().firstSelectedGameObject = UIStartButton;
		MainMenu.SetActive(true);
		PauseMenu.SetActive(false);
		//Player.SetActive(false);
	}

	private void SetPauseMenu()
    {
		//Time.timeScale = 0f;
		EventSystem.GetComponent<EventSystem>().firstSelectedGameObject = UIRestartButton;
		MainMenu.SetActive(false);
		PauseMenu.SetActive(true);
		//Player.SetActive(false);
	}

	public void StartButton()
	{
		//Time.timeScale = 1f;
		MainMenu.SetActive(false);
		PauseMenu.SetActive(false);
		//Player.SetActive(true);
	}
	public void RestartButton()
    {
		//Time.timeScale = 1f;
		Player.transform.position = restartPosition;
		MainMenu.SetActive(false);
		PauseMenu.SetActive(false);
		//Player.SetActive(true);
	}
	public void ExitButton()
    {
		Application.Quit();
    }

}