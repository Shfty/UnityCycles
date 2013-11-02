using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
	// Fields
	int initScore;

	// Properties
	public int Score;

	// Unity Methods
	void Awake()
	{
		initScore = Score;
	}

	public void OnEnable()
	{
		Score = initScore;
	}
}
