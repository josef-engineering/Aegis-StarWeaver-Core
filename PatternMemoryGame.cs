using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.UI;

using TMPro;


[System.Serializable]

public class PlayerLoadout

{

public string nodeName;

public Transform nodeSocket;

[Header("Primary Settings")]

public GameObject shieldPrefab;

public Vector3 gunOffset;

public Vector3 gunRotation;

public Vector3 gunScale = Vector3.one;

[Header("Secondary Settings")]

public GameObject knifePrefab;

public Vector3 knifeOffset;

public Vector3 knifeRotation;

public Vector3 knifeScale = Vector3.one;

}


public class PatternMemoryGame : MonoBehaviour

{

public static PatternMemoryGame I { get; private set; }


[Header("Audio FX (One Shots)")]

public AudioSource sfxSource; // For Clicks/Lasers

public AudioClip pressSound;

public AudioClip lineWhooshSound;


[Header("Audio Ambience (Loop)")]

public AudioSource ambienceSource; // For the Black Hole Wobble

public AudioClip ambienceClip; // Drag your wobble mp3 here

public float maxAmbienceVolume = 0.5f; // Adjust volume here (0 to 1)


[Header("Raycast & Layers")]

public LayerMask nodeLayer;


[Header("1. Standard Node Loadout")]

public List<PlayerLoadout> nodeLoadouts = new List<PlayerLoadout>();


[Header("2. Custom Shield Loadout")]

public List<PlayerLoadout> nodeLoadouts2 = new List<PlayerLoadout>();


[Header("Node Collection & UI")]

public Transform nodesParent;

public TextMeshProUGUI levelTextDisplay;

[Header("Black Hole HUD")]

public Image blackHoleHealthRing;

public int levelsPerBlackHole = 4;

[Header("UI Panels")]

public GameObject gameOverPanel;

public GameObject mainMenuPanel;

public GameObject gameHUDPanel;


[Header("Boss Defeat Sequence")]

public Material goldMaterial;

public Color bossDefeatedColor = new Color(1f, 0.6f, 0f);

public GameObject bossDefeatedTextPanel;

public float bossSequenceDuration = 3.5f;


[Header("Hint System")]

public Button hintButton;

public GameObject buyHintsPanel;

[Header("Hint Animation")]

public GameObject arrowHeadPrefab;

public float extrusionSpeed = 5.0f;

public Material hintLineMaterial;

public float hintLineWidth = 0.5f;

private Material defaultLineMaterial;

private float defaultLineWidth;

private GameObject currentArrowInstance;


[Header("Line Renderer")]

public LineRenderer patternLine;

public List<Transform> patternNodes = new List<Transform>();


[Header("Materials")]

public Material normalMaterial;

public Material highlightMaterial;


[Header("Playback Timing & AI Director")]

public float highlightDuration = 0.45f;

public float gapDuration = 0.12f;

// --- AI DIRECTOR VARIABLES ---

public float baseHighlightSpeed = 0.45f;

public int streakThreshold = 3;

public float fastWinTimeLimit = 4.0f;

private int consecutiveFastWins = 0;

private float roundStartTime;

// -----------------------------


private string activeLoadoutName = "";

private const string SAVE_KEY_LEVEL = "PMG_SavedActualLevel";

private const string SAVE_KEY_SHAPE = "PMG_CurrentShape";


private int currentLevel = 1;

private int currentPatternLength = 3;

private bool isGameActive = false;

private bool isShowingPattern = false;

private List<int> currentPattern = new List<int>();

private List<int> playerEnteredPattern = new List<int>();

private Coroutine playPatternCoroutine = null;


[Header("Dynamic Instructions")]

public TextMeshProUGUI instructionText;

public string watchText = "OBSERVE SIGNAL";

public string repeatText = "REPLICATE SEQUENCE";


public System.Action OnPatternPlaybackFinished;


private void Awake() { if (I != null && I != this) { Destroy(gameObject); return; } I = this; }


private void Start()

{

LoadProgress();

if (!PlayerPrefs.HasKey("PMG_WeaponsActive"))

{

PlayerPrefs.SetInt("PMG_WeaponsActive", 1);

PlayerPrefs.SetString("EquippedLoadoutName", "custom shields");

PlayerPrefs.Save();

}


if (patternLine != null)

{

patternLine.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);

defaultLineMaterial = patternLine.material;

defaultLineWidth = patternLine.widthMultiplier;

}

if(gameOverPanel != null) gameOverPanel.SetActive(false);

if(buyHintsPanel != null) buyHintsPanel.SetActive(false);

if(gameHUDPanel != null) gameHUDPanel.SetActive(false);

if(bossDefeatedTextPanel != null) bossDefeatedTextPanel.SetActive(false);


if (hintButton != null) hintButton.onClick.AddListener(OnHintButtonPressed);


// --- AMBIENCE SETUP (START IN MENU = ON) ---

if (ambienceSource != null && ambienceClip != null)

{

ambienceSource.clip = ambienceClip;

ambienceSource.loop = true;

ambienceSource.volume = maxAmbienceVolume;

ambienceSource.Play();

}

// -------------------------------------------


UpdateLevelUI();

UpdateHintUI();

}


