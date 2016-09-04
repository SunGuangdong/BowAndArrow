using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 作为主要脚本， 附加在 弓 上
/// </summary>
public class BowAndArrow : MonoBehaviour {

    // 若要确定鼠标位置，我们需要 raycast
    private Ray mouseRay1;
	private RaycastHit rayHit;

    // raycast 在屏幕中的位置
    private float posX;
	private float posY;

	// 引用的 对象/预制体
	public GameObject bowString;
	GameObject arrow;
	public GameObject arrowPrefab;
	public GameObject gameManager;	
	public GameObject risingText;
	public GameObject target;

	// 音效资源
	public AudioClip stringPull;
	public AudioClip stringRelease;
	public AudioClip arrowSwoosh;

	// has sound already be played
	bool stringPullSoundPlayed;
	bool stringReleaseSoundPlayed;
	bool arrowSwooshSoundPlayed;

	// the bowstring is a line renderer
	private List<Vector3> bowStringPosition;
	LineRenderer bowStringLinerenderer;

	// to determine the string pullout
	float arrowStartX;
	float length;

	// some status vars
	bool arrowShot;
	bool arrowPrepared;

	// position of the line renderers middle part 
	Vector3 stringPullout;
	Vector3 stringRestPosition = new Vector3 (-0.44f, -0.06f, 2f);

	// 游戏状态（每个状态对应一个界面）
	public enum GameStates
    {
		menu, 
		instructions,
		game,
		over,
		hiscore,
	};

	// 默认界面
	public GameStates gameState = GameStates.menu;

	// UI 界面
	public Canvas menuCanvas;
	public Canvas instructionsCanvas;
	public Canvas highscoreCanvas;
	public Canvas gameCanvas;
	public Canvas gameOverCanvas;

	// 文本组件
	public Text arrowText;
	public Text scoreText;
	public Text endscoreText;
	public Text actualHighscoreText;
	public Text newHighscoreText;
	public Text newHighText;

	// 设置每局总共有多少只箭
	public int arrows = 20;
	// actual score
	public int score = 0;



    /// <summary>
    /// 重新开始游戏的处理
    /// </summary>
	void resetGame() {
		arrows = 20;
		score = 0;
		// be sure that there is only one arrow in the game
		if (GameObject.Find("arrow") == null)
			createArrow (true);
	}



	void Start () {
		// set the UI screens
		menuCanvas.enabled = true;
		instructionsCanvas.enabled = false;
		highscoreCanvas.enabled = false;
		gameCanvas.enabled = false;
		gameOverCanvas.enabled = false;

		// 创建 PlayerPref 本地存储
		initScore ();

		// 创建箭 并设置目标
		createArrow (true);

		// 箭弦的显示
		bowStringLinerenderer = bowString.AddComponent<LineRenderer>();
		bowStringLinerenderer.SetVertexCount(3);
		bowStringLinerenderer.SetWidth(0.05F, 0.05F);
		bowStringLinerenderer.useWorldSpace = false;
		bowStringLinerenderer.material = Resources.Load ("Materials/bowStringMaterial") as Material;

		bowStringPosition = new List<Vector3> ();
		bowStringPosition.Add(new Vector3 (-0.44f, 1.43f, 2f));
		bowStringPosition.Add(new Vector3 (-0.44f, -0.06f, 2f));
		bowStringPosition.Add(new Vector3 (-0.43f, -1.32f, 2f));

		bowStringLinerenderer.SetPosition (0, bowStringPosition [0]);
		bowStringLinerenderer.SetPosition (1, bowStringPosition [1]);
		bowStringLinerenderer.SetPosition (2, bowStringPosition [2]);
		arrowStartX = 0.7f;

		stringPullout = stringRestPosition;
	}



