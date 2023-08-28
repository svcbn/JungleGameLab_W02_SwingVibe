using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using W02;

public class MenuManager : MonoBehaviour
{
	public GameObject MainMenu;
	public GameObject PauseMenu;
	public GameObject Player;

    private void Start()
    {
		SetMainMenu();
	}
    private void Update()
	{
		if (InputManager.Instance.ExitButton)
		{
			if (!PauseMenu.active)
				SetPauseMenu();
			else
				StartButton();
		}
	}

	private void SetMainMenu()
    {
		//Time.timeScale = 0f;
		MainMenu.SetActive(true);
		PauseMenu.SetActive(false);
		//Player.SetActive(false);
	}

	private void SetPauseMenu()
    {
		//Time.timeScale = 0f;
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
		MainMenu.SetActive(false);
		PauseMenu.SetActive(false);
		//Player.SetActive(true);
	}
	public void ExitButton()
    {
		Application.Quit();
    }

}