private void Update()

{

if (!isGameActive) return;

if (isShowingPattern) return;

if (gameOverPanel != null && gameOverPanel.activeSelf) return;

if (buyHintsPanel != null && buyHintsPanel.activeSelf) return;


if (Input.GetMouseButton(0)) { DetectNodeSwipe(); UpdateGhostLine(); }

if (Input.GetMouseButtonUp(0)) { CheckPatternResult(); if (patternLine != null) patternLine.positionCount = 0; }

}


// --- AUDIO HELPER ---

private void PlaySFX(AudioClip clip)

{

if (clip == null || sfxSource == null) return;

sfxSource.pitch = Random.Range(0.95f, 1.05f);

sfxSource.PlayOneShot(clip);

}


// --- HINT SYSTEM ---

public void OnHintButtonPressed()

{

if (isShowingPattern) return;

if (!isGameActive) return;

bool hasUsedFree = PlayerPrefs.GetInt("PMG_UsedFreeHint", 0) == 1;

int paidHints = PlayerPrefs.GetInt("PMG_PaidHints", 0);


if (!hasUsedFree) {

PlayerPrefs.SetInt("PMG_UsedFreeHint", 1); PlayerPrefs.Save();

StartCoroutine(PlayExtrusionHint());

} else if (paidHints > 0) {

PlayerPrefs.SetInt("PMG_PaidHints", paidHints - 1); PlayerPrefs.Save();

StartCoroutine(PlayExtrusionHint());

} else {

Time.timeScale = 0f; isGameActive = false;

if (buyHintsPanel != null) buyHintsPanel.SetActive(true);

}

UpdateHintUI();

}

public void CloseHintShop() { if (buyHintsPanel != null) buyHintsPanel.SetActive(false); Time.timeScale = 1f; isGameActive = true; }
// --- IAP WRAPPER ---
// Link the "Buy Hints" IAP Button to THIS function in the Inspector
public void Buy5HintsSuccess() 
{ 
    AddPurchasedHints(5); 
}
public void AddPurchasedHints(int amount) { int current = PlayerPrefs.GetInt("PMG_PaidHints", 0); PlayerPrefs.SetInt("PMG_PaidHints", current + amount); PlayerPrefs.Save(); UpdateHintUI(); CloseHintShop(); StartCoroutine(PlayExtrusionHint()); }

private void UpdateHintUI() {

if (hintButton == null) return;

bool hasUsedFree = PlayerPrefs.GetInt("PMG_UsedFreeHint", 0) == 1;

int paidHints = PlayerPrefs.GetInt("PMG_PaidHints", 0);

TextMeshProUGUI btnText = hintButton.GetComponentInChildren<TextMeshProUGUI>();

if (btnText != null) {

if (!hasUsedFree) btnText.text = "HINT (FREE)";

else if (paidHints > 0) btnText.text = $"HINT ({paidHints})";

else btnText.text = "HINT (+)";

}

}