	// 根据状态显示界面
	void Update ()
    {
		switch (gameState)
        {
		case GameStates.menu:
			// 返回键 退出程序 (android)
			if (Input.GetKeyDown(KeyCode.Escape))
            {
				Application.Quit();
			}
			break;

		case GameStates.game:
			// 
			showArrows();
			showScore();

			// 返回键 返回到主菜单 (android)
			if (Input.GetKeyDown(KeyCode.Escape))
            {
				showMenu();
			}

            // 通过鼠标操纵游戏
            if (Input.GetMouseButton(0))
            {
				// the player pulls the string
				if (!stringPullSoundPlayed)
                    {
					// play sound
					GetComponent<AudioSource>().PlayOneShot(stringPull);
					stringPullSoundPlayed = true;
				}
				// detrmine the pullout and set up the arrow
				prepareArrow();
			}

			// 鼠标左键  或者 点击释放
			if (Input.GetMouseButtonUp (0) && arrowPrepared) {
				// play string sound
				if (!stringReleaseSoundPlayed) {
					GetComponent<AudioSource>().PlayOneShot(stringRelease);
					stringReleaseSoundPlayed = true;
				}
				// play arrow sound
				if (!arrowSwooshSoundPlayed) {
					GetComponent<AudioSource>().PlayOneShot(arrowSwoosh);
					arrowSwooshSoundPlayed = true;
				}
				// shot the arrow (rigid body physics)
				shootArrow();
			}
			// in any case: update the bowstring line renderer
			drawBowString();
			break;
		case GameStates.instructions:
			break;
		case GameStates.over:
			break;
		case GameStates.hiscore:
			break;
		}
	}


	//  初始化分数
	public void initScore()
    {
		if (!PlayerPrefs.HasKey ("Score"))
			PlayerPrefs.SetInt ("Score", 0);
	}


	public void showScore()
    {
		scoreText.text = "Score: " + score.ToString();
	}


	public void showArrows()
    {
		arrowText.text = "Arrows: " + arrows.ToString ();
	}


	//  创建一把箭
	public void createArrow(bool hitTarget)
    {
		Camera.main.GetComponent<CamMovement> ().resetCamera ();
		// when a new arrow is created means that:
		// sounds has been played
		stringPullSoundPlayed = false;
		stringReleaseSoundPlayed = false;
		arrowSwooshSoundPlayed = false;
		// does the player has an arrow left ?
		if (arrows > 0)
        {
			// may target's position be altered?
			if (hitTarget)
            {
				// if the player hit the target with the last arrow, 
				// it's set to a new random position
				float x = Random.Range(-1f,8f);
				float y = Random.Range(-3f,3f);
				Vector3 position = target.transform.position;
				position.x = x;
				position.y = y;
				target.transform.position = position;
			}

			// 初始化新的 箭
			this.transform.localRotation = Quaternion.identity;
			arrow = Instantiate (arrowPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			arrow.name = "arrow";
			arrow.transform.localScale = this.transform.localScale;
			arrow.transform.localPosition = this.transform.position + new Vector3 (0.7f, 0, 0);
			arrow.transform.localRotation = this.transform.localRotation;
			arrow.transform.parent = this.transform;
			// transmit a reference to the arrow script
			arrow.GetComponent<RotateArrow> ().setBow (gameObject);
			arrowShot = false;
			arrowPrepared = false;


			// 
			arrows --;
		}
		else    // 没有箭了  Game Over
        {
			gameState = GameStates.over;
			gameOverCanvas.enabled = true;
			endscoreText.text = "You shot all the arrows and scored " + score + " points.";
		}
	}


	//  放箭
	public void shootArrow()
    {
        // 这时候 要为 箭添加 刚体
		if (arrow.GetComponent<Rigidbody>() == null)
        {
			arrowShot = true;
			arrow.AddComponent<Rigidbody>();
			arrow.transform.parent = gameManager.transform;
			arrow.GetComponent<Rigidbody>().AddForce (Quaternion.Euler (new Vector3(transform.rotation.eulerAngles.x,transform.rotation.eulerAngles.y,transform.rotation.eulerAngles.z))*new Vector3(25f*length,0,0), ForceMode.VelocityChange);
		}
		arrowPrepared = false;
		stringPullout = stringRestPosition;

		// Cam
		Camera.main.GetComponent<CamMovement> ().resetCamera ();
		Camera.main.GetComponent<CamMovement> ().setArrow (arrow);

	}


	// 预备  箭
	public void prepareArrow()
    {
		// get the touch point on the screen
		mouseRay1 = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(mouseRay1, out rayHit, 1000f) && arrowShot == false)
		{
			// determine the position on the screen
			posX = this.rayHit.point.x;
			posY = this.rayHit.point.y;
			// set the bows angle to the arrow
			Vector2 mousePos = new Vector2(transform.position.x-posX,
                transform.position.y-posY);
			float angleZ = Mathf.Atan2(mousePos.y,mousePos.x)*Mathf.Rad2Deg;
			transform.eulerAngles = new Vector3(0,0,angleZ);
			// determine the arrow pullout
			length = mousePos.magnitude / 3f;
			length = Mathf.Clamp(length,0,1);
			// set the bowstrings line renderer
			stringPullout = new Vector3(-(0.44f+length), -0.06f, 2f);
			// set the arrows position
			Vector3 arrowPosition = arrow.transform.localPosition;
			arrowPosition.x = (arrowStartX - length);
			arrow.transform.localPosition = arrowPosition;
		}
		arrowPrepared = true;
	}



