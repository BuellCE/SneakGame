using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    private static GameController controllerInstance;

    public int startingDifficulty = 10;
    public int difficultyIncreaseEachLevel = 1;
    public float mapBrightness = 0.05f;

    private bool playing = false;
    private TerrainGenerator generator;
    private CameraMovement cameraMover;
    private PausedMenu pauseMenu;
    private Light sunLight;

    private bool isGameOver;
    private bool canPlayerControl;

    GamePlayMap map;

    private int currentDifficulty;
    private Text loadingText;

    void Awake() {
        DontDestroyOnLoad(this.gameObject);

        if (controllerInstance != null) {
            Destroy(controllerInstance.gameObject);
        }
        controllerInstance = this;

    }

    void Start() {
        currentDifficulty = startingDifficulty;
        loadingText = GameObject.FindGameObjectWithTag("LoadingText").GetComponent<Text>();
        generator = GetComponent<TerrainGenerator>();
    }

    public void StartGame() {
        loadingText.enabled = true;
        StartCoroutine(LoadLevelScene());
    }

    IEnumerator LoadLevelScene() {

        AsyncOperation op = SceneManager.LoadSceneAsync("Levels");
        op.allowSceneActivation = false;
        while (!op.isDone) {
            loadingText.text = "Loading... " + (int) (op.progress * 100) +  "%";
            if (op.progress >= 0.9f) {
                op.allowSceneActivation = true;
            }
            yield return null;
        }
        if (op.isDone && !playing) {
            LoadingComplete();
        }
    }

    private void LoadingComplete() {
        playing = true;
        sunLight = GameObject.FindGameObjectWithTag("SunLight").GetComponent<Light>();
        cameraMover = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMovement>();
        map = GameObject.FindGameObjectWithTag("Map").GetComponent<GamePlayMap>();
        pauseMenu = GameObject.FindGameObjectWithTag("PausedMenu").GetComponent<PausedMenu>();
        LoadNewLevel();
    }

    public void LoadNewLevel() {
        generator.GenerateNewLevelVariables(currentDifficulty);
        generator.DestroySpawnedEnemies();
        generator.DestroySpawnedPlayer();
        generator.GenerateLevel();
        map.GenerateMap(generator.GetBlockMap(), generator.GetEndPosition());
        cameraMover.SetMoveLocation(generator.GetEndLocation() + new Vector3(0, 5, 0));
        sunLight.intensity = 0.3f;
        pauseMenu.SetToPlayingMenu();
        isGameOver = false;
        canPlayerControl = false;
        generator.SpawnInPlayer();
        StartCoroutine(StartGame(10));
    }

    IEnumerator StartGame(float time) {
        yield return new WaitForSeconds(time);
        canPlayerControl = true;
        sunLight.intensity = mapBrightness;
        cameraMover.TargetPlayer();
        generator.SpawnInEnemies();
        generator.AllowEnemyMovement();
    }


    public void RestartLevel() {
        canPlayerControl = false;
        isGameOver = false;
        pauseMenu.SetToPlayingMenu();
        generator.DestroySpawnedPlayer();
        StartCoroutine(SpawnInNewEntities());
    }

    IEnumerator SpawnInNewEntities() {
        yield return new WaitForSeconds(1);
        canPlayerControl = true;
        generator.DestroySpawnedEnemies();
        generator.SpawnInPlayer();
        cameraMover.TargetPlayer();
        generator.SpawnInEnemies();
        generator.AllowEnemyMovement();
    }

    public void ReturnToMenu() {
        sunLight = null;
        cameraMover = null;
        map = null;
        pauseMenu = null;
        playing = false;
        canPlayerControl = false;
        SceneManager.LoadScene("Menu");
    }

    public void StartNextLevel() {
        currentDifficulty += difficultyIncreaseEachLevel;
        cameraMover.gameObject.transform.position = cameraMover.GetStartingPosition();
        LoadNewLevel();
    }

    public void FinishLevel() {
        generator.DisableEnemyMovement();
        pauseMenu.SetToEndOfLevelMenu();
        pauseMenu.SetMenuVisibility(true);
        canPlayerControl = false;
        isGameOver = true;
    }

    public bool IsGameOver() {
        return isGameOver;
    }

    public bool CanPlayerControl() {
        return canPlayerControl;
    }

    public void QuitGame() {
        Application.Quit();
    }

}