private IEnumerator PlayExtrusionHint()

{

isShowingPattern = true; playerEnteredPattern.Clear();

if (patternLine != null) { patternLine.positionCount = 0; if(hintLineMaterial != null) patternLine.material = hintLineMaterial; patternLine.widthMultiplier = hintLineWidth; }

if (arrowHeadPrefab != null) { if (currentArrowInstance != null) Destroy(currentArrowInstance); currentArrowInstance = Instantiate(arrowHeadPrefab); }

if (currentPattern.Count > 0) {

patternLine.positionCount = 1; Vector3 startNodePos = patternNodes[currentPattern[0]].position; startNodePos.z -= 0.5f; patternLine.SetPosition(0, startNodePos);

SetNodeHighlight(currentPattern[0], true);

PlaySFX(pressSound);


if (currentArrowInstance != null) currentArrowInstance.transform.position = startNodePos;

for (int i = 0; i < currentPattern.Count - 1; i++) {

int startIdx = currentPattern[i]; int endIdx = currentPattern[i + 1];

Vector3 p0 = patternNodes[startIdx].position; p0.z -= 0.5f; Vector3 p1 = patternNodes[endIdx].position; p1.z -= 0.5f;

patternLine.positionCount = i + 2;

PlaySFX(lineWhooshSound);

float t = 0;

while (t < 1f) {

t += Time.unscaledDeltaTime * extrusionSpeed;

Vector3 currentTipPos = Vector3.Lerp(p0, p1, t);

patternLine.SetPosition(i + 1, currentTipPos);

if (currentArrowInstance != null) { currentArrowInstance.transform.position = currentTipPos; currentArrowInstance.transform.LookAt(p1); }

yield return null;

}

patternLine.SetPosition(i + 1, p1);

SetNodeHighlight(endIdx, true);

PlaySFX(pressSound);

}

}

yield return new WaitForSecondsRealtime(1.0f);

if (currentArrowInstance != null) Destroy(currentArrowInstance);

if (patternLine != null) patternLine.positionCount = 0;

foreach(int idx in currentPattern) SetNodeHighlight(idx, false);

ResetLineVisuals(); isShowingPattern = false;

}

private void ResetLineVisuals() { if (patternLine != null) { patternLine.material = defaultLineMaterial; patternLine.widthMultiplier = defaultLineWidth; } }

// --- MAIN GAME FLOW ---

public void RetryLevel() 
{ 
    if(gameOverPanel != null) gameOverPanel.SetActive(false); 
    
    // NEW: Turn the HUD (and Hint Button) back ON
    if(gameHUDPanel != null) gameHUDPanel.SetActive(true);
    
    isGameActive = true; 
    Time.timeScale = 1f; 
    playerEnteredPattern.Clear(); 
    StartCoroutine(PlayPatternCoroutine()); 
}
public void ReturnToMainMenu() {

isGameActive = false; Time.timeScale = 1f;

// --- AMBIENCE ON (Back to Menu) ---

if (ambienceSource != null) ambienceSource.Play();

// ----------------------------------


if (gameOverPanel != null) gameOverPanel.SetActive(false);

if (buyHintsPanel != null) buyHintsPanel.SetActive(false);

if (gameHUDPanel != null) gameHUDPanel.SetActive(false);

if (bossDefeatedTextPanel != null) bossDefeatedTextPanel.SetActive(false);

ResetLineVisuals();

if (currentArrowInstance != null) Destroy(currentArrowInstance);

if (mainMenuPanel != null) {

mainMenuPanel.SetActive(true);

Transform mainContent = mainMenuPanel.transform.Find("MainContent");

if (mainContent != null) mainContent.gameObject.SetActive(true);

MainMenuController menuScript = mainMenuPanel.GetComponent<MainMenuController>();

if (menuScript != null) menuScript.ResetMenu();

}

currentLevel = 1; UpdateLevelUI(); UpdateHintUI(); playerEnteredPattern.Clear(); currentPattern.Clear(); StopAllCoroutines();

if (patternLine != null) patternLine.positionCount = 0;

foreach (Transform node in patternNodes) foreach (Transform child in node) Destroy(child.gameObject);

}