	// 设置弓弦
	public void drawBowString()
    {
		bowStringLinerenderer = bowString.GetComponent<LineRenderer>();
		bowStringLinerenderer.SetPosition (0, bowStringPosition [0]);
		bowStringLinerenderer.SetPosition (1, stringPullout);
		bowStringLinerenderer.SetPosition (2, bowStringPosition [2]);
	}
	

	// 设置分数
	public void setPoints(int points)
    {
		score += points;
		if (points == 50)
        {
			arrows++;
			GameObject rt1 = (GameObject)Instantiate(risingText, 
                new Vector3(0,0,0),Quaternion.identity);
			rt1.transform.position = this.transform.position + new Vector3(0,0,0);
			rt1.transform.name = "rt1";
			// each target's "ring" is 0.07f wide
			// so it's relatively simple to calculate the ring hit (thus the score)
			rt1.GetComponent<TextMesh>().text= "Bonus arrow";
		}
	}

    // 显示   说明介绍
    public void showInstructions()
    {
		menuCanvas.enabled = false;
		instructionsCanvas.enabled = true;
	}


	// 隐藏 说明介绍
	public void hideInstructions()
    {
		menuCanvas.enabled = true;
		instructionsCanvas.enabled = false;
	}


	// 显示最高分
	public void showHighscore()
    {
		menuCanvas.enabled = false;
		highscoreCanvas.enabled = true;
		actualHighscoreText.text = "Actual Hiscore: " + PlayerPrefs.GetInt ("Score") + " points";
		newHighscoreText.text = "Your Score: " + score + " points";
		if (score > PlayerPrefs.GetInt("Score"))
			newHighText.enabled = true;
		else
			newHighText.enabled = false;
	}


	// 隐藏 最高分
	public void hideHighScore()
    {
		menuCanvas.enabled = true;
		highscoreCanvas.enabled = false;
		if (score > PlayerPrefs.GetInt ("Score"))
        {
			PlayerPrefs.SetInt("Score",score);
		}
		resetGame();
	}


	// 检查最高分  如果是新的最高分就显示   否则  重新开始
	public void checkHighScore()
    {
		gameOverCanvas.enabled = false;
		if (score > PlayerPrefs.GetInt ("Score"))
        {
			showHighscore();
		}
		else {
			menuCanvas.enabled = true;
			resetGame();
		}
	}

	// 
	public void startGame()
    {
		menuCanvas.enabled = false;
		highscoreCanvas.enabled = false;
		instructionsCanvas.enabled = false;
		gameCanvas.enabled = true;
		gameState = GameStates.game;
	}

	public void showMenu()
    {
		menuCanvas.enabled = true;
		gameState = GameStates.menu;
		resetGame ();
	}
}