public void StartGame()

{

isGameActive = true; Time.timeScale = 1f;

// --- AMBIENCE OFF (Focus Mode) ---

if (ambienceSource != null) ambienceSource.Stop();

// ---------------------------------


if (gameHUDPanel != null) gameHUDPanel.SetActive(true);

if (bossDefeatedTextPanel != null) bossDefeatedTextPanel.SetActive(false);

if(blackHoleHealthRing != null) blackHoleHealthRing.color = Color.white;

ResetLineVisuals();


bool weaponsActive = PlayerPrefs.GetInt("PMG_WeaponsActive", 0) == 1;

string savedLoadoutName = PlayerPrefs.GetString("EquippedLoadoutName", "custom shields");


if (weaponsActive) SpawnManualLoadouts(savedLoadoutName);

else RefreshNodeCollections();


playerEnteredPattern.Clear();

GeneratePattern(currentPatternLength);

if (playPatternCoroutine != null) StopCoroutine(playPatternCoroutine);

playPatternCoroutine = StartCoroutine(PlayPatternCoroutine());

UpdateHintUI();

}


public void ApplyShape(GameObject shapePrefab) { if (shapePrefab == null) return; PlayerPrefs.SetString(SAVE_KEY_SHAPE, shapePrefab.name); PlayerPrefs.Save(); foreach (var node in patternNodes) { foreach (Transform child in node) Destroy(child.gameObject); GameObject instance = Instantiate(shapePrefab, node); instance.transform.localPosition = Vector3.zero; } }

public void ProcessPlayerSwipe(List<int> swipedPattern) { playerEnteredPattern = new List<int>(swipedPattern); CheckPatternResult(); }

public void RefreshNodeCollections() { patternNodes.Clear(); List<PlayerLoadout> targetList = (activeLoadoutName == "custom shields2") ? nodeLoadouts2 : nodeLoadouts; foreach (var loadout in targetList) if (loadout.nodeSocket != null) patternNodes.Add(loadout.nodeSocket); }

public void SpawnManualLoadouts(string loadoutName) { activeLoadoutName = loadoutName; RefreshNodeCollections(); List<PlayerLoadout> list = (loadoutName == "custom shields2") ? nodeLoadouts2 : nodeLoadouts; foreach (var loadout in list) SpawnSingleLoadout(loadout); }

private void SpawnSingleLoadout(PlayerLoadout loadout) { if (loadout.nodeSocket == null) return; foreach (Transform child in loadout.nodeSocket) Destroy(child.gameObject); if (loadout.shieldPrefab != null) { GameObject shield = Instantiate(loadout.shieldPrefab, loadout.nodeSocket); shield.transform.localPosition = loadout.gunOffset; shield.transform.localEulerAngles = loadout.gunRotation; shield.transform.localScale = loadout.gunScale; } if (loadout.knifePrefab != null) { GameObject knife = Instantiate(loadout.knifePrefab, loadout.nodeSocket); knife.transform.localPosition = loadout.knifeOffset; knife.transform.localEulerAngles = loadout.knifeRotation; knife.transform.localScale = loadout.knifeScale; } }

private void DetectNodeSwipe() {

Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

if (Physics.Raycast(ray, out RaycastHit hit, 500f, nodeLayer)) {

int index = patternNodes.IndexOf(hit.transform);

if (index == -1 && hit.transform.parent != null) index = patternNodes.IndexOf(hit.transform.parent);

if (index != -1) {

if (playerEnteredPattern.Count == 0 || playerEnteredPattern[playerEnteredPattern.Count - 1] != index) {

PlaySFX(pressSound);

if (playerEnteredPattern.Count > 0) PlaySFX(lineWhooshSound);


// --- VIBRATION ADDED HERE (Tactile Feedback) ---

if (PlayerPrefs.GetInt("VibrationEnabled", 1) == 1) Handheld.Vibrate();

// -----------------------------------------------


playerEnteredPattern.Add(index); SetNodeHighlight(index, true);

if (instructionText != null && instructionText.gameObject.activeSelf) instructionText.gameObject.SetActive(false);

}

}

}

}

private void UpdateGhostLine() { if (patternLine == null || patternNodes.Count == 0 || playerEnteredPattern.Count == 0) return; float zOffset = -0.5f; patternLine.positionCount = playerEnteredPattern.Count + 1; for (int i = 0; i < playerEnteredPattern.Count; i++) { Vector3 nodePos = patternNodes[playerEnteredPattern[i]].position; nodePos.z += zOffset; patternLine.SetPosition(i, nodePos); } Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); Plane plane = new Plane(Vector3.forward, patternNodes[0].position); if (plane.Raycast(ray, out float enter)) { Vector3 mousePoint = ray.GetPoint(enter); mousePoint.z += zOffset; patternLine.SetPosition(playerEnteredPattern.Count, mousePoint); } }

private IEnumerator PlayPatternCoroutine() {

isShowingPattern = true;

if (currentLevel == 1 && instructionText != null) { instructionText.gameObject.SetActive(true); instructionText.text = watchText; instructionText.alpha = 1f; }

else if (instructionText != null) { instructionText.gameObject.SetActive(false); }


if (patternLine != null) patternLine.positionCount = 0;

for (int i = 0; i < currentPattern.Count; i++) {

int idx = currentPattern[i];

PlaySFX(pressSound);

if (i > 0) PlaySFX(lineWhooshSound);


SetNodeHighlight(idx, true);

if (patternLine != null) { patternLine.positionCount = i + 1; Vector3 pos = patternNodes[idx].position; pos.z -= 0.5f; patternLine.SetPosition(i, pos); }

yield return new WaitForSecondsRealtime(highlightDuration);

SetNodeHighlight(idx, false);

yield return new WaitForSecondsRealtime(gapDuration);

}

yield return new WaitForSecondsRealtime(0.5f);

if (patternLine != null) patternLine.positionCount = 0;

isShowingPattern = false;

if (currentLevel == 1 && instructionText != null) instructionText.text = repeatText;

// --- AI DIRECTOR: START TIMER ---

roundStartTime = Time.time;

// --------------------------------

OnPatternPlaybackFinished?.Invoke();

}

private void CheckPatternResult() {

if (playerEnteredPattern.Count == 0) return;

bool isCorrect = (playerEnteredPattern.Count == currentPattern.Count);

if (isCorrect) { for (int i = 0; i < currentPattern.Count; i++) if (playerEnteredPattern[i] != currentPattern[i]) { isCorrect = false; break; } }

if (isCorrect) {

// --- VIBRATION ADDED HERE (Success Buzz) ---

if (PlayerPrefs.GetInt("VibrationEnabled", 1) == 1) Handheld.Vibrate();

// -------------------------------------------


// --- AI DIRECTOR CHECK ---

CheckAdaptiveDifficulty();

// -------------------------


bool isBossKill = (currentLevel % levelsPerBlackHole == 0);

if (isBossKill) StartCoroutine(BossDefeatSequence());

else { TriggerWinEffect(); currentLevel++; currentPatternLength = (currentLevel <= 3) ? 3 : (currentLevel <= 6) ? 4 : 5; SaveProgress(); UpdateLevelUI(); Invoke("StartGame", 1.5f); }

}

else {

// Reset speed on failure

highlightDuration = baseHighlightSpeed;

gapDuration = 0.12f;


// --- VIBRATION ADDED HERE (Failure Buzz) ---

if (PlayerPrefs.GetInt("VibrationEnabled", 1) == 1) Handheld.Vibrate();

// -------------------------------------------


TriggerGameOver();

}

foreach (int idx in playerEnteredPattern) SetNodeHighlight(idx, false);

playerEnteredPattern.Clear();

}


private void CheckAdaptiveDifficulty()

{

float timeTaken = Time.time - roundStartTime;

if (timeTaken <= fastWinTimeLimit)

{

consecutiveFastWins++;

if (consecutiveFastWins >= streakThreshold)

{

consecutiveFastWins = 0;

highlightDuration = Mathf.Max(0.2f, highlightDuration * 0.9f);

gapDuration = Mathf.Max(0.05f, gapDuration * 0.9f);

if (instructionText != null) { instructionText.gameObject.SetActive(true); instructionText.text = "SPEED UP!"; Invoke("HideInstruction", 1.5f); }

}

}

else { consecutiveFastWins = 0; }

}

private void HideInstruction() { if(instructionText != null) instructionText.gameObject.SetActive(false); }


private IEnumerator BossDefeatSequence()

{

isGameActive = false;

if (goldMaterial != null) { foreach (Transform node in patternNodes) { Renderer[] rends = node.GetComponentsInChildren<Renderer>(); foreach (Renderer r in rends) r.material = goldMaterial; } }

if (blackHoleHealthRing != null) { blackHoleHealthRing.color = bossDefeatedColor; blackHoleHealthRing.fillAmount = 1f; }

if (bossDefeatedTextPanel != null) bossDefeatedTextPanel.SetActive(true);

yield return new WaitForSecondsRealtime(bossSequenceDuration);

if (bossDefeatedTextPanel != null) bossDefeatedTextPanel.SetActive(false);

currentLevel++; currentPatternLength = (currentLevel <= 3) ? 3 : (currentLevel <= 6) ? 4 : 5; SaveProgress(); UpdateLevelUI();

StartGame();

}


void TriggerGameOver() 
{ 
    isGameActive = false; 
    
    // NEW: Turn off the HUD so the Hint Button disappears
    if(gameHUDPanel != null) gameHUDPanel.SetActive(false); 
    
    if(gameOverPanel != null) gameOverPanel.SetActive(true); 
    else Debug.LogWarning("Game Over Panel is missing!"); 
}
void TriggerWinEffect() { foreach (Transform node in patternNodes) { Renderer[] rends = node.GetComponentsInChildren<Renderer>(); foreach (Renderer r in rends) r.material = highlightMaterial; } }

public void SetNodeHighlight(int index, bool highlight) { if (index < 0 || index >= patternNodes.Count) return; Renderer[] rends = patternNodes[index].GetComponentsInChildren<Renderer>(); foreach (Renderer r in rends) r.material = highlight ? highlightMaterial : normalMaterial; }

public void UpdateLevelUI() { if (levelTextDisplay != null) levelTextDisplay.text = $"Level: {currentLevel}"; if (blackHoleHealthRing != null) { float progress = (float)((currentLevel - 1) % levelsPerBlackHole + 1) / (float)levelsPerBlackHole; blackHoleHealthRing.fillAmount = progress; blackHoleHealthRing.color = Color.white; } }

public void SaveProgress() { PlayerPrefs.SetInt(SAVE_KEY_LEVEL, currentLevel); PlayerPrefs.Save(); }

public void LoadProgress() { currentLevel = PlayerPrefs.GetInt(SAVE_KEY_LEVEL, 1); }

private void GeneratePattern(int length) { currentPattern.Clear(); int last = -1; for (int i = 0; i < length; i++) { int pick; do { pick = Random.Range(0, patternNodes.Count); } while (pick == last); currentPattern.Add(pick); last = pick; } }

} 