using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using System;

public class SimonUltimateShowdownScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;
    public KMColorblindMode colorblind;
    public GameObject[] cbtexts;
    private bool colorblindActive;

    private string[] modulesNames = { "Simon's Stages/Simon Stages", "Simon Scrambles", "Simon Screams", "Simon Stores (coming to a module near you Kappa)", "Simon Stops", "Tasha Squeals", "Simon Simons" };
    private int[] modulesOfButtons = { -1, -1, -1, -1, -1, -1 };
    private string[] colorsOfButtons = { "", "", "", "", "", "" };
    private List<string> correctColors = new List<string>();
    private List<string> correctColorsModified = new List<string>();
    private int stage = 0;
    private bool allSame;
    private bool resetButtons;
    private bool[] allSamePresses = new bool[6];
    private KMSelectable[] actualButtons = new KMSelectable[6];
    private List<GameObject> alreadyUp = new List<GameObject>();
    private List<int> typeUp = new List<int>();
    private List<int> posUp = new List<int>();
    private int posOfLastPress;
    private Coroutine timerCo;
    private int currentStrikes = 0;

    private string[] colorFlashes = { "", "", "", "", "" };
    private int[] buttonsFlashing = { -1, -1, -1, -1, -1 };
    private bool shouldFlash = true;
    private bool firstPress;
    private Coroutine pressFlash;
    private float timeBeforeNextSequence = 3.0f;
    private List<int> colorPositions = new List<int>();
    private List<int> colorPositionsModified = new List<int>();
    private int usedPriorityList;
    private List<string> presses = new List<string>();
    private bool cooldown = true;

    private string[] stagesColorNames = { "red", "blue", "yellow", "green", "orange", "pink", "magenta", "lime", "cyan", "white" };
    public Material[] stagesColorMats;
    public Color[] stagesButtonColors;
    public GameObject[] stagesButtonsObj;
    public GameObject[] stagesLightsObj;
    public KMSelectable[] stagesButtons;
    public GameObject[] stagesColoredBases;
    public GameObject[] stagesBases;
    public Light[] stagesLights;
    public TextMesh[] stagesTexts;
    private bool dealingWithLights = false;
    private int[] stagesRandomPressSounds = { 0, 0, 0, 0, 0, 0 };
    private bool[] stagesButtonsEnabled = { false, false, false, false, false, false };

    private string[] scramblesColorNames = { "red", "blue", "yellow", "green" };
    public Material[] scramblesColorMats;
    public GameObject[] scramblesButtonsObj;
    public GameObject[] scramblesLightsObj;
    public KMSelectable[] scramblesButtons;
    public Light[] scramblesLights;
    private bool[] scramblesButtonsEnabled = { false, false, false, false, false, false };

    private string[] screamsColorNames = { "red", "blue", "yellow", "green", "orange", "purple" };
    public Material[] screamsColorMats;
    public GameObject[] screamsButtonsObj;
    public GameObject[] screamsLightsObj;
    public KMSelectable[] screamsButtons;
    public Light[] screamsLights;
    private int[] screamsRandomFlashSounds = { 0, 0, 0, 0, 0, 0 };
    private int[] screamsRandomPressSounds = { 0, 0, 0, 0, 0, 0 };
    private bool[] screamsButtonsEnabled = { false, false, false, false, false, false };

    private string[] stopsColorNames = { "red", "blue", "yellow", "green", "orange", "purple" };
    public Material[] stopsColorMats;
    public GameObject[] stopsButtonsObj;
    public GameObject[] stopsLightsObj;
    public KMSelectable[] stopsButtons;
    public Light[] stopsLights;
    private int[] stopsRandomPressSounds = { 0, 0, 0, 0, 0, 0 };
    private bool[] stopsButtonsEnabled = { false, false, false, false, false, false };
    private int indexOfStop;
    private bool stopsAnnounceCtrlInput;
    private bool stopsCtrlInput = false;
    private string correctCtrlInput;
    private float timeStopsCtrlInput = 20.0f;
    private Coroutine ctrlInputTimer;
    private bool stopsMissedInput = false;

    private string[] tashaColorNames = { "pink", "green", "yellow", "blue" };
    private Color32[] tashaColors = { new Color32(236, 0, 220, 255), new Color32(0, 182, 0, 255), new Color32(223, 226, 0, 255), new Color32(21, 21, 161, 255) };
    public GameObject[] tashaButtonsObj;
    public GameObject[] tashaLightsObj;
    public KMSelectable[] tashaButtons;
    public Light[] tashaLights;
    private int[] tashaRandomPressSounds = { 0, 0, 0, 0, 0, 0 };
    private bool[] tashaButtonsEnabled = { false, false, false, false, false, false };

    private string[] simonsColorNames = { "red", "blue", "yellow", "green" };
    public Material[] simonsColorMats;
    public GameObject[] simonsButtonsObj;
    public GameObject[] simonsLightsObj;
    public KMSelectable[] simonsButtons;
    public Light[] simonsLights;
    private int[] simonsRandomPressSounds = { 0, 0, 0, 0, 0, 0 };
    private bool[] simonsButtonsEnabled = { false, false, false, false, false, false };

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        firstPress = false;
        resetButtons = true;
        foreach (KMSelectable obj in stagesButtons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        foreach (KMSelectable obj in scramblesButtons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        foreach (KMSelectable obj in screamsButtons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        foreach (KMSelectable obj in stopsButtons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        foreach (KMSelectable obj in tashaButtons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        foreach (KMSelectable obj in simonsButtons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        for (int i = 0; i < 6; i++)
        {
            stagesLights[i].enabled = false;
            scramblesLights[i].enabled = false;
            screamsLights[i].enabled = false;
            stopsLights[i].enabled = false;
            tashaLights[i].enabled = false;
            simonsLights[i].enabled = false;
        }
        ButtonsDown();
        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void OnActivate()
    {
        if (colorblindActive)
        {
            for(int i = 0; i < 6; i++)
            {
                cbtexts[i].SetActive(true);
            }
        }
        StartCoroutine(flashingStart());
    }

    void Update()
    {
        if (bomb.GetStrikes() != currentStrikes && currentStrikes != 2 && moduleSolved != true)
        {
            currentStrikes = bomb.GetStrikes();
            bool hi = false;
            for(int i = 0; i < stage+2; i++)
            {
                if (modulesOfButtons[buttonsFlashing[i]].Equals(6))
                {
                    hi = true;
                }
            }
            if (hi && !cooldown)
            {
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Change in the # of strikes detected! There may be new correct colors for the flashing Simon Simons button(s)!", moduleId);
                for (int i = 0; i < stage+2; i++)
                {
                    if (modulesOfButtons[buttonsFlashing[i]].Equals(6))
                    {
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] For flash {1} the new correct color from Simon Simons is...", moduleId, i+1);
                        getCorrectSimonsAnswer(colorFlashes[i], buttonsFlashing[i], i, false);
                    }
                }
                getFinalSequence(true);
            }
        }
    }

    void ImportantStartupStuff()
    {
        allSame = false;
        stage++;
        allSamePresses = new bool[6];
        stopsAnnounceCtrlInput = false;
        timeStopsCtrlInput = 20.0f;
        indexOfStop = -2;
        correctColors.Clear();
        colorPositions.Clear();
        correctColorsModified.Clear();
        presses.Clear();
        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Current Stage: {1} ({2} flashes)", moduleId, stage, stage + 2);
        //Prep all objects
        if (resetButtons)
        {
            alreadyUp.Clear();
            posUp.Clear();
            typeUp.Clear();
            stagesButtonsEnabled = new bool[6];
            scramblesButtonsEnabled = new bool[6];
            screamsButtonsEnabled = new bool[6];
            stopsButtonsEnabled = new bool[6];
            tashaButtonsEnabled = new bool[6];
            simonsButtonsEnabled = new bool[6];
            actualButtons = new KMSelectable[6];
            if (stopsMissedInput)
            {
                stopsMissedInput = false;
                shouldFlash = true;
                StartCoroutine(flashSequence());
            }
            for (int i = 0; i < 6; i++)
            {
                stagesButtonsObj[i].GetComponent<Renderer>().enabled = false;
                stagesLightsObj[i].SetActive(false);
                scramblesButtonsObj[i].GetComponent<Renderer>().enabled = false;
                scramblesLightsObj[i].SetActive(false);
                screamsButtonsObj[i].GetComponent<Renderer>().enabled = false;
                screamsLightsObj[i].SetActive(false);
                stopsButtonsObj[i].GetComponent<Renderer>().enabled = false;
                stopsLightsObj[i].SetActive(false);
                tashaButtonsObj[i].GetComponent<Renderer>().enabled = false;
                tashaButtons[i].gameObject.GetComponent<Renderer>().enabled = false;
                tashaLightsObj[i].SetActive(false);
                simonsButtonsObj[i].GetComponent<Renderer>().enabled = false;
                simonsLightsObj[i].SetActive(false);
            }
        }
    }

    void Start () {
        colorblindActive = colorblind.ColorblindModeActive;
        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Colorblind Mode: {1}", moduleId, colorblindActive);
        ImportantStartupStuff();
        //Randomize modules of buttons
        if (resetButtons)
        {
            for (int i = 0; i < 6; i++)
            {
                int rando = UnityEngine.Random.Range(0, 7);
                while(rando == 3)
                {
                    rando = UnityEngine.Random.Range(0, 7);
                }
                modulesOfButtons[i] = rando;
                ButtonUp(rando, i);
                if (rando == 0)
                {
                    stagesButtonsObj[i].GetComponent<Renderer>().enabled = true;
                    stagesLightsObj[i].SetActive(true);
                    stagesButtonsEnabled[i] = true;
                    actualButtons[i] = stagesButtons[i];
                    alreadyUp.Add(stagesButtonsObj[i]);
                }
                else if (rando == 1)
                {
                    scramblesButtonsObj[i].GetComponent<Renderer>().enabled = true;
                    scramblesLightsObj[i].SetActive(true);
                    scramblesButtonsEnabled[i] = true;
                    actualButtons[i] = scramblesButtons[i];
                    alreadyUp.Add(scramblesButtonsObj[i]);
                }
                else if (rando == 2)
                {
                    screamsButtonsObj[i].GetComponent<Renderer>().enabled = true;
                    screamsLightsObj[i].SetActive(true);
                    screamsButtonsEnabled[i] = true;
                    actualButtons[i] = screamsButtons[i];
                    alreadyUp.Add(screamsButtonsObj[i]);
                }
                else if (rando == 4)
                {
                    stopsButtonsObj[i].GetComponent<Renderer>().enabled = true;
                    stopsLightsObj[i].SetActive(true);
                    stopsButtonsEnabled[i] = true;
                    actualButtons[i] = stopsButtons[i];
                    alreadyUp.Add(stopsButtonsObj[i]);
                }
                else if (rando == 5)
                {
                    tashaButtonsObj[i].GetComponent<Renderer>().enabled = true;
                    tashaButtons[i].gameObject.GetComponent<Renderer>().enabled = true;
                    tashaLightsObj[i].SetActive(true);
                    tashaButtonsEnabled[i] = true;
                    actualButtons[i] = tashaButtons[i];
                    alreadyUp.Add(tashaButtonsObj[i]);
                }
                else if (rando == 6)
                {
                    simonsButtonsObj[i].GetComponent<Renderer>().enabled = true;
                    simonsLightsObj[i].SetActive(true);
                    simonsButtonsEnabled[i] = true;
                    actualButtons[i] = simonsButtons[i];
                    alreadyUp.Add(simonsButtonsObj[i]);
                }
                typeUp.Add(rando);
                posUp.Add(i);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The {1} button comes from {2}", moduleId, integerToPosition(i), modulesNames[rando]);
            }
        }
        else
        {
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The buttons come from the same modules as the last stage!", moduleId);
        }
        //Randomize attributes of module buttons
        //Sounds
        stagesRandomPressSounds = new int[6];
        screamsRandomFlashSounds = new int[6];
        screamsRandomPressSounds = new int[6];
        stopsRandomPressSounds = new int[6];
        tashaRandomPressSounds = new int[6];
        simonsRandomPressSounds = new int[6];
        for (int i = 0; i < 6; i++)
        {
            int rando = UnityEngine.Random.Range(1, 11);
            int rando2 = UnityEngine.Random.Range(1, 7);
            int rando3 = UnityEngine.Random.Range(7, 13);
            int rando4 = UnityEngine.Random.Range(1, 7);
            int rando5 = UnityEngine.Random.Range(1, 5);
            int rando6 = UnityEngine.Random.Range(1, 5);
            stagesRandomPressSounds[i] = rando;
            screamsRandomFlashSounds[i] = rando2;
            screamsRandomPressSounds[i] = rando3;
            stopsRandomPressSounds[i] = rando4;
            tashaRandomPressSounds[i] = rando5;
            simonsRandomPressSounds[i] = rando6;
            while (stagesSoundsEqual(i))
            {
                rando = UnityEngine.Random.Range(1, 11);
                stagesRandomPressSounds[i] = rando;
            }
            while (screamsSoundsEqual(i))
            {
                rando2 = UnityEngine.Random.Range(1, 7);
                rando3 = UnityEngine.Random.Range(7, 13);
                screamsRandomFlashSounds[i] = rando2;
                screamsRandomPressSounds[i] = rando3;
            }
            while (stopsSoundsEqual(i))
            {
                rando4 = UnityEngine.Random.Range(1, 7);
                stopsRandomPressSounds[i] = rando4;
            }
        }
        while (tashaSoundsEqual())
        {
            for (int i = 0; i < 6; i++)
            {
                int rando = UnityEngine.Random.Range(1, 5);
                tashaRandomPressSounds[i] = rando;
            }
        }
        while (simonsSoundsEqual())
        {
            for (int i = 0; i < 6; i++)
            {
                int rando = UnityEngine.Random.Range(1, 5);
                simonsRandomPressSounds[i] = rando;
            }
        }
        //Colors
        for (int i = 0; i < 6; i++)
        {
            int rando = UnityEngine.Random.Range(0, 10);
            int rando2 = UnityEngine.Random.Range(0, 4);
            int rando3 = UnityEngine.Random.Range(0, 6);
            int rando4 = UnityEngine.Random.Range(0, 6);
            int rando5 = UnityEngine.Random.Range(0, 4);
            int rando6 = UnityEngine.Random.Range(0, 4);
            if (modulesOfButtons[i] == 0)
            {
                colorsOfButtons[i] = stagesColorNames[rando];
                setStagesColor(colorsOfButtons[i], stagesTexts[i], stagesLights[i], stagesColoredBases[i]);
            }
            else if (modulesOfButtons[i] == 1)
            {
                colorsOfButtons[i] = scramblesColorNames[rando2];
                setScramblesColor(colorsOfButtons[i], scramblesLights[i], scramblesButtonsObj[i]);
            }
            else if (modulesOfButtons[i] == 2)
            {
                colorsOfButtons[i] = screamsColorNames[rando3];
                setScreamsColor(colorsOfButtons[i], screamsLights[i], screamsButtonsObj[i]);
            }
            else if (modulesOfButtons[i] == 4)
            {
                colorsOfButtons[i] = stopsColorNames[rando4];
                setStopsColor(colorsOfButtons[i], stopsLights[i], stopsButtonsObj[i]);
            }
            else if (modulesOfButtons[i] == 5)
            {
                colorsOfButtons[i] = tashaColorNames[rando5];
                setTashaColor(colorsOfButtons[i], tashaLights[i], tashaButtons[i]);
            }
            else if (modulesOfButtons[i] == 6)
            {
                colorsOfButtons[i] = simonsColorNames[rando5];
                setSimonsColor(colorsOfButtons[i], simonsButtons[i]);
            }
            if (colorsOfButtons[i].Equals("pink"))
            {
                cbtexts[i].GetComponent<TextMesh>().text = colorsOfButtons[i].ElementAt(1).ToString().ToUpper();
            }
            else
            {
                cbtexts[i].GetComponent<TextMesh>().text = colorsOfButtons[i].ElementAt(0).ToString().ToUpper();
            }
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The {1} button is the color {2}", moduleId, integerToPosition(i), colorsOfButtons[i]);
            //Set stages literal button colors
            if (resetButtons)
            {
                int randostages = UnityEngine.Random.Range(0, 2);
                if (randostages == 0)
                {
                    stagesButtonsObj[i].GetComponent<Renderer>().material = stagesColorMats[10];
                    stagesTexts[i].color = stagesButtonColors[0];
                }
                else
                {
                    stagesButtonsObj[i].GetComponent<Renderer>().material = stagesColorMats[11];
                    stagesTexts[i].color = stagesButtonColors[1];
                }
            }
        }
        //Randomize Flashes
        if (stage == 1)
        {
            for(int i = 0; i < 3; i++)
            {
                int rando = UnityEngine.Random.Range(0, 6);
                buttonsFlashing[i] = rando;
                colorFlashes[i] = colorsOfButtons[buttonsFlashing[i]];
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Flash {1} is the {2} button ({3})", moduleId, i+1, integerToPosition(buttonsFlashing[i]), colorFlashes[i]);
                if(modulesOfButtons[rando] == 4)
                {
                    if (!stopsAnnounceCtrlInput)
                    {
                        stopsAnnounceCtrlInput = true;
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] A Simon Stops button is flashing! This means a Control Input will be necessary!", moduleId);
                    }
                }
            }
        }
        else if (stage == 2)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i == 3)
                {
                    int rando = UnityEngine.Random.Range(0, 6);
                    buttonsFlashing[i] = rando;
                }
                colorFlashes[i] = colorsOfButtons[buttonsFlashing[i]];
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Flash {1} is the {2} button ({3})", moduleId, i + 1, integerToPosition(buttonsFlashing[i]), colorFlashes[i]);
                if (modulesOfButtons[buttonsFlashing[i]] == 4)
                {
                    if (!stopsAnnounceCtrlInput)
                    {
                        stopsAnnounceCtrlInput = true;
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] A Simon Stops button is flashing! This means a Control Input will be necessary!", moduleId);
                    }
                }
            }
        }
        else if (stage == 3)
        {
            for (int i = 0; i < 5; i++)
            {
                if (i == 4)
                {
                    int rando = UnityEngine.Random.Range(0, 6);
                    buttonsFlashing[i] = rando;
                }
                colorFlashes[i] = colorsOfButtons[buttonsFlashing[i]];
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Flash {1} is the {2} button ({3})", moduleId, i + 1, integerToPosition(buttonsFlashing[i]), colorFlashes[i]);
                if (modulesOfButtons[buttonsFlashing[i]] == 4)
                {
                    if (!stopsAnnounceCtrlInput)
                    {
                        stopsAnnounceCtrlInput = true;
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] A Simon Stops button is flashing! This means a Control Input will be necessary if all buttons are not the same color!", moduleId);
                    }
                }
            }
        }
        //Calculate answer
        if (allSameColors())
        {
            allSame = true;
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] All buttons have the same color! Press all Simon Stores buttons and then the rest in any order!", moduleId);
        }
        else
        {
            //Get initial correct colors
            for (int i = 0; i < stage + 2; i++)
            {
                if (modulesOfButtons[buttonsFlashing[i]] == 0)
                {
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] For flash {1} the correct colors from Simon's Stages/Simon Stages are...", moduleId, i + 1);
                    getCorrectStagesAnswer(colorFlashes[i], i);
                }
                else if (modulesOfButtons[buttonsFlashing[i]] == 1)
                {
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] For flash {1} the correct colors from Simon Scrambles are...", moduleId, i + 1);
                    getCorrectScramblesAnswer(colorFlashes[i], i);
                }
                else if (modulesOfButtons[buttonsFlashing[i]] == 2)
                {
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] For flash {1} the correct colors from Simon Screams are...", moduleId, i + 1);
                    getCorrectScreamsAnswer(colorFlashes[i], i);
                }
                else if (modulesOfButtons[buttonsFlashing[i]] == 4)
                {
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] For flash {1} the correct color from Simon Stops is...", moduleId, i + 1);
                    getCorrectStopsAnswer(colorFlashes[i], buttonsFlashing[i], i);
                }
                else if (modulesOfButtons[buttonsFlashing[i]] == 5)
                {
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] For flash {1} the correct color from Tasha Squeals is...", moduleId, i + 1);
                    getCorrectTashaAnswer(colorFlashes[i], buttonsFlashing[i], i);
                }
                else if (modulesOfButtons[buttonsFlashing[i]] == 6)
                {
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] For flash {1} the correct color from Simon Simons is...", moduleId, i + 1);
                    getCorrectSimonsAnswer(colorFlashes[i], buttonsFlashing[i], i, true);
                }
            }
            getFinalSequence(false);
        }
        if (stopsAnnounceCtrlInput && !allSame)
        {
            if (correctColorsModified.Count == 1)
                indexOfStop = 0;
            else
                indexOfStop = UnityEngine.Random.Range(0, correctColorsModified.Count);
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Control Input from Simon Stops will occur after press {1}!", moduleId, indexOfStop+1);
        }
        resetButtons = false;
    }

    private void getFinalSequence(bool verynew)
    {
        //output initial sequence
        string output = "";
        for (int i = 0; i < correctColors.Count; i++)
        {
            if (i == (correctColors.Count - 1))
            {
                output += correctColors[i];
            }
            else
            {
                output += correctColors[i] + ", ";
            }
        }
        if(!verynew)
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The initial sequence of colors before modification is {1}", moduleId, output);
        //Make modifications
        correctColorsModified = correctColors;
        colorPositionsModified = colorPositions;
        //Rule 1
        if ((bomb.IsIndicatorPresent("SND") || bomb.IsIndicatorPresent("CAR")) && colorFlashes[1].Equals("green"))
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 1 is True", moduleId);
            string col1 = correctColors.ElementAt(colorPositions.IndexOf(1));
            string col2 = correctColors.ElementAt(colorPositions.IndexOf(2));
            string col3 = correctColors.ElementAt(colorPositions.IndexOf(3));
            string col4 = "";
            string col5 = "";
            if (stage == 2 || stage == 3)
                col4 = correctColors.ElementAt(colorPositions.IndexOf(4));
            if (stage == 3)
                col5 = correctColors.ElementAt(colorPositions.IndexOf(5));
            colorPositionsModified.Clear();
            correctColorsModified.Clear();
            correctColorsModified.Add(col1);
            correctColorsModified.Add(col2);
            correctColorsModified.Add(col3);
            colorPositionsModified.Add(1);
            colorPositionsModified.Add(2);
            colorPositionsModified.Add(3);
            if (stage == 1)
            {
                if (!verynew)
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The new modified color sequence after rule 1 is {1}, {2}, and {3}", moduleId, correctColorsModified.ElementAt(0), correctColorsModified.ElementAt(1), correctColorsModified.ElementAt(2));
            }
            else if (stage == 2)
            {
                correctColorsModified.Add(col4);
                colorPositionsModified.Add(4);
                if (!verynew)
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The new modified color sequence after rule 1 is {1}, {2}, {3}, and {4}", moduleId, correctColorsModified.ElementAt(0), correctColorsModified.ElementAt(1), correctColorsModified.ElementAt(2), correctColorsModified.ElementAt(3));
            }
            else if (stage == 3)
            {
                correctColorsModified.Add(col4);
                correctColorsModified.Add(col5);
                colorPositionsModified.Add(4);
                colorPositionsModified.Add(5);
                if (!verynew)
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The new modified color sequence after rule 1 is {1}, {2}, {3}, {4}, and {5}", moduleId, correctColorsModified.ElementAt(0), correctColorsModified.ElementAt(1), correctColorsModified.ElementAt(2), correctColorsModified.ElementAt(3), correctColorsModified.ElementAt(4));
            }
        }
        else
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 1 is False", moduleId);
        }
        //Rule 2
        if (colorsNeededForFlash(3).Contains("red"))
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 2 is True", moduleId);
            for (int i = 0; i < correctColorsModified.Count; i++)
            {
                if (correctColorsModified[i].Equals("red"))
                {
                    correctColorsModified[i] = "blue";
                }
            }
            string temp = "";
            for (int i = 0; i < correctColorsModified.Count; i++)
            {
                if (i == (correctColorsModified.Count - 1))
                {
                    temp += correctColorsModified[i];
                }
                else
                {
                    temp += correctColorsModified[i] + ", ";
                }
            }
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The new modified color sequence after rule 2 is {1}", moduleId, temp);
        }
        else
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 2 is False", moduleId);
        }
        //Rule 3
        if (sumOfSimonMods() >= 2)
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 3 is True", moduleId);
            for (int i = 0; i < correctColorsModified.Count; i++)
            {
                if (correctColorsModified[i].Equals("orange"))
                {
                    correctColorsModified[i] = "white";
                }
                else if (correctColorsModified[i].Equals("green"))
                {
                    correctColorsModified[i] = "lime";
                }
            }
            string temp = "";
            for (int i = 0; i < correctColorsModified.Count; i++)
            {
                if (i == (correctColorsModified.Count - 1))
                {
                    temp += correctColorsModified[i];
                }
                else
                {
                    temp += correctColorsModified[i] + ", ";
                }
            }
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The new modified color sequence after rule 3 is {1}", moduleId, temp);
        }
        else
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 3 is False", moduleId);
        }
        //Rule 4
        if (colorsNeededForFlash(2).Contains("purple"))
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 4 is True", moduleId);
            for (int i = 0; i < correctColorsModified.Count; i++)
            {
                if (correctColorsModified[i].Equals("yellow"))
                {
                    correctColorsModified[i] = "orange";
                }
            }
            correctColorsModified.Add("green");
            colorPositionsModified.Add(0);
            string temp = "";
            for (int i = 0; i < correctColorsModified.Count; i++)
            {
                if (i == (correctColorsModified.Count - 1))
                {
                    temp += correctColorsModified[i];
                }
                else
                {
                    temp += correctColorsModified[i] + ", ";
                }
            }
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The new modified color sequence after rule 4 is {1}", moduleId, temp);
        }
        else
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 4 is False", moduleId);
        }
        //Rule 5
        if ((colorFlashes[1].Equals("yellow") || colorFlashes[1].Equals("blue") || colorFlashes[2].Equals("yellow") || colorFlashes[2].Equals("blue")) && (bomb.GetSerialNumberNumbers().Contains(0) || bomb.GetSerialNumberNumbers().Contains(5) || bomb.GetSerialNumberNumbers().Contains(9)))
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 5 is True", moduleId);
            correctColorsModified.Insert(colorPositionsModified.IndexOf(2), "cyan");
            colorPositionsModified.Insert(colorPositionsModified.IndexOf(2), 0);
            correctColorsModified.Insert(colorPositionsModified.IndexOf(2), "purple");
            colorPositionsModified.Insert(colorPositionsModified.IndexOf(2), 0);
            string temp = "";
            for (int i = 0; i < correctColorsModified.Count; i++)
            {
                if (i == (correctColorsModified.Count - 1))
                {
                    temp += correctColorsModified[i];
                }
                else
                {
                    temp += correctColorsModified[i] + ", ";
                }
            }
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The new modified color sequence after rule 5 is {1}", moduleId, temp);
        }
        else
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 5 is False", moduleId);
        }
        //Rule 6
        if (colorFlashes[0].Equals("red") || colorFlashes[0].Equals("orange") || colorFlashes[0].Equals("lime"))
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 6 is True", moduleId);
            for (int i = 0; i < correctColorsModified.Count; i++)
            {
                if (correctColorsModified[i].Equals("blue"))
                {
                    correctColorsModified[i] = "cyan";
                }
            }
            string temp = "";
            for (int i = 0; i < correctColorsModified.Count; i++)
            {
                if (i == (correctColorsModified.Count - 1))
                {
                    temp += correctColorsModified[i];
                }
                else
                {
                    temp += correctColorsModified[i] + ", ";
                }
            }
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The new modified color sequence after rule 6 is {1}", moduleId, temp);
        }
        else
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 6 is False", moduleId);
        }
        //Rule 7
        if ((stage == 2) && halfOfFlashesWerePrimary())
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 7 is True", moduleId);
            while (correctColorsModified.Contains("orange"))
            {
                correctColorsModified.Remove("orange");
            }
            while (correctColorsModified.Contains("yellow"))
            {
                correctColorsModified.Remove("yellow");
            }
            while (correctColorsModified.Contains("white"))
            {
                correctColorsModified.Remove("white");
            }
            //at this point colorPositionsModified is no longer usable because some flash's presses may have been removed entirely
            //therefore we do not need to worry about it anymore since there will be no more rules after this
            string temp = "";
            for (int i = 0; i < correctColorsModified.Count; i++)
            {
                if (i == (correctColorsModified.Count - 1))
                {
                    temp += correctColorsModified[i];
                }
                else
                {
                    temp += correctColorsModified[i] + ", ";
                }
            }
            if (correctColorsModified.Count == 0)
            {
                if (!verynew)
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The new modified color sequence after rule 7 is nothing", moduleId);
            }
            else
            {
                if (!verynew)
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The new modified color sequence after rule 7 is {1}", moduleId, temp);
            }
        }
        else
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Modification rule 7 is False", moduleId);
        }
        //Apply priority lists
        bool usedList = false;
        for (int i = 0; i < correctColorsModified.Count; i++)
        {
            if (!colorsOfButtons.Contains(correctColorsModified[i]))
            {
                usedList = true;
                correctColorsModified[i] = getPriorityListColor();
            }
        }
        string temp2 = "";
        for (int i = 0; i < correctColorsModified.Count; i++)
        {
            if (i == (correctColorsModified.Count - 1))
            {
                temp2 += correctColorsModified[i];
            }
            else
            {
                temp2 += correctColorsModified[i] + ", ";
            }
        }
        if (usedList)
        {
            if (correctColorsModified.Count == 0)
            {
                if (!verynew)
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The new modified color sequence after applying priority list {1} is nothing", moduleId, usedPriorityList);
            }
            else
            {
                if (!verynew)
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The new modified color sequence after applying priority list {1} is {2}", moduleId, usedPriorityList, temp2);
            }
        }
        if (correctColorsModified.Count == 0)
        {
            temp2 = getPriorityListColor();
            correctColorsModified.Add(temp2);
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Due to no detected presses the final modified color sequence for stage {1} is {2} according to priority list {3}", moduleId, stage, temp2);
        }
        else
        {
            if (!verynew)
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The final modified color sequence for stage {1} is {2}", moduleId, stage, temp2);
            else
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The new final modified color sequence for stage {1} is {2}", moduleId, stage, temp2);
        }
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleSolved != true && cooldown != true)
        {
            if (pressed == stagesButtons[0] && stagesButtonsEnabled[0])
            {
                HandlePress(0, 0);
            }
            else if (pressed == stagesButtons[1] && stagesButtonsEnabled[1])
            {
                HandlePress(0, 1);
            }
            else if (pressed == stagesButtons[2] && stagesButtonsEnabled[2])
            {
                HandlePress(0, 2);
            }
            else if (pressed == stagesButtons[3] && stagesButtonsEnabled[3])
            {
                HandlePress(0, 3);
            }
            else if (pressed == stagesButtons[4] && stagesButtonsEnabled[4])
            {
                HandlePress(0, 4);
            }
            else if (pressed == stagesButtons[5] && stagesButtonsEnabled[5])
            {
                HandlePress(0, 5);
            }
            else if (pressed == scramblesButtons[0] && scramblesButtonsEnabled[0])
            {
                HandlePress(1, 0);
            }
            else if (pressed == scramblesButtons[1] && scramblesButtonsEnabled[1])
            {
                HandlePress(1, 1);
            }
            else if (pressed == scramblesButtons[2] && scramblesButtonsEnabled[2])
            {
                HandlePress(1, 2);
            }
            else if (pressed == scramblesButtons[3] && scramblesButtonsEnabled[3])
            {
                HandlePress(1, 3);
            }
            else if (pressed == scramblesButtons[4] && scramblesButtonsEnabled[4])
            {
                HandlePress(1, 4);
            }
            else if (pressed == scramblesButtons[5] && scramblesButtonsEnabled[5])
            {
                HandlePress(1, 5);
            }
            else if (pressed == screamsButtons[0] && screamsButtonsEnabled[0])
            {
                HandlePress(2, 0);
            }
            else if (pressed == screamsButtons[1] && screamsButtonsEnabled[1])
            {
                HandlePress(2, 1);
            }
            else if (pressed == screamsButtons[2] && screamsButtonsEnabled[2])
            {
                HandlePress(2, 2);
            }
            else if (pressed == screamsButtons[3] && screamsButtonsEnabled[3])
            {
                HandlePress(2, 3);
            }
            else if (pressed == screamsButtons[4] && screamsButtonsEnabled[4])
            {
                HandlePress(2, 4);
            }
            else if (pressed == screamsButtons[5] && screamsButtonsEnabled[5])
            {
                HandlePress(2, 5);
            }
            else if (pressed == stopsButtons[0] && stopsButtonsEnabled[0])
            {
                HandlePress(4, 0);
            }
            else if (pressed == stopsButtons[1] && stopsButtonsEnabled[1])
            {
                HandlePress(4, 1);
            }
            else if (pressed == stopsButtons[2] && stopsButtonsEnabled[2])
            {
                HandlePress(4, 2);
            }
            else if (pressed == stopsButtons[3] && stopsButtonsEnabled[3])
            {
                HandlePress(4, 3);
            }
            else if (pressed == stopsButtons[4] && stopsButtonsEnabled[4])
            {
                HandlePress(4, 4);
            }
            else if (pressed == stopsButtons[5] && stopsButtonsEnabled[5])
            {
                HandlePress(4, 5);
            }
            else if (pressed == tashaButtons[0] && tashaButtonsEnabled[0])
            {
                HandlePress(5, 0);
            }
            else if (pressed == tashaButtons[1] && tashaButtonsEnabled[1])
            {
                HandlePress(5, 1);
            }
            else if (pressed == tashaButtons[2] && tashaButtonsEnabled[2])
            {
                HandlePress(5, 2);
            }
            else if (pressed == tashaButtons[3] && tashaButtonsEnabled[3])
            {
                HandlePress(5, 3);
            }
            else if (pressed == tashaButtons[4] && tashaButtonsEnabled[4])
            {
                HandlePress(5, 4);
            }
            else if (pressed == tashaButtons[5] && tashaButtonsEnabled[5])
            {
                HandlePress(5, 5);
            }
            else if (pressed == simonsButtons[0] && simonsButtonsEnabled[0])
            {
                HandlePress(6, 0);
            }
            else if (pressed == simonsButtons[1] && simonsButtonsEnabled[1])
            {
                HandlePress(6, 1);
            }
            else if (pressed == simonsButtons[2] && simonsButtonsEnabled[2])
            {
                HandlePress(6, 2);
            }
            else if (pressed == simonsButtons[3] && simonsButtonsEnabled[3])
            {
                HandlePress(6, 3);
            }
            else if (pressed == simonsButtons[4] && simonsButtonsEnabled[4])
            {
                HandlePress(6, 4);
            }
            else if (pressed == simonsButtons[5] && simonsButtonsEnabled[5])
            {
                HandlePress(6, 5);
            }
        }
    }

    private void HandlePress(int type, int pos)
    {
        firstPress = true;
        if (pressFlash == null)
        {
            if (type == 6)
            {
                simonsButtons[pos].AddInteractionPunch();
                audio.PlaySoundAtTransform("simonspress" + simonsRandomPressSounds[pos], simonsButtons[pos].transform);
            }
            else if (type == 5)
            {
                tashaButtons[pos].AddInteractionPunch(2f);
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, tashaButtons[pos].transform);
                audio.PlaySoundAtTransform("tasha" + tashaRandomPressSounds[pos], tashaButtons[pos].transform);
            }
            else if (type == 4)
                audio.PlaySoundAtTransform("tone" + stopsRandomPressSounds[pos], stopsButtons[pos].transform);
            else if (type == 2)
            {
                screamsButtons[pos].AddInteractionPunch();
                audio.PlaySoundAtTransform("Sound" + screamsRandomPressSounds[pos], screamsButtons[pos].transform);
            }
            else if (type == 1)
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, scramblesButtons[pos].transform);
            else if (type == 0)
            {
                stagesButtons[pos].AddInteractionPunch(0.25f);
                audio.PlaySoundAtTransform("press" + stagesRandomPressSounds[pos], stagesButtons[pos].transform);
            }
            shouldFlash = false;
            for (int i = 0; i < 6; i++)
            {
                stagesLights[i].enabled = false;
                scramblesLights[i].enabled = false;
                screamsLights[i].enabled = false;
                stopsLights[i].enabled = false;
                tashaLights[i].enabled = false;
                simonsLights[i].enabled = false;
                stagesBases[i].SetActive(true);
            }
            if (timeBeforeNextSequence == 3.0f)
                timerCo = StartCoroutine(timer());
            else
                timeBeforeNextSequence = 3.0f;
            if (type == 6)
                pressFlash = StartCoroutine(singleSimonsFlash(simonsLights[pos]));
            else if (type == 5)
                pressFlash = StartCoroutine(singleTashaFlash(tashaLights[pos]));
            else if (type == 4)
                pressFlash = StartCoroutine(singleStopsFlash(stopsLights[pos]));
            else if (type == 2)
                pressFlash = StartCoroutine(singleScreamsFlash(screamsLights[pos]));
            else if (type == 1)
                pressFlash = StartCoroutine(singleScramblesFlash(scramblesLights[pos]));
            else if (type == 0)
                pressFlash = StartCoroutine(singleStagesFlash(stagesLights[pos], stagesBases[pos]));
            posOfLastPress = pos;
            if (allSame)
            {
                //replace 3 with stores button
                if ((modulesOfButtons[pos] == 3 && remainingStoresButtons()) || (!remainingStoresButtons() && allSamePresses[pos] == false))
                {
                    if (pos == 0)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Top Left button ({1}) has been pressed, that is correct", moduleId, colorsOfButtons[pos]);
                    else if (pos == 1)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Top Right button ({1}) has been pressed, that is correct", moduleId, colorsOfButtons[pos]);
                    else if (pos == 2)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Right button ({1}) has been pressed, that is correct", moduleId, colorsOfButtons[pos]);
                    else if (pos == 3)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Bottom Right button ({1}) has been pressed, that is correct", moduleId, colorsOfButtons[pos]);
                    else if (pos == 4)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Bottom Left button ({1}) has been pressed, that is correct", moduleId, colorsOfButtons[pos]);
                    else if (pos == 5)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Left button ({1}) has been pressed, that is correct", moduleId, colorsOfButtons[pos]);
                    allSamePresses[pos] = true;
                }
                else
                {
                    if (pos == 0)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Top Left button ({1}) has been pressed, that is incorrect. Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 1)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Top Right button ({1}) has been pressed, that is incorrect. Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 2)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Right button ({1}) has been pressed, that is incorrect. Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 3)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Bottom Right button ({1}) has been pressed, that is incorrect. Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 4)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Bottom Left button ({1}) has been pressed, that is incorrect. Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 5)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Left button ({1}) has been pressed, that is incorrect. Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    GetComponent<KMBombModule>().HandleStrike();
                    resetButtons = true;
                    stage--;
                    cooldown = true;
                    StartCoroutine(inCooldown(true));
                }
                //check for win
                if (allSamePresses[0] == true && allSamePresses[1] == true && allSamePresses[2] == true && allSamePresses[3] == true && allSamePresses[4] == true && allSamePresses[5] == true && stage != 3)
                {
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] All buttons have been pressed, advancing to next stage...", moduleId);
                    cooldown = true;
                    StartCoroutine(inCooldown(false));
                }
                else if (allSamePresses[0] == true && allSamePresses[1] == true && allSamePresses[2] == true && allSamePresses[3] == true && allSamePresses[4] == true && allSamePresses[5] == true && stage == 3)
                {
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] All buttons have been pressed, module disarmed!", moduleId);
                    moduleSolved = true;
                    shouldFlash = false;
                    StopCoroutine(pressFlash);
                    StartCoroutine(solveAnim());
                    GetComponent<KMBombModule>().HandlePass();
                }
            }
            else if (stopsCtrlInput)
            {
                if (colorsOfButtons[pos].Equals(correctCtrlInput))
                {
                    if (pos == 0)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Top Left button ({1}) has been pressed, that is correct (Control Input)", moduleId, colorsOfButtons[pos]);
                    else if (pos == 1)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Top Right button ({1}) has been pressed, that is correct (Control Input)", moduleId, colorsOfButtons[pos]);
                    else if (pos == 2)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Right button ({1}) has been pressed, that is correct (Control Input)", moduleId, colorsOfButtons[pos]);
                    else if (pos == 3)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Bottom Right button ({1}) has been pressed, that is correct (Control Input)", moduleId, colorsOfButtons[pos]);
                    else if (pos == 4)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Bottom Left button ({1}) has been pressed, that is correct (Control Input)", moduleId, colorsOfButtons[pos]);
                    else if (pos == 5)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Left button ({1}) has been pressed, that is correct (Control Input)", moduleId, colorsOfButtons[pos]);
                    Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Control Input entered successfully! The module can now continue!", moduleId);
                    StopCoroutine(ctrlInputTimer);
                    //check for win
                    if (allTheSameAnswers() && stage != 3)
                    {
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] All correct colors have been pressed, advancing to next stage...", moduleId);
                        cooldown = true;
                        StartCoroutine(inCooldown(false));
                    }
                    else if (allTheSameAnswers() && stage == 3)
                    {
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] All correct colors have been pressed, module disarmed!", moduleId);
                        moduleSolved = true;
                        shouldFlash = false;
                        StopCoroutine(pressFlash);
                        StartCoroutine(solveAnim());
                        GetComponent<KMBombModule>().HandlePass();
                    }
                }
                else
                {
                    if (pos == 0)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Top Left button ({1}) has been pressed, that is incorrect (Control Input). Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 1)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Top Right button ({1}) has been pressed, that is incorrect (Control Input). Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 2)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Right button ({1}) has been pressed, that is incorrect (Control Input). Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 3)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Bottom Right button ({1}) has been pressed, that is incorrect (Control Input). Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 4)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Bottom Left button ({1}) has been pressed, that is incorrect (Control Input). Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 5)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Left button ({1}) has been pressed, that is incorrect (Control Input). Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    GetComponent<KMBombModule>().HandleStrike();
                    StopCoroutine(ctrlInputTimer);
                    StopCoroutine(timerCo);
                    resetButtons = true;
                    stage--;
                    cooldown = true;
                    stopsMissedInput = true;
                    StartCoroutine(inCooldown(true));
                }
                stopsCtrlInput = false;
            }
            else
            {
                if (colorsOfButtons[pos].Equals(correctColorsModified[presses.Count]))
                {
                    presses.Add(colorsOfButtons[pos]);
                    if (pos == 0)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Top Left button ({1}) has been pressed, that is correct", moduleId, colorsOfButtons[pos]);
                    else if (pos == 1)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Top Right button ({1}) has been pressed, that is correct", moduleId, colorsOfButtons[pos]);
                    else if (pos == 2)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Right button ({1}) has been pressed, that is correct", moduleId, colorsOfButtons[pos]);
                    else if (pos == 3)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Bottom Right button ({1}) has been pressed, that is correct", moduleId, colorsOfButtons[pos]);
                    else if (pos == 4)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Bottom Left button ({1}) has been pressed, that is correct", moduleId, colorsOfButtons[pos]);
                    else if (pos == 5)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Left button ({1}) has been pressed, that is correct", moduleId, colorsOfButtons[pos]);
                    if (indexOfStop == (presses.Count-1))
                    {
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Time for Control Input! Calculating Result...", moduleId);
                        correctCtrlInput = getStopsCtrlInput();
                        stopsCtrlInput = true;
                        ctrlInputTimer = StartCoroutine(ctrlInputTimerMethod());
                        StopCoroutine(timerCo);
                    }
                }
                else
                {
                    if (pos == 0)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Top Left button ({1}) has been pressed, that is incorrect. Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 1)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Top Right button ({1}) has been pressed, that is incorrect. Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 2)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Right button ({1}) has been pressed, that is incorrect. Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 3)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Bottom Right button ({1}) has been pressed, that is incorrect. Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 4)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Bottom Left button ({1}) has been pressed, that is incorrect. Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    else if (pos == 5)
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] The Left button ({1}) has been pressed, that is incorrect. Strike! Resetting stage...", moduleId, colorsOfButtons[pos]);
                    GetComponent<KMBombModule>().HandleStrike();
                    resetButtons = true;
                    stage--;
                    cooldown = true;
                    StartCoroutine(inCooldown(true));
                }
                //here so when control input comes up it does not check for win until input is done
                if (!stopsCtrlInput)
                {
                    //check for win
                    if (allTheSameAnswers() && stage != 3)
                    {
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] All correct colors have been pressed, advancing to next stage...", moduleId);
                        cooldown = true;
                        StartCoroutine(inCooldown(false));
                    }
                    else if (allTheSameAnswers() && stage == 3)
                    {
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] All correct colors have been pressed, module disarmed!", moduleId);
                        moduleSolved = true;
                        shouldFlash = false;
                        StopCoroutine(pressFlash);
                        pressFlash = null;
                        posOfLastPress = pos;
                        StartCoroutine(solveAnim());
                        GetComponent<KMBombModule>().HandlePass();
                    }
                }
            }
        }
    }

    private void getCorrectStagesAnswer(string color, int flashNum)
    {
        if (color.Equals("red"))
        {
            if (stage == 1)
            {
                correctColors.Add(colorFlashes[0]);
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorFlashes[1]);
                colorPositions.Add(0);
                correctColors.Add(colorFlashes[2]);
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}, {2}, and {3}", moduleId, colorFlashes[0], colorFlashes[1], colorFlashes[2]);
            }
            else if (stage == 2)
            {
                correctColors.Add(colorFlashes[0]);
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorFlashes[1]);
                colorPositions.Add(0);
                correctColors.Add(colorFlashes[2]);
                colorPositions.Add(0);
                correctColors.Add(colorFlashes[3]);
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}, {2}, {3}, and {4}", moduleId, colorFlashes[0], colorFlashes[1], colorFlashes[2], colorFlashes[3]);
            }
            else if (stage == 3)
            {
                correctColors.Add(colorFlashes[0]);
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorFlashes[1]);
                colorPositions.Add(0);
                correctColors.Add(colorFlashes[2]);
                colorPositions.Add(0);
                correctColors.Add(colorFlashes[3]);
                colorPositions.Add(0);
                correctColors.Add(colorFlashes[4]);
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}, {2}, {3}, {4}, and {5}", moduleId, colorFlashes[0], colorFlashes[1], colorFlashes[2], colorFlashes[3], colorFlashes[4]);
            }
        }
        else if (color.Equals("blue"))
        {
            if (stage == 1)
            {
                correctColors.Add(colorFlashes[2]);
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorFlashes[1]);
                colorPositions.Add(0);
                correctColors.Add(colorFlashes[0]);
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}, {2}, and {3}", moduleId, colorFlashes[2], colorFlashes[1], colorFlashes[0]);
            }
            else if (stage == 2)
            {
                correctColors.Add(colorFlashes[3]);
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorFlashes[2]);
                colorPositions.Add(0);
                correctColors.Add(colorFlashes[1]);
                colorPositions.Add(0);
                correctColors.Add(colorFlashes[0]);
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}, {2}, {3}, and {4}", moduleId, colorFlashes[3], colorFlashes[2], colorFlashes[1], colorFlashes[0]);
            }
            else if (stage == 3)
            {
                correctColors.Add(colorFlashes[4]);
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorFlashes[3]);
                colorPositions.Add(0);
                correctColors.Add(colorFlashes[2]);
                colorPositions.Add(0);
                correctColors.Add(colorFlashes[1]);
                colorPositions.Add(0);
                correctColors.Add(colorFlashes[0]);
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}, {2}, {3}, {4}, and {5}", moduleId, colorFlashes[4], colorFlashes[3], colorFlashes[2], colorFlashes[1], colorFlashes[0]);
            }
        }
        else if (color.Equals("yellow"))
        {
            correctColors.Add(colorFlashes[0]);
            colorPositions.Add(flashNum + 1);
            correctColors.Add(colorFlashes[1]);
            colorPositions.Add(0);
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, colorFlashes[0], colorFlashes[1]);
        }
        else if (color.Equals("orange"))
        {
            correctColors.Add(colorFlashes[1]);
            colorPositions.Add(flashNum + 1);
            correctColors.Add(colorFlashes[0]);
            colorPositions.Add(0);
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, colorFlashes[1], colorFlashes[0]);
        }
        else if (color.Equals("magenta"))
        {
            if (stage == 1)
            {
                correctColors.Add(colorFlashes[1]);
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorFlashes[2]);
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, colorFlashes[1], colorFlashes[2]);
            }
            else if (stage == 2)
            {
                correctColors.Add(colorFlashes[2]);
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorFlashes[3]);
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, colorFlashes[2], colorFlashes[3]);
            }
            else if (stage == 3)
            {
                correctColors.Add(colorFlashes[3]);
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorFlashes[4]);
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, colorFlashes[3], colorFlashes[4]);
            }
        }
        else if (color.Equals("green"))
        {
            if (stage == 1)
            {
                correctColors.Add(colorFlashes[2]);
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorFlashes[1]);
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, colorFlashes[2], colorFlashes[1]);
            }
            else if (stage == 2)
            {
                correctColors.Add(colorFlashes[3]);
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorFlashes[2]);
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, colorFlashes[3], colorFlashes[2]);
            }
            else if (stage == 3)
            {
                correctColors.Add(colorFlashes[4]);
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorFlashes[3]);
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, colorFlashes[4], colorFlashes[3]);
            }
        }
        else if (color.Equals("pink"))
        {
            if (stage == 1)
            {
                correctColors.Add(colorOppositeButton(buttonsFlashing[0]));
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorOppositeButton(buttonsFlashing[1]));
                colorPositions.Add(0);
                correctColors.Add(colorOppositeButton(buttonsFlashing[2]));
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}, {2}, and {3}", moduleId, colorOppositeButton(buttonsFlashing[0]), colorOppositeButton(buttonsFlashing[1]), colorOppositeButton(buttonsFlashing[2]));
            }
            else if (stage == 2)
            {
                correctColors.Add(colorOppositeButton(buttonsFlashing[0]));
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorOppositeButton(buttonsFlashing[1]));
                colorPositions.Add(0);
                correctColors.Add(colorOppositeButton(buttonsFlashing[2]));
                colorPositions.Add(0);
                correctColors.Add(colorOppositeButton(buttonsFlashing[3]));
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}, {2}, {3}, and {4}", moduleId, colorOppositeButton(buttonsFlashing[0]), colorOppositeButton(buttonsFlashing[1]), colorOppositeButton(buttonsFlashing[2]), colorOppositeButton(buttonsFlashing[3]));
            }
            else if (stage == 3)
            {
                correctColors.Add(colorOppositeButton(buttonsFlashing[0]));
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorOppositeButton(buttonsFlashing[1]));
                colorPositions.Add(0);
                correctColors.Add(colorOppositeButton(buttonsFlashing[2]));
                colorPositions.Add(0);
                correctColors.Add(colorOppositeButton(buttonsFlashing[3]));
                colorPositions.Add(0);
                correctColors.Add(colorOppositeButton(buttonsFlashing[4]));
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}, {2}, {3}, {4}, and {5}", moduleId, colorOppositeButton(buttonsFlashing[0]), colorOppositeButton(buttonsFlashing[1]), colorOppositeButton(buttonsFlashing[2]), colorOppositeButton(buttonsFlashing[3]), colorOppositeButton(buttonsFlashing[4]));
            }
        }
        else if (color.Equals("lime"))
        {
            if (stage == 1)
            {
                correctColors.Add(colorOppositeButton(buttonsFlashing[2]));
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorOppositeButton(buttonsFlashing[1]));
                colorPositions.Add(0);
                correctColors.Add(colorOppositeButton(buttonsFlashing[0]));
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}, {2}, and {3}", moduleId, colorOppositeButton(buttonsFlashing[2]), colorOppositeButton(buttonsFlashing[1]), colorOppositeButton(buttonsFlashing[0]));
            }
            else if (stage == 2)
            {
                correctColors.Add(colorOppositeButton(buttonsFlashing[3]));
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorOppositeButton(buttonsFlashing[2]));
                colorPositions.Add(0);
                correctColors.Add(colorOppositeButton(buttonsFlashing[1]));
                colorPositions.Add(0);
                correctColors.Add(colorOppositeButton(buttonsFlashing[0]));
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}, {2}, {3}, and {4}", moduleId, colorOppositeButton(buttonsFlashing[3]), colorOppositeButton(buttonsFlashing[2]), colorOppositeButton(buttonsFlashing[1]), colorOppositeButton(buttonsFlashing[0]));
            }
            else if (stage == 3)
            {
                correctColors.Add(colorOppositeButton(buttonsFlashing[4]));
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorOppositeButton(buttonsFlashing[3]));
                colorPositions.Add(0);
                correctColors.Add(colorOppositeButton(buttonsFlashing[2]));
                colorPositions.Add(0);
                correctColors.Add(colorOppositeButton(buttonsFlashing[1]));
                colorPositions.Add(0);
                correctColors.Add(colorOppositeButton(buttonsFlashing[0]));
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}, {2}, {3}, {4}, and {5}", moduleId, colorOppositeButton(buttonsFlashing[4]), colorOppositeButton(buttonsFlashing[3]), colorOppositeButton(buttonsFlashing[2]), colorOppositeButton(buttonsFlashing[1]), colorOppositeButton(buttonsFlashing[0]));
            }
        }
        else if (color.Equals("cyan"))
        {
            if (stage == 1)
            {
                correctColors.Add(colorOppositeButton(buttonsFlashing[0]));
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorOppositeButton(buttonsFlashing[2]));
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, colorOppositeButton(buttonsFlashing[0]), colorOppositeButton(buttonsFlashing[2]));
            }
            else if (stage == 2)
            {
                correctColors.Add(colorOppositeButton(buttonsFlashing[0]));
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorOppositeButton(buttonsFlashing[3]));
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, colorOppositeButton(buttonsFlashing[0]), colorOppositeButton(buttonsFlashing[3]));
            }
            else if (stage == 3)
            {
                correctColors.Add(colorOppositeButton(buttonsFlashing[0]));
                colorPositions.Add(flashNum + 1);
                correctColors.Add(colorOppositeButton(buttonsFlashing[4]));
                colorPositions.Add(0);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, colorOppositeButton(buttonsFlashing[0]), colorOppositeButton(buttonsFlashing[4]));
            }
        }
        else if (color.Equals("white"))
        {
            correctColors.Add(colorOppositeButton(buttonsFlashing[2]));
            colorPositions.Add(flashNum + 1);
            correctColors.Add(colorOppositeButton(buttonsFlashing[1]));
            colorPositions.Add(0);
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, colorOppositeButton(buttonsFlashing[2]), colorOppositeButton(buttonsFlashing[1]));
        }
    }

    private void getCorrectScramblesAnswer(string color, int flashNum)
    {
        if (color.Equals("red"))
        {
            string[] correctRedCols = { "blue", "yellow", "yellow", "green", "green", "red", "blue", "green", "blue", "yellow" };
            correctColors.Add(correctRedCols[flashNum]);
            colorPositions.Add(flashNum + 1);
            correctColors.Add(correctRedCols[flashNum + 5]);
            colorPositions.Add(0);
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, correctRedCols[flashNum], correctRedCols[flashNum + 5]);
        }
        else if (color.Equals("blue"))
        {
            string[] correctBlueCols = { "yellow", "green", "red", "red", "red", "blue", "yellow", "yellow", "red", "green" };
            correctColors.Add(correctBlueCols[flashNum]);
            colorPositions.Add(flashNum + 1);
            correctColors.Add(correctBlueCols[flashNum + 5]);
            colorPositions.Add(0);
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, correctBlueCols[flashNum], correctBlueCols[flashNum + 5]);
        }
        else if (color.Equals("green"))
        {
            string[] correctGreenCols = { "red", "red", "blue", "blue", "yellow", "green", "red", "red", "green", "blue" };
            correctColors.Add(correctGreenCols[flashNum]);
            colorPositions.Add(flashNum + 1);
            correctColors.Add(correctGreenCols[flashNum + 5]);
            colorPositions.Add(0);
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, correctGreenCols[flashNum], correctGreenCols[flashNum + 5]);
        }
        else if (color.Equals("yellow"))
        {
            string[] correctYellowCols = { "green", "blue", "green", "yellow", "blue", "yellow", "green", "blue", "yellow", "red" };
            correctColors.Add(correctYellowCols[flashNum]);
            colorPositions.Add(flashNum + 1);
            correctColors.Add(correctYellowCols[flashNum + 5]);
            colorPositions.Add(0);
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1} and {2}", moduleId, correctYellowCols[flashNum], correctYellowCols[flashNum + 5]);
        }
    }

    private List<char[]> getCorrectChars(string color)
    {
        List<char[]> temp = new List<char[]>();
        if (color.Equals("red"))
        {
            temp.Add(new char[] { 'F', 'A', 'D', 'H', 'C', 'E' });
            temp.Add(new char[] { 'F', 'H', 'E', 'C', 'A', 'D' });
            temp.Add(new char[] { 'C', 'F', 'D', 'E', 'H', 'A' });
        }
        else if (color.Equals("orange"))
        {
            temp.Add(new char[] { 'C', 'D', 'E', 'A', 'F', 'H' });
            temp.Add(new char[] { 'E', 'F', 'C', 'D', 'H', 'A' });
            temp.Add(new char[] { 'H', 'C', 'F', 'A', 'D', 'E' });
        }
        else if (color.Equals("yellow"))
        {
            temp.Add(new char[] { 'H', 'E', 'F', 'C', 'D', 'A' });
            temp.Add(new char[] { 'A', 'C', 'H', 'F', 'D', 'E' });
            temp.Add(new char[] { 'F', 'H', 'E', 'D', 'A', 'C' });
        }
        else if (color.Equals("green"))
        {
            temp.Add(new char[] { 'E', 'C', 'H', 'D', 'A', 'F' });
            temp.Add(new char[] { 'C', 'D', 'A', 'H', 'E', 'F' });
            temp.Add(new char[] { 'D', 'E', 'A', 'H', 'C', 'F' });
        }
        else if (color.Equals("blue"))
        {
            temp.Add(new char[] { 'D', 'F', 'A', 'E', 'H', 'C' });
            temp.Add(new char[] { 'D', 'E', 'F', 'A', 'C', 'H' });
            temp.Add(new char[] { 'E', 'A', 'H', 'C', 'F', 'D' });
        }
        else if (color.Equals("purple"))
        {
            temp.Add(new char[] { 'A', 'H', 'C', 'F', 'E', 'D' });
            temp.Add(new char[] { 'H', 'A', 'D', 'E', 'F', 'C' });
            temp.Add(new char[] { 'A', 'D', 'C', 'F', 'E', 'H' });
        }
        return temp;
    }

    private void getCorrectScreamsAnswer(string color, int flashNum)
    {
        char needed = 'N';
        List<char[]> neededchars = getCorrectChars(color);
        char[] colset1 = neededchars[0];
        char[] colset2 = neededchars[1];
        char[] colset3 = neededchars[2];
        if (threeAdjColors())
        {
            if (flashNum == 0 || flashNum == 3)
            {
                needed = colset1[0];
            }
            else if (flashNum == 1 || flashNum == 4)
            {
                needed = colset2[0];
            }
            else if (flashNum == 2)
            {
                needed = colset3[0];
            }
        }
        else if (adjOrigAdj())
        {
            if (flashNum == 0 || flashNum == 3)
            {
                needed = colset1[1];
            }
            else if (flashNum == 1 || flashNum == 4)
            {
                needed = colset2[1];
            }
            else if (flashNum == 2)
            {
                needed = colset3[1];
            }
        }
        else if (atMostRedYellowBlue())
        {
            if (flashNum == 0 || flashNum == 3)
            {
                needed = colset1[2];
            }
            else if (flashNum == 1 || flashNum == 4)
            {
                needed = colset2[2];
            }
            else if (flashNum == 2)
            {
                needed = colset3[2];
            }
        }
        else if (oppositeNoFlash())
        {
            if (flashNum == 0 || flashNum == 3)
            {
                needed = colset1[3];
            }
            else if (flashNum == 1 || flashNum == 4)
            {
                needed = colset2[3];
            }
            else if (flashNum == 2)
            {
                needed = colset3[3];
            }
        }
        else if (twoAdjFlashed())
        {
            if (flashNum == 0 || flashNum == 3)
            {
                needed = colset1[4];
            }
            else if (flashNum == 1 || flashNum == 4)
            {
                needed = colset2[4];
            }
            else if (flashNum == 2)
            {
                needed = colset3[4];
            }
        }
        else
        {
            if (flashNum == 0 || flashNum == 3)
            {
                needed = colset1[5];
            }
            else if (flashNum == 1 || flashNum == 4)
            {
                needed = colset2[5];
            }
            else if (flashNum == 2)
            {
                needed = colset3[5];
            }
        }
        char[] sequence = new char[6];
        if (needed.Equals('A'))
        {
            sequence[0] = 'Y';
            sequence[1] = 'P';
            sequence[2] = 'O';
            sequence[3] = 'G';
            sequence[4] = 'R';
            sequence[5] = 'B';
        }
        else if (needed.Equals('C'))
        {
            sequence[0] = 'O';
            sequence[1] = 'Y';
            sequence[2] = 'G';
            sequence[3] = 'B';
            sequence[4] = 'P';
            sequence[5] = 'R';
        }
        else if (needed.Equals('D'))
        {
            sequence[0] = 'G';
            sequence[1] = 'R';
            sequence[2] = 'B';
            sequence[3] = 'O';
            sequence[4] = 'Y';
            sequence[5] = 'P';
        }
        else if (needed.Equals('E'))
        {
            sequence[0] = 'R';
            sequence[1] = 'B';
            sequence[2] = 'P';
            sequence[3] = 'Y';
            sequence[4] = 'O';
            sequence[5] = 'G';
        }
        else if (needed.Equals('F'))
        {
            sequence[0] = 'B';
            sequence[1] = 'O';
            sequence[2] = 'R';
            sequence[3] = 'P';
            sequence[4] = 'G';
            sequence[5] = 'Y';
        }
        else if (needed.Equals('H'))
        {
            sequence[0] = 'P';
            sequence[1] = 'G';
            sequence[2] = 'Y';
            sequence[3] = 'R';
            sequence[4] = 'B';
            sequence[5] = 'O';
        }
        string colors = "";
        bool firstFound = false;
        if (bomb.GetIndicators().Count() >= 3)
        {
            correctColors.Add(charToColor(sequence[0]));
            colors += charToColor(sequence[0]) + " ";
            colorPositions.Add(flashNum + 1);
            firstFound = true;
        }
        if (bomb.GetPortCount() >= 3)
        {
            correctColors.Add(charToColor(sequence[1]));
            colors += charToColor(sequence[1]) + " ";
            if (!firstFound)
            {
                colorPositions.Add(flashNum + 1);
                firstFound = true;
            }
            else
                colorPositions.Add(0);
        }
        if (bomb.GetSerialNumberNumbers().Count() >= 3)
        {
            correctColors.Add(charToColor(sequence[2]));
            colors += charToColor(sequence[2]) + " ";
            if (!firstFound)
            {
                colorPositions.Add(flashNum + 1);
                firstFound = true;
            }
            else
                colorPositions.Add(0);
        }
        if (bomb.GetSerialNumberLetters().Count() >= 3)
        {
            correctColors.Add(charToColor(sequence[3]));
            colors += charToColor(sequence[3]) + " ";
            if (!firstFound)
            {
                colorPositions.Add(flashNum + 1);
            }
            else
                colorPositions.Add(0);
        }
        if (bomb.GetBatteryCount() >= 3)
        {
            correctColors.Add(charToColor(sequence[4]));
            colors += charToColor(sequence[4]) + " ";
            colorPositions.Add(0);
        }
        if (bomb.GetBatteryHolderCount() >= 3)
        {
            correctColors.Add(charToColor(sequence[5]));
            colors += charToColor(sequence[5]) + " ";
            colorPositions.Add(0);
        }
        colors = colors.Trim();
        colors = colors.Replace(" ", ", ");
        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}", moduleId, colors);
    }

    private void getCorrectStopsAnswer(string color, int pos, int flashNum)
    {
        if (color.Equals("red"))
        {
            if(stage == 1)
            {
                correctColors.Add("blue");
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] blue", moduleId);
            }
            else if (stage == 2)
            {
                correctColors.Add(getBatteryClockwiseColor(pos));
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}", moduleId, getBatteryClockwiseColor(pos));
            }
            else if (stage == 3)
            {
                correctColors.Add("yellow");
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] yellow", moduleId);
            }
        }
        else if (color.Equals("orange"))
        {
            if (stage == 1)
            {
                correctColors.Add(getBatteryClockwiseColor(pos));
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}", moduleId, getBatteryClockwiseColor(pos));
            }
            else if (stage == 2)
            {
                correctColors.Add("yellow");
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] yellow", moduleId);
            }
            else if (stage == 3)
            {
                correctColors.Add("orange");
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] orange", moduleId);
            }
        }
        else if (color.Equals("yellow"))
        {
            if (stage == 1)
            {
                correctColors.Add("yellow");
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] yellow", moduleId);
            }
            else if (stage == 2)
            {
                correctColors.Add(getBatteryClockwiseColor(pos));
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}", moduleId, getBatteryClockwiseColor(pos));
            }
            else if (stage == 3)
            {
                correctColors.Add("green");
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] green", moduleId);
            }
        }
        else if (color.Equals("green"))
        {
            if (stage == 1)
            {
                correctColors.Add("red");
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] red", moduleId);
            }
            else if (stage == 2)
            {
                correctColors.Add("purple");
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] purple", moduleId);
            }
            else if (stage == 3)
            {
                correctColors.Add(getBatteryClockwiseColor(pos));
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}", moduleId, getBatteryClockwiseColor(pos));
            }
        }
        else if (color.Equals("blue"))
        {
            if (stage == 1)
            {
                correctColors.Add("purple");
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] purple", moduleId);
            }
            else if (stage == 2)
            {
                correctColors.Add("orange");
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] orange", moduleId);
            }
            else if (stage == 3)
            {
                correctColors.Add(getBatteryClockwiseColor(pos));
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}", moduleId, getBatteryClockwiseColor(pos));
            }
        }
        else if (color.Equals("purple"))
        {
            if (stage == 1)
            {
                correctColors.Add(getBatteryClockwiseColor(pos));
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}", moduleId, getBatteryClockwiseColor(pos));
            }
            else if (stage == 2)
            {
                correctColors.Add("blue");
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] blue", moduleId);
            }
            else if (stage == 3)
            {
                correctColors.Add("red");
                colorPositions.Add(flashNum + 1);
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] red", moduleId);
            }
        }
    }

    private void getCorrectTashaAnswer(string color, int pos, int flashNum)
    {
        string correct = "";
        int neededpos = -1;
        string[] actualleftcols;
        string[] neededs1;
        string[] neededs2;
        string[] neededs3;
        string[] leftcols1 = { "green", "yellow", "blue" };
        string[] leftcols2 = { "yellow", "blue", "green" };
        string[] leftcols3 = { "blue", "green", "yellow" };
        string[] leftcols4 = { "green", "blue", "yellow" };
        if (((colorsOfButtons[0].Equals("pink") || colorsOfButtons[1].Equals("pink")) && !(flashNum == 1 || flashNum == 4)) || (!(colorsOfButtons[0].Equals("pink") || colorsOfButtons[1].Equals("pink")) && (flashNum == 1 || flashNum == 4)))
        {
            string[] neededs1temp = { "yellow", "pink", "blue" };
            string[] neededs2temp = { "green", "yellow", "green" };
            string[] neededs3temp = { "blue", "green", "pink" };
            neededs1 = neededs1temp;
            neededs2 = neededs2temp;
            neededs3 = neededs1temp;
            actualleftcols = leftcols1;
            if (pos == 0 || pos == 1)
            {
                neededpos = 0;
            }
            else if (pos == 5)
            {
                neededpos = 1;
            }
            else if (pos == 2)
            {
                neededpos = 2;
            }
            else
            {
                correct = "pink";
            }
        }
        else if ((colorsOfButtons[5].Equals("pink") && !(flashNum == 3)) || (!colorsOfButtons[5].Equals("pink") && (flashNum == 3)))
        {
            string[] neededs1temp = { "blue", "yellow", "pink" };
            string[] neededs2temp = { "pink", "green", "yellow" };
            string[] neededs3temp = { "yellow", "blue", "green" };
            neededs1 = neededs1temp;
            neededs2 = neededs2temp;
            neededs3 = neededs1temp;
            actualleftcols = leftcols2;
            if (pos == 0 || pos == 1)
            {
                neededpos = 0;
            }
            else if (pos == 2)
            {
                neededpos = 1;
            }
            else if (pos == 3 || pos == 4)
            {
                neededpos = 2;
            }
            else
            {
                correct = "pink";
            }
        }
        else if (((bomb.GetBatteryCount() % 2 == 0) && !(flashNum == 1 || flashNum == 2 || flashNum == 4)) || (!(bomb.GetBatteryCount() % 2 == 0) && (flashNum == 1 || flashNum == 2 || flashNum == 4)))
        {
            string[] neededs1temp = { "yellow", "blue", "pink" };
            string[] neededs2temp = { "green", "yellow", "green" };
            string[] neededs3temp = { "pink", "pink", "blue" };
            neededs1 = neededs1temp;
            neededs2 = neededs2temp;
            neededs3 = neededs1temp;
            actualleftcols = leftcols3;
            if (pos == 5)
            {
                neededpos = 0;
            }
            else if (pos == 3 || pos == 4)
            {
                neededpos = 1;
            }
            else if (pos == 0 || pos == 1)
            {
                neededpos = 2;
            }
            else
            {
                correct = "pink";
            }
        }
        else
        {
            string[] neededs1temp = { "pink", "pink", "green" };
            string[] neededs2temp = { "green", "blue", "yellow" };
            string[] neededs3temp = { "yellow", "green", "blue" };
            neededs1 = neededs1temp;
            neededs2 = neededs2temp;
            neededs3 = neededs1temp;
            actualleftcols = leftcols4;
            if (pos == 2)
            {
                neededpos = 0;
            }
            else if (pos == 5)
            {
                neededpos = 1;
            }
            else if (pos == 3 || pos == 4)
            {
                neededpos = 2;
            }
            else
            {
                correct = "pink";
            }
        }
        if (neededpos != -1)
        {
            if (color.Equals(neededs1[neededpos]))
            {
                correct = actualleftcols[0];
            }
            else if (color.Equals(neededs2[neededpos]))
            {
                correct = actualleftcols[1];
            }
            else if (color.Equals(neededs3[neededpos]))
            {
                correct = actualleftcols[2];
            }
            else
            {
                correct = "pink";
            }
        }
        correctColors.Add(correct);
        colorPositions.Add(flashNum + 1);
        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}", moduleId, correct);
    }

    private void getCorrectSimonsAnswer(string color, int pos, int flashNum, bool modify)
    {
        string[] row1 = { "green", "yellow", "green", "red", "green", "red", "blue", "blue", "green", "red", "blue", "yellow", "blue", "yellow", "red", "yellow" };
        string[] row2 = { "yellow", "blue", "red", "green", "blue", "red", "green", "yellow", "red", "red", "yellow", "green", "green", "blue", "blue", "yellow" };
        string[] row3 = { "green", "red", "green", "red", "yellow", "yellow", "blue", "green", "red", "blue", "blue", "red", "yellow", "yellow", "blue", "green" };
        int index = 0;
        if(pos == 2)
        {
            index += 4;
        }
        else if (pos == 5)
        {
            index += 8;
        }
        else if (pos == 3 || pos == 4)
        {
            index += 12;
        }
        if (color.Equals("yellow"))
        {
            index += 1;
        }
        else if (color.Equals("red"))
        {
            index += 2;
        }
        else if (color.Equals("green"))
        {
            index += 3;
        }
        if(currentStrikes == 0)
        {
            if (modify)
            {
                correctColors.Add(row1[index]);
                colorPositions.Add(flashNum + 1);
            }
            else
            {
                correctColors[colorPositions.IndexOf(flashNum + 1)] = row1[index];
            }
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}", moduleId, row1[index]);
        }
        else if (currentStrikes == 1)
        {
            if (modify)
            {
                correctColors.Add(row2[index]);
                colorPositions.Add(flashNum + 1);
            }
            else
            {
                correctColors[colorPositions.IndexOf(flashNum + 1)] = row2[index];
            }
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}", moduleId, row2[index]);
        }
        else if (currentStrikes == 2)
        {
            if (modify)
            {
                correctColors.Add(row3[index]);
                colorPositions.Add(flashNum + 1);
            }
            else
            {
                correctColors[colorPositions.IndexOf(flashNum + 1)] = row3[index];
            }
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] {1}", moduleId, row3[index]);
        }
    }

    private string getStopsCtrlInput()
    {
        string keyword = "";
        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Total Flashes - 2 = Stage # (Table B Column) | {1} - 2 = Stage {2} (Table B Column)", moduleId, stage+2, stage);
        if (stage == 1)
        {
            string[] stage1keywords = { "SC", "1N", "PS", "1P", "2N", "OC", "NS", "2P", "PP", "NP" };
            int sum = ((bomb.GetBatteryCount() * serialConsonants()) + bomb.GetSerialNumberNumbers().ElementAt(bomb.GetSerialNumberNumbers().Count() - 1)) % 10;
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] ((Battery Count * Serial Number Consonants) + Last Digit of Serial Number) % 10 = Sum % 10 (Table B Row) | (({1} * {2}) + {3}) % 10 = {4} (Table B Row)", moduleId, bomb.GetBatteryCount(), serialConsonants(), bomb.GetSerialNumberNumbers().ElementAt(bomb.GetSerialNumberNumbers().Count() - 1), sum);
            keyword = stage1keywords[sum];
        }
        else if (stage == 2)
        {
            string[] stage2keywords = { "1P", "NP", "PP", "SC", "OC", "PS", "2P", "1N", "NS", "2N" };
            int sum = ((bomb.GetPortCount() * 2) + bomb.GetBatteryHolderCount() + bomb.GetSerialNumberNumbers().ElementAt(bomb.GetSerialNumberNumbers().Count() - 1)) % 10;
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] ((Port Count * 2) + Battery Holder Count + Last Digit of Serial Number) % 10 = Sum % 10 (Table B Row) | (({1} * 2) + {2} + {3}) % 10 = {4} (Table B Row)", moduleId, bomb.GetPortCount(), bomb.GetBatteryHolderCount(), bomb.GetSerialNumberNumbers().ElementAt(bomb.GetSerialNumberNumbers().Count() - 1), sum);
            keyword = stage2keywords[sum];
        }
        else if (stage == 3)
        {
            string[] stage3keywords = { "OC", "1N", "1P", "NS", "2P", "PP", "PS", "SC", "NP", "2N" };
            int sum = (2 + bomb.GetOffIndicators().Count() + (bomb.GetOnIndicators().Count() * 3) + bomb.GetSerialNumberNumbers().ElementAt(bomb.GetSerialNumberNumbers().Count() - 1)) % 10;
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] (2 + Unlit Indicators + (Lit Indicators * 3) + Last Digit of Serial Number) % 10 = Sum % 10 (Table B Row) | (2 + {1} + ({2} * 3) + {3}) % 10 = {4} (Table B Row)", moduleId, bomb.GetOffIndicators().Count(), bomb.GetOnIndicators().Count(), bomb.GetSerialNumberNumbers().ElementAt(bomb.GetSerialNumberNumbers().Count() - 1), sum);
            keyword = stage3keywords[sum];
        }
        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Two Character Code Rule from Table B: {1}", moduleId, keyword);
        if (keyword.Equals("SC"))
        {
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] SC is same color as previously pressed, which is: {1}", moduleId, colorsOfButtons[posOfLastPress]);
            return colorsOfButtons[posOfLastPress];
        }
        else if (keyword.Equals("OC"))
        {
            int newpos = posOfLastPress;
            for(int i = 0; i < 3; i++)
            {
                newpos++;
                if(newpos == 6)
                {
                    newpos = 0;
                }
            }
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] OC is opposite color to previously pressed (3 clockwise), which makes the Control Input: {1}", moduleId, colorsOfButtons[newpos]);
            return colorsOfButtons[newpos];
        }
        else if (keyword.Equals("1N"))
        {
            int newpos = posOfLastPress + 1;
            if(newpos == 6)
            {
                newpos = 0;
            }
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] 1N is 1 color clockwise to previously pressed, which makes the Control Input: {1}", moduleId, colorsOfButtons[newpos]);
            return colorsOfButtons[newpos];
        }
        else if (keyword.Equals("1P"))
        {
            int newpos = posOfLastPress - 1;
            if (newpos == -1)
            {
                newpos = 5;
            }
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] 1P is 1 color counter-clockwise to previously pressed, which makes the Control Input: {1}", moduleId, colorsOfButtons[newpos]);
            return colorsOfButtons[newpos];
        }
        else if (keyword.Equals("2N"))
        {
            int newpos = posOfLastPress + 2;
            if (newpos == 6)
            {
                newpos = 0;
            }
            else if (newpos == 7)
            {
                newpos = 1;
            }
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] 2N is 2 colors clockwise to previously pressed, which makes the Control Input: {1}", moduleId, colorsOfButtons[newpos]);
            return colorsOfButtons[newpos];
        }
        else if (keyword.Equals("2P"))
        {
            int newpos = posOfLastPress - 2;
            if (newpos == -1)
            {
                newpos = 5;
            }
            else if (newpos == -2)
            {
                newpos = 4;
            }
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] 2P is 2 colors counter-clockwise to previously pressed, which makes the Control Input: {1}", moduleId, colorsOfButtons[newpos]);
            return colorsOfButtons[newpos];
        }
        else if (keyword.Equals("NP"))
        {
            if(colorsOfButtons.Contains("blue") || colorsOfButtons.Contains("red") || colorsOfButtons.Contains("yellow"))
            {
                int newpos = posOfLastPress;
                for(int i = 0; i < 6; i++)
                {
                    newpos++;
                    if(newpos == 6)
                    {
                        newpos = 0;
                    }
                    if(colorsOfButtons[newpos].Equals("blue") || colorsOfButtons[newpos].Equals("red") || colorsOfButtons[newpos].Equals("yellow"))
                    {
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] NP is the next primary color in clockwise order from previously pressed, which makes the Control Input: {1}", moduleId, colorsOfButtons[newpos]);
                        return colorsOfButtons[newpos];
                    }
                }
            }
            else
            {
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] No primary colors are present for NP so use SC instead, which makes the Control Input: {1}", moduleId, colorsOfButtons[posOfLastPress]);
                return colorsOfButtons[posOfLastPress];
            }
        }
        else if (keyword.Equals("PP"))
        {
            if (colorsOfButtons.Contains("blue") || colorsOfButtons.Contains("red") || colorsOfButtons.Contains("yellow"))
            {
                int newpos = posOfLastPress;
                for (int i = 0; i < 6; i++)
                {
                    newpos--;
                    if (newpos == -1)
                    {
                        newpos = 5;
                    }
                    if (colorsOfButtons[newpos].Equals("blue") || colorsOfButtons[newpos].Equals("red") || colorsOfButtons[newpos].Equals("yellow"))
                    {
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] PP is the next primary color in counter-clockwise order from previously pressed, which makes the Control Input: {1}", moduleId, colorsOfButtons[newpos]);
                        return colorsOfButtons[newpos];
                    }
                }
            }
            else
            {
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] No primary colors are present for PP so use SC instead, which makes the Control Input: {1}", moduleId, colorsOfButtons[posOfLastPress]);
                return colorsOfButtons[posOfLastPress];
            }
        }
        else if (keyword.Equals("NS"))
        {
            if (colorsOfButtons.Contains("orange") || colorsOfButtons.Contains("green") || colorsOfButtons.Contains("purple"))
            {
                int newpos = posOfLastPress;
                for (int i = 0; i < 6; i++)
                {
                    newpos++;
                    if (newpos == 6)
                    {
                        newpos = 0;
                    }
                    if (colorsOfButtons[newpos].Equals("orange") || colorsOfButtons[newpos].Equals("green") || colorsOfButtons[newpos].Equals("purple"))
                    {
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] NS is the next secondary color in clockwise order from previously pressed, which makes the Control Input: {1}", moduleId, colorsOfButtons[newpos]);
                        return colorsOfButtons[newpos];
                    }
                }
            }
            else
            {
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] No secondary colors are present for NS so use SC instead, which makes the Control Input: {1}", moduleId, colorsOfButtons[posOfLastPress]);
                return colorsOfButtons[posOfLastPress];
            }
        }
        else if (keyword.Equals("PS"))
        {
            if (colorsOfButtons.Contains("orange") || colorsOfButtons.Contains("green") || colorsOfButtons.Contains("purple"))
            {
                int newpos = posOfLastPress;
                for (int i = 0; i < 6; i++)
                {
                    newpos--;
                    if (newpos == -1)
                    {
                        newpos = 5;
                    }
                    if (colorsOfButtons[newpos].Equals("orange") || colorsOfButtons[newpos].Equals("green") || colorsOfButtons[newpos].Equals("purple"))
                    {
                        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] PS is the next secondary color in counter-clockwise order from previously pressed, which makes the Control Input: {1}", moduleId, colorsOfButtons[newpos]);
                        return colorsOfButtons[newpos];
                    }
                }
            }
            else
            {
                Debug.LogFormat("[Simon's Ultimate Showdown #{0}] No secondary colors are present for PS so use SC instead, which makes the Control Input: {1}", moduleId, colorsOfButtons[posOfLastPress]);
                return colorsOfButtons[posOfLastPress];
            }
        }
        //so mod doesnt crash if an error occurs
        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Error! Had trouble calculating the correct Control Input so the color from the priority list will be used: {1}", moduleId, getPriorityListColor());
        return getPriorityListColor();
    }

    private int serialConsonants()
    {
        char[] vowels = { 'A', 'E', 'I', 'O', 'U' };
        int total = 0;
        for(int i = 0; i < bomb.GetSerialNumberLetters().Count(); i++)
        {
            if (!vowels.Contains(bomb.GetSerialNumberLetters().ElementAt(i)))
            {
                total++;
            }
        }
        return total;
    }

    private string getBatteryClockwiseColor(int pos)
    {
        for(int i = 0; i < bomb.GetBatteryCount(); i++)
        {
            pos++;
            if(pos == 6)
            {
                pos = 0;
            }
        }
        return colorsOfButtons[pos];
    }

    private bool threeAdjColors()
    {
        string buttonSeq = "";
        for(int i = 0; i < stage+2; i++)
        {
            buttonSeq += buttonsFlashing[i];
        }
        if (buttonSeq.Contains("012") || buttonSeq.Contains("123") || buttonSeq.Contains("234") || buttonSeq.Contains("345") || buttonSeq.Contains("450") || buttonSeq.Contains("501"))
        {
            return true;
        }
        return false;
    }

    private bool adjOrigAdj()
    {
        string buttonSeq = "";
        for (int i = 0; i < stage+2; i++)
        {
            buttonSeq += buttonsFlashing[i];
        }
        if (buttonSeq.Contains("010") || buttonSeq.Contains("050") || buttonSeq.Contains("101") || buttonSeq.Contains("121") || buttonSeq.Contains("232") || buttonSeq.Contains("212") || buttonSeq.Contains("323") || buttonSeq.Contains("343") || buttonSeq.Contains("434") || buttonSeq.Contains("454") || buttonSeq.Contains("545") || buttonSeq.Contains("505"))
        {
            return true;
        }
        return false;
    }

    private bool atMostRedYellowBlue()
    {
        bool blue = false;
        bool red = false;
        bool yellow = false;
        List<string> colors = new List<string>();
        for(int i = 0; i < stage+2; i++)
        {
            colors.Add(colorsOfButtons[buttonsFlashing[i]]);
        }
        if (colors.Contains("blue"))
            blue = true;
        if (colors.Contains("red"))
            red = true;
        if (colors.Contains("yellow"))
            yellow = true;
        if (blue && yellow && !red)
        {
            return false;
        }
        else if (!blue && yellow && red)
        {
            return false;
        }
        else if (blue && !yellow && red)
        {
            return false;
        }
        else if (blue && yellow && red)
        {
            return false;
        }
        return true;
    }

    private bool oppositeNoFlash()
    {
        if ((!buttonsFlashing.Contains(0) && !buttonsFlashing.Contains(3)) || (!buttonsFlashing.Contains(1) && !buttonsFlashing.Contains(4)) || (!buttonsFlashing.Contains(2) && !buttonsFlashing.Contains(5)))
        {
            return true;
        }
        return false;
    }

    private bool twoAdjFlashed()
    {
        string buttonSeq = "";
        for (int i = 0; i < buttonsFlashing.Length; i++)
        {
            buttonSeq += buttonsFlashing[i];
        }
        if (buttonSeq.Contains("01") || buttonSeq.Contains("12") || buttonSeq.Contains("23") || buttonSeq.Contains("34") || buttonSeq.Contains("45") || buttonSeq.Contains("50"))
        {
            return true;
        }
        return false;
    }

    private void setStagesColor(string color, TextMesh text, Light light, GameObject coloredBase)
    {
        if (color.Equals("red"))
        {
            text.text = "R";
            light.color = stagesColorMats[0].color;
            coloredBase.GetComponent<Renderer>().material = stagesColorMats[0];
        }
        else if (color.Equals("blue"))
        {
            text.text = "B";
            light.color = stagesColorMats[1].color;
            coloredBase.GetComponent<Renderer>().material = stagesColorMats[1];
        }
        else if (color.Equals("yellow"))
        {
            text.text = "Y";
            light.color = stagesColorMats[2].color;
            coloredBase.GetComponent<Renderer>().material = stagesColorMats[2];
        }
        else if (color.Equals("green"))
        {
            text.text = "G";
            light.color = stagesColorMats[3].color;
            coloredBase.GetComponent<Renderer>().material = stagesColorMats[3];
        }
        else if (color.Equals("orange"))
        {
            text.text = "O";
            light.color = stagesColorMats[4].color;
            coloredBase.GetComponent<Renderer>().material = stagesColorMats[4];
        }
        else if (color.Equals("pink"))
        {
            text.text = "P";
            light.color = stagesColorMats[5].color;
            coloredBase.GetComponent<Renderer>().material = stagesColorMats[5];
        }
        else if (color.Equals("magenta"))
        {
            text.text = "M";
            light.color = stagesColorMats[6].color;
            coloredBase.GetComponent<Renderer>().material = stagesColorMats[6];
        }
        else if (color.Equals("lime"))
        {
            text.text = "L";
            light.color = stagesColorMats[7].color;
            coloredBase.GetComponent<Renderer>().material = stagesColorMats[7];
        }
        else if (color.Equals("cyan"))
        {
            text.text = "C";
            light.color = stagesColorMats[8].color;
            coloredBase.GetComponent<Renderer>().material = stagesColorMats[8];
        }
        else if (color.Equals("white"))
        {
            text.text = "W";
            light.color = stagesColorMats[9].color;
            coloredBase.GetComponent<Renderer>().material = stagesColorMats[9];
        }
    }

    private void setScramblesColor(string color, Light light, GameObject obj)
    {
        if (color.Equals("red"))
        {
            light.color = scramblesColorMats[0].color;
            obj.GetComponent<Renderer>().material = scramblesColorMats[0];
        }
        else if (color.Equals("blue"))
        {
            light.color = scramblesColorMats[1].color;
            obj.GetComponent<Renderer>().material = scramblesColorMats[1];
        }
        else if (color.Equals("yellow"))
        {
            light.color = scramblesColorMats[2].color;
            obj.GetComponent<Renderer>().material = scramblesColorMats[2];
        }
        else if (color.Equals("green"))
        {
            light.color = scramblesColorMats[3].color;
            obj.GetComponent<Renderer>().material = scramblesColorMats[3];
        }
    }

    private void setScreamsColor(string color, Light light, GameObject obj)
    {
        if (color.Equals("red"))
        {
            light.color = screamsColorMats[0].color;
            obj.GetComponent<Renderer>().material = screamsColorMats[0];
        }
        else if (color.Equals("blue"))
        {
            light.color = screamsColorMats[1].color;
            obj.GetComponent<Renderer>().material = screamsColorMats[1];
        }
        else if (color.Equals("yellow"))
        {
            light.color = screamsColorMats[2].color;
            obj.GetComponent<Renderer>().material = screamsColorMats[2];
        }
        else if (color.Equals("green"))
        {
            light.color = screamsColorMats[3].color;
            obj.GetComponent<Renderer>().material = screamsColorMats[3];
        }
        else if (color.Equals("orange"))
        {
            light.color = screamsColorMats[4].color;
            obj.GetComponent<Renderer>().material = screamsColorMats[4];
        }
        else if (color.Equals("purple"))
        {
            light.color = screamsColorMats[5].color;
            obj.GetComponent<Renderer>().material = screamsColorMats[5];
        }
    }

    private void setStopsColor(string color, Light light, GameObject obj)
    {
        if (color.Equals("red"))
        {
            light.color = stopsColorMats[0].color;
            obj.GetComponent<Renderer>().material = stopsColorMats[0];
        }
        else if (color.Equals("blue"))
        {
            light.color = stopsColorMats[1].color;
            obj.GetComponent<Renderer>().material = stopsColorMats[1];
        }
        else if (color.Equals("yellow"))
        {
            light.color = stopsColorMats[2].color;
            obj.GetComponent<Renderer>().material = stopsColorMats[2];
        }
        else if (color.Equals("green"))
        {
            light.color = stopsColorMats[3].color;
            obj.GetComponent<Renderer>().material = stopsColorMats[3];
        }
        else if (color.Equals("orange"))
        {
            light.color = stopsColorMats[4].color;
            obj.GetComponent<Renderer>().material = stopsColorMats[4];
        }
        else if (color.Equals("purple"))
        {
            light.color = stopsColorMats[5].color;
            obj.GetComponent<Renderer>().material = stopsColorMats[5];
        }
    }

    private void setTashaColor(string color, Light light, KMSelectable obj)
    {
        if (color.Equals("pink"))
        {
            light.color = tashaColors[0];
            obj.gameObject.GetComponent<Renderer>().material.color = tashaColors[0];
        }
        else if (color.Equals("green"))
        {
            light.color = tashaColors[1];
            obj.gameObject.GetComponent<Renderer>().material.color = tashaColors[1];
        }
        else if (color.Equals("yellow"))
        {
            light.color = tashaColors[2];
            obj.gameObject.GetComponent<Renderer>().material.color = tashaColors[2];
        }
        else if (color.Equals("blue"))
        {
            light.color = tashaColors[3];
            obj.gameObject.GetComponent<Renderer>().material.color = tashaColors[3];
        }
    }

    private void setSimonsColor(string color, KMSelectable obj)
    {
        if (color.Equals("red"))
        {
            obj.gameObject.GetComponent<Renderer>().material = simonsColorMats[0];
        }
        else if (color.Equals("blue"))
        {
            obj.gameObject.GetComponent<Renderer>().material = simonsColorMats[1];
        }
        else if (color.Equals("yellow"))
        {
            obj.gameObject.GetComponent<Renderer>().material = simonsColorMats[2];
        }
        else if (color.Equals("green"))
        {
            obj.gameObject.GetComponent<Renderer>().material = simonsColorMats[3];
        }
    }

    private bool stagesSoundsEqual(int j)
    {
        for(int i = 0; i < 6; i++)
        {
            if (i != j)
            {
                if (stagesRandomPressSounds[i] == stagesRandomPressSounds[j])
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool screamsSoundsEqual(int j)
    {
        for (int i = 0; i < 6; i++)
        {
            if (i != j)
            {
                if (screamsRandomPressSounds[i] == screamsRandomPressSounds[j])
                {
                    return true;
                }
                if (screamsRandomFlashSounds[i] == screamsRandomFlashSounds[j])
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool stopsSoundsEqual(int j)
    {
        for (int i = 0; i < 6; i++)
        {
            if (i != j)
            {
                if (stopsRandomPressSounds[i] == stopsRandomPressSounds[j])
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool tashaSoundsEqual()
    {
        if (!tashaRandomPressSounds.Contains(1) && !tashaRandomPressSounds.Contains(2) && !tashaRandomPressSounds.Contains(3) && !tashaRandomPressSounds.Contains(4))
            return true;
        return false;
    }

    private bool simonsSoundsEqual()
    {
        if (!simonsRandomPressSounds.Contains(1) && !simonsRandomPressSounds.Contains(2) && !simonsRandomPressSounds.Contains(3) && !simonsRandomPressSounds.Contains(4))
            return true;
        return false;
    }

    private string colorOppositeButton(int but)
    {
        for(int i = 0; i < 3; i++)
        {
            but--;
            if(but == -1)
            {
                but = 5;
            }
        }
        return colorsOfButtons[but];
    }

    private List<string> colorsNeededForFlash(int flash)
    {
        List<string> temp = new List<string>();
        if(flash == (stage + 2))
        {
            for (int i = colorPositionsModified.IndexOf(flash); i < colorPositionsModified.Count; i++)
            {
                temp.Add(correctColorsModified[i]);
            }
        }
        else
        {
            for (int i = colorPositionsModified.IndexOf(flash); i < colorPositionsModified.IndexOf(flash+1); i++)
            {
                temp.Add(correctColorsModified[i]);
            }
        }
        return temp;
    }

    private bool halfOfFlashesWerePrimary()
    {
        int sum = 0;
        for(int i = 0; i < colorFlashes.Length; i++)
        {
            if(colorFlashes[i].Equals("red") || colorFlashes[i].Equals("yellow") || colorFlashes[i].Equals("blue"))
            {
                sum++;
            }
        }
        if(sum == 2)
        {
            return true;
        }
        return false;
    }

    private int sumOfSimonMods()
    {
        int sum = 0;
        for(int i = 0; i < bomb.GetModuleNames().Count; i++)
        {
            if(bomb.GetModuleNames().ElementAt(i).Equals("Simon's Stages") || bomb.GetModuleNames().ElementAt(i).Equals("Simon Stages") || bomb.GetModuleNames().ElementAt(i).Equals("Simon Scrambles") || bomb.GetModuleNames().ElementAt(i).Equals("Simon Screams") || bomb.GetModuleNames().ElementAt(i).Equals("Simon Stops") || bomb.GetModuleNames().ElementAt(i).Equals("Tasha Squeals") || bomb.GetModuleNames().ElementAt(i).Equals("Simon Simons"))
            {
                sum++;
            }
        }
        return sum;
    }

    private string getPriorityListColor()
    {
        bool scram = false;
        bool tasha = false;
        for(int i = 0; i < modulesOfButtons.Length; i++)
        {
            if(modulesOfButtons[i] == 1)
            {
                scram = true;
            }else if (modulesOfButtons[i] == 5)
            {
                tasha = true;
            }
        }
        if(tasha && scram)
        {
            usedPriorityList = 1;
            string[] list1 = { "lime", "white", "orange", "cyan", "green", "pink", "purple", "yellow", "magenta", "red", "blue" };
            for(int i = 0; i < list1.Length; i++)
            {
                if (colorsOfButtons.Contains(list1[i]))
                {
                    return list1[i];
                }
            }
        }
        else if(bomb.GetModuleNames().Contains("Simon Sends") || bomb.GetModuleNames().Contains("Simon's On First"))
        {
            usedPriorityList = 2;
            string[] list2 = { "orange", "pink", "white", "yellow", "blue", "red", "cyan", "purple", "green", "lime", "magenta" };
            for (int i = 0; i < list2.Length; i++)
            {
                if (colorsOfButtons.Contains(list2[i]))
                {
                    return list2[i];
                }
            }
        }
        else
        {
            usedPriorityList = 3;
            string[] list3 = { "magenta", "purple", "pink", "green", "lime", "white", "red", "blue", "cyan", "yellow", "orange" };
            for (int i = 0; i < list3.Length; i++)
            {
                if (colorsOfButtons.Contains(list3[i]))
                {
                    return list3[i];
                }
            }
        }
        return "";
    }

    private void ButtonsDown()
    {
        stagesButtonsObj[0].transform.localPosition = stagesButtonsObj[0].transform.localPosition + Vector3.up * -0.012f;
        stagesButtonsObj[1].transform.localPosition = stagesButtonsObj[1].transform.localPosition + Vector3.up * -0.012f;
        stagesButtonsObj[2].transform.localPosition = stagesButtonsObj[2].transform.localPosition + Vector3.up * -0.012f;
        stagesButtonsObj[3].transform.localPosition = stagesButtonsObj[3].transform.localPosition + Vector3.up * -0.012f;
        stagesButtonsObj[4].transform.localPosition = stagesButtonsObj[4].transform.localPosition + Vector3.up * -0.012f;
        stagesButtonsObj[5].transform.localPosition = stagesButtonsObj[5].transform.localPosition + Vector3.up * -0.012f;
        scramblesButtonsObj[0].transform.localPosition = scramblesButtonsObj[0].transform.localPosition + Vector3.up * -0.03f;
        scramblesButtonsObj[1].transform.localPosition = scramblesButtonsObj[1].transform.localPosition + Vector3.up * -0.03f;
        scramblesButtonsObj[2].transform.localPosition = scramblesButtonsObj[2].transform.localPosition + Vector3.up * -0.03f;
        scramblesButtonsObj[3].transform.localPosition = scramblesButtonsObj[3].transform.localPosition + Vector3.up * -0.03f;
        scramblesButtonsObj[4].transform.localPosition = scramblesButtonsObj[4].transform.localPosition + Vector3.up * -0.03f;
        scramblesButtonsObj[5].transform.localPosition = scramblesButtonsObj[5].transform.localPosition + Vector3.up * -0.03f;
        screamsButtonsObj[0].transform.localPosition = screamsButtonsObj[0].transform.localPosition + Vector3.up * -0.2f;
        screamsButtonsObj[1].transform.localPosition = screamsButtonsObj[1].transform.localPosition + Vector3.up * -0.2f;
        screamsButtonsObj[2].transform.localPosition = screamsButtonsObj[2].transform.localPosition + Vector3.up * -0.2f;
        screamsButtonsObj[3].transform.localPosition = screamsButtonsObj[3].transform.localPosition + Vector3.up * -0.2f;
        screamsButtonsObj[4].transform.localPosition = screamsButtonsObj[4].transform.localPosition + Vector3.up * -0.2f;
        screamsButtonsObj[5].transform.localPosition = screamsButtonsObj[5].transform.localPosition + Vector3.up * -0.2f;
        stopsButtonsObj[0].transform.localPosition = stopsButtonsObj[0].transform.localPosition + Vector3.up * -0.01f;
        stopsButtonsObj[1].transform.localPosition = stopsButtonsObj[1].transform.localPosition + Vector3.up * -0.01f;
        stopsButtonsObj[2].transform.localPosition = stopsButtonsObj[2].transform.localPosition + Vector3.up * -0.01f;
        stopsButtonsObj[3].transform.localPosition = stopsButtonsObj[3].transform.localPosition + Vector3.up * -0.01f;
        stopsButtonsObj[4].transform.localPosition = stopsButtonsObj[4].transform.localPosition + Vector3.up * -0.01f;
        stopsButtonsObj[5].transform.localPosition = stopsButtonsObj[5].transform.localPosition + Vector3.up * -0.01f;
        tashaButtonsObj[0].transform.localPosition = tashaButtonsObj[0].transform.localPosition + Vector3.up * -0.01f;
        tashaButtonsObj[1].transform.localPosition = tashaButtonsObj[1].transform.localPosition + Vector3.up * -0.01f;
        tashaButtonsObj[2].transform.localPosition = tashaButtonsObj[2].transform.localPosition + Vector3.up * -0.01f;
        tashaButtonsObj[3].transform.localPosition = tashaButtonsObj[3].transform.localPosition + Vector3.up * -0.01f;
        tashaButtonsObj[4].transform.localPosition = tashaButtonsObj[4].transform.localPosition + Vector3.up * -0.01f;
        tashaButtonsObj[5].transform.localPosition = tashaButtonsObj[5].transform.localPosition + Vector3.up * -0.01f;
        simonsButtonsObj[0].transform.localPosition = simonsButtonsObj[0].transform.localPosition + Vector3.up * -0.014f;
        simonsButtonsObj[1].transform.localPosition = simonsButtonsObj[1].transform.localPosition + Vector3.up * -0.014f;
        simonsButtonsObj[2].transform.localPosition = simonsButtonsObj[2].transform.localPosition + Vector3.up * -0.014f;
        simonsButtonsObj[3].transform.localPosition = simonsButtonsObj[3].transform.localPosition + Vector3.up * -0.014f;
        simonsButtonsObj[4].transform.localPosition = simonsButtonsObj[4].transform.localPosition + Vector3.up * -0.014f;
        simonsButtonsObj[5].transform.localPosition = simonsButtonsObj[5].transform.localPosition + Vector3.up * -0.014f;
    }

    private void ButtonUp(int module, int pos)
    {
        if (module == 0)
        {
            stagesButtonsObj[pos].transform.localPosition = stagesButtonsObj[pos].transform.localPosition + Vector3.up * 0.012f;
        }
        else if (module == 1)
        {
            scramblesButtonsObj[pos].transform.localPosition = scramblesButtonsObj[pos].transform.localPosition + Vector3.up * 0.03f;
        }
        else if (module == 2)
        {
            screamsButtonsObj[pos].transform.localPosition = screamsButtonsObj[pos].transform.localPosition + Vector3.up * 0.2f;
        }
        else if (module == 4)
        {
            stopsButtonsObj[pos].transform.localPosition = stopsButtonsObj[pos].transform.localPosition + Vector3.up * 0.01f;
        }
        else if (module == 5)
        {
            tashaButtonsObj[pos].transform.localPosition = tashaButtonsObj[pos].transform.localPosition + Vector3.up * 0.01f;
        }
        else if (module == 6)
        {
            simonsButtonsObj[pos].transform.localPosition = simonsButtonsObj[pos].transform.localPosition + Vector3.up * 0.014f;
        }
    }

    private void ButtonDown(int module, int pos)
    {
        if (module == 0)
        {
            stagesButtonsObj[pos].transform.localPosition = stagesButtonsObj[pos].transform.localPosition + Vector3.up * -0.012f;
        }
        else if (module == 1)
        {
            scramblesButtonsObj[pos].transform.localPosition = scramblesButtonsObj[pos].transform.localPosition + Vector3.up * -0.03f;
        }
        else if (module == 2)
        {
            screamsButtonsObj[pos].transform.localPosition = screamsButtonsObj[pos].transform.localPosition + Vector3.up * -0.2f;
        }
        else if (module == 4)
        {
            stopsButtonsObj[pos].transform.localPosition = stopsButtonsObj[pos].transform.localPosition + Vector3.up * -0.01f;
        }
        else if (module == 5)
        {
            tashaButtonsObj[pos].transform.localPosition = tashaButtonsObj[pos].transform.localPosition + Vector3.up * -0.01f;
        }
        else if (module == 6)
        {
            simonsButtonsObj[pos].transform.localPosition = simonsButtonsObj[pos].transform.localPosition + Vector3.up * -0.014f;
        }
    }

    private string integerToPosition(int i)
    {
        if(i == 0)
        {
            return "Top Left";
        }
        else if (i == 1)
        {
            return "Top Right";
        }
        else if (i == 2)
        {
            return "Right";
        }
        else if (i == 3)
        {
            return "Bottom Right";
        }
        else if (i == 4)
        {
            return "Bottom Left";
        }
        else if (i == 5)
        {
            return "Left";
        }
        return "";
    }

    private bool allSameColors()
    {
        for(int i = 1; i < 6; i++)
        {
            if (!colorsOfButtons[i].Equals(colorsOfButtons[i - 1]))
            {
                return false;
            }
        }
        return true;
    }

    private bool allTheSameAnswers()
    {
        if(presses.Count != correctColorsModified.Count)
        {
            return false;
        }
        for(int i = 0; i < presses.Count; i++)
        {
            if (!presses[i].Equals(correctColorsModified[i]))
            {
                return false;
            }
        }
        return true;
    }

    private bool remainingStoresButtons()
    {
        for(int i = 0; i < 6; i++)
        {
            //replace 3 with whatever stores will be
            if(modulesOfButtons[i] == 3 && allSamePresses[i] == false)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator downButtonScreamsEnd(int localpos)
    {
        int movement = 0;
        while (movement != 20)
        {
            yield return new WaitForSeconds(0.02f);
            if (modulesOfButtons[localpos] == 0)
            {
                stagesButtonsObj[localpos].transform.localPosition = stagesButtonsObj[localpos].transform.localPosition + Vector3.up * -0.0006f;
            }
            else if (modulesOfButtons[localpos] == 1)
            {
                scramblesButtonsObj[localpos].transform.localPosition = scramblesButtonsObj[localpos].transform.localPosition + Vector3.up * -0.0015f;
            }
            else if (modulesOfButtons[localpos] == 2)
            {
                screamsButtonsObj[localpos].transform.localPosition = screamsButtonsObj[localpos].transform.localPosition + Vector3.up * -0.01f;
            }
            else if (modulesOfButtons[localpos] == 4)
            {
                stopsButtonsObj[localpos].transform.localPosition = stopsButtonsObj[localpos].transform.localPosition + Vector3.up * -0.0005f;
            }
            else if (modulesOfButtons[localpos] == 5)
            {
                tashaButtonsObj[localpos].transform.localPosition = tashaButtonsObj[localpos].transform.localPosition + Vector3.up * -0.0005f;
            }
            else if (modulesOfButtons[localpos] == 6)
            {
                simonsButtonsObj[localpos].transform.localPosition = simonsButtonsObj[localpos].transform.localPosition + Vector3.up * -0.0007f;
            }
            movement++;
        }
    }

    private IEnumerator inCooldown(bool reset)
    {
        yield return new WaitForSeconds(1.5f);
        if (reset)
        {
            for (int i = 0; i < alreadyUp.Count; i++)
            {
                ButtonDown(typeUp[i], posUp[i]);
            }
        }
        Start();
        cooldown = false;
    }

    private IEnumerator dealWithLights(int i)
    {
        dealingWithLights = true;
        int[] temp = { 0, 1, 5, 2, 4, 3 };
        stagesBases[temp[i]].SetActive(false);
        stagesLights[temp[i]].enabled = true;
        scramblesLights[temp[i]].enabled = true;
        screamsLights[temp[i]].enabled = true;
        stopsLights[temp[i]].enabled = true;
        tashaLights[temp[i]].enabled = true;
        simonsLights[temp[i]].enabled = true;
        yield return new WaitForSeconds(0.05f);
        stagesBases[temp[i]].SetActive(true);
        stagesLights[temp[i]].enabled = false;
        scramblesLights[temp[i]].enabled = false;
        screamsLights[temp[i]].enabled = false;
        stopsLights[temp[i]].enabled = false;
        tashaLights[temp[i]].enabled = false;
        simonsLights[temp[i]].enabled = false;
        yield return new WaitForSeconds(0.05f);
        dealingWithLights = false;
    }

    private IEnumerator flashingStart()
    {
        //Play Simon's Stages/Simon Stages intro sequence if a button from it is available
        if (modulesOfButtons.Contains(0))
        {
            audio.PlaySoundAtTransform("scaryRiff", transform);
            for(int i = 0; i < 6; i++)
            {
                StartCoroutine(dealWithLights(i));
                while (dealingWithLights) { yield return new WaitForSeconds(0.0001f); }
            }
            for (int i = 4; i >= 0; i--)
            {
                StartCoroutine(dealWithLights(i));
                while (dealingWithLights) { yield return new WaitForSeconds(0.0001f); }
            }
            for (int i = 1; i < 6; i++)
            {
                StartCoroutine(dealWithLights(i));
                while (dealingWithLights) { yield return new WaitForSeconds(0.0001f); }
            }
            for (int i = 4; i >= 0; i--)
            {
                StartCoroutine(dealWithLights(i));
                while (dealingWithLights) { yield return new WaitForSeconds(0.0001f); }
            }
            for (int i = 1; i < 6; i++)
            {
                StartCoroutine(dealWithLights(i));
                while (dealingWithLights) { yield return new WaitForSeconds(0.0001f); }
            }
            for (int k = 0; k < 42; k++)
            {
                for (int i = 0; i < 6; i++)
                {
                    stagesBases[i].SetActive(false);
                    stagesLights[i].enabled = true;
                    scramblesLights[i].enabled = true;
                    screamsLights[i].enabled = true;
                    stopsLights[i].enabled = true;
                    tashaLights[i].enabled = true;
                    simonsLights[i].enabled = true;
                }
                yield return new WaitForSeconds(0.05f);
                for (int i = 0; i < 6; i++)
                {
                    stagesBases[i].SetActive(true);
                    stagesLights[i].enabled = false;
                    scramblesLights[i].enabled = false;
                    screamsLights[i].enabled = false;
                    stopsLights[i].enabled = false;
                    tashaLights[i].enabled = false;
                    simonsLights[i].enabled = false;
                }
                yield return new WaitForSeconds(0.05f);
            }
        }
        cooldown = false;
        StartCoroutine(flashSequence());
    }

    private IEnumerator solveAnim()
    {
        if (modulesOfButtons.Contains(0))
        {
            audio.PlaySoundAtTransform("solveRiff", transform);
            for (int i = 0; i < 6; i++)
            {
                stagesBases[i].SetActive(false);
                stagesLights[i].enabled = true;
                scramblesLights[i].enabled = true;
                screamsLights[i].enabled = true;
                stopsLights[i].enabled = true;
                tashaLights[i].enabled = true;
                simonsLights[i].enabled = true;
            }
            yield return new WaitForSeconds(1.0f);
            for (int i = 0; i < 6; i++)
            {
                stagesBases[i].SetActive(true);
                stagesLights[i].enabled = false;
                scramblesLights[i].enabled = false;
                screamsLights[i].enabled = false;
                stopsLights[i].enabled = false;
                tashaLights[i].enabled = false;
                simonsLights[i].enabled = false;
            }
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < 6; i++)
            {
                stagesBases[i].SetActive(false);
                stagesLights[i].enabled = true;
                scramblesLights[i].enabled = true;
                screamsLights[i].enabled = true;
                stopsLights[i].enabled = true;
                tashaLights[i].enabled = true;
                simonsLights[i].enabled = true;
            }
            yield return new WaitForSeconds(1.0f);
            for (int i = 0; i < 6; i++)
            {
                stagesBases[i].SetActive(true);
                stagesLights[i].enabled = false;
                scramblesLights[i].enabled = false;
                screamsLights[i].enabled = false;
                stopsLights[i].enabled = false;
                tashaLights[i].enabled = false;
                simonsLights[i].enabled = false;
            }
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < 6; i++)
            {
                stagesBases[i].SetActive(false);
                stagesLights[i].enabled = true;
                scramblesLights[i].enabled = true;
                screamsLights[i].enabled = true;
                stopsLights[i].enabled = true;
                tashaLights[i].enabled = true;
                simonsLights[i].enabled = true;
            }
        }
        yield return new WaitForSeconds(0.5f);
        if (modulesOfButtons.Contains(2))
        {
            for (int i = 0; i < 6; i++)
            {
                stagesBases[i].SetActive(true);
                stagesLights[i].enabled = false;
                scramblesLights[i].enabled = false;
                screamsLights[i].enabled = false;
                stopsLights[i].enabled = false;
                tashaLights[i].enabled = false;
                simonsLights[i].enabled = false;
            }
            audio.PlaySoundAtTransform("Victory", transform);
            int counter1 = posOfLastPress;
            int counter2 = posOfLastPress;
            stagesBases[counter1].SetActive(false);
            stagesLights[counter1].enabled = true;
            scramblesLights[counter1].enabled = true;
            screamsLights[counter1].enabled = true;
            stopsLights[counter1].enabled = true;
            tashaLights[counter1].enabled = true;
            simonsLights[counter1].enabled = true;
            yield return new WaitForSeconds(0.1f);
            stagesBases[counter1].SetActive(true);
            stagesLights[counter1].enabled = false;
            scramblesLights[counter1].enabled = false;
            screamsLights[counter1].enabled = false;
            stopsLights[counter1].enabled = false;
            tashaLights[counter1].enabled = false;
            simonsLights[counter1].enabled = false;
            for (int i = 0; i < 12; i++)
            {
                counter1--;
                if (counter1 == -1)
                {
                    counter1 = 5;
                }
                counter2++;
                if (counter2 == 6)
                {
                    counter2 = 0;
                }
                stagesBases[counter1].SetActive(false);
                stagesLights[counter1].enabled = true;
                scramblesLights[counter1].enabled = true;
                screamsLights[counter1].enabled = true;
                stopsLights[counter1].enabled = true;
                tashaLights[counter1].enabled = true;
                simonsLights[counter1].enabled = true;
                stagesBases[counter2].SetActive(false);
                stagesLights[counter2].enabled = true;
                scramblesLights[counter2].enabled = true;
                screamsLights[counter2].enabled = true;
                stopsLights[counter2].enabled = true;
                tashaLights[counter2].enabled = true;
                simonsLights[counter2].enabled = true;
                yield return new WaitForSeconds(0.1f);
                stagesBases[counter1].SetActive(true);
                stagesLights[counter1].enabled = false;
                scramblesLights[counter1].enabled = false;
                screamsLights[counter1].enabled = false;
                stopsLights[counter1].enabled = false;
                tashaLights[counter1].enabled = false;
                simonsLights[counter1].enabled = false;
                stagesBases[counter2].SetActive(true);
                stagesLights[counter2].enabled = false;
                scramblesLights[counter2].enabled = false;
                screamsLights[counter2].enabled = false;
                stopsLights[counter2].enabled = false;
                tashaLights[counter2].enabled = false;
                simonsLights[counter2].enabled = false;
            }
            int localpos = posOfLastPress-1;
            for (int i = 0; i < 6; i++)
            {
                localpos++;
                if(localpos == 6)
                {
                    localpos = 0;
                }
                StartCoroutine(downButtonScreamsEnd(localpos));
                yield return new WaitForSeconds(0.3f);
            }
        }
    }

    private IEnumerator flashSequence()
    {
        if (!shouldFlash) { yield break; };
        yield return new WaitForSeconds(0.1f);
        if (!shouldFlash) { yield break; };
        for (int i = 0; i < stage + 2; i++)
        {
            if (modulesOfButtons[buttonsFlashing[i]] == 0)
            {
                if (firstPress)
                    audio.PlaySoundAtTransform("press" + stagesRandomPressSounds[buttonsFlashing[i]], stagesButtons[buttonsFlashing[i]].transform);
                stagesBases[buttonsFlashing[i]].SetActive(false);
                stagesLights[buttonsFlashing[i]].enabled = true;
                if (!shouldFlash) { yield break; }
                yield return new WaitForSeconds(0.5f);
                if (!shouldFlash) { yield break; }
                stagesBases[buttonsFlashing[i]].SetActive(true);
                stagesLights[buttonsFlashing[i]].enabled = false;
                if (!shouldFlash) { yield break; }
                yield return new WaitForSeconds(0.25f);
                if (!shouldFlash) { yield break; }
            }
            else if (modulesOfButtons[buttonsFlashing[i]] == 1)
            {
                scramblesLights[buttonsFlashing[i]].enabled = true;
                if (!shouldFlash) { yield break; }
                yield return new WaitForSeconds(0.5f);
                if (!shouldFlash) { yield break; }
                scramblesLights[buttonsFlashing[i]].enabled = false;
                if (!shouldFlash) { yield break; }
                yield return new WaitForSeconds(0.25f);
                if (!shouldFlash) { yield break; }
            }
            else if (modulesOfButtons[buttonsFlashing[i]] == 2)
            {
                if (firstPress)
                    audio.PlaySoundAtTransform("Sound" + screamsRandomFlashSounds[buttonsFlashing[i]], screamsButtons[buttonsFlashing[i]].transform);
                screamsLights[buttonsFlashing[i]].enabled = true;
                if (!shouldFlash) { yield break; }
                yield return new WaitForSeconds(0.5f);
                if (!shouldFlash) { yield break; }
                screamsLights[buttonsFlashing[i]].enabled = false;
                if (!shouldFlash) { yield break; }
                yield return new WaitForSeconds(0.25f);
                if (!shouldFlash) { yield break; }
            }
            else if (modulesOfButtons[buttonsFlashing[i]] == 4)
            {
                if (firstPress)
                    audio.PlaySoundAtTransform("tone" + stopsRandomPressSounds[buttonsFlashing[i]], stopsButtons[buttonsFlashing[i]].transform);
                stopsLights[buttonsFlashing[i]].enabled = true;
                if (!shouldFlash) { yield break; }
                yield return new WaitForSeconds(0.5f);
                if (!shouldFlash) { yield break; }
                stopsLights[buttonsFlashing[i]].enabled = false;
                if (!shouldFlash) { yield break; }
                yield return new WaitForSeconds(0.25f);
                if (!shouldFlash) { yield break; }
            }
            else if (modulesOfButtons[buttonsFlashing[i]] == 5)
            {
                if (firstPress)
                    audio.PlaySoundAtTransform("tasha" + tashaRandomPressSounds[buttonsFlashing[i]], tashaButtons[buttonsFlashing[i]].transform);
                tashaLights[buttonsFlashing[i]].enabled = true;
                if (!shouldFlash) { yield break; }
                yield return new WaitForSeconds(0.5f);
                if (!shouldFlash) { yield break; }
                tashaLights[buttonsFlashing[i]].enabled = false;
                if (!shouldFlash) { yield break; }
                yield return new WaitForSeconds(0.25f);
                if (!shouldFlash) { yield break; }
            }
            else if (modulesOfButtons[buttonsFlashing[i]] == 6)
            {
                if (firstPress)
                    audio.PlaySoundAtTransform("simonspress" + tashaRandomPressSounds[buttonsFlashing[i]], simonsButtons[buttonsFlashing[i]].transform);
                simonsLights[buttonsFlashing[i]].enabled = true;
                if (!shouldFlash) { yield break; }
                yield return new WaitForSeconds(0.5f);
                if (!shouldFlash) { yield break; }
                simonsLights[buttonsFlashing[i]].enabled = false;
                if (!shouldFlash) { yield break; }
                yield return new WaitForSeconds(0.25f);
                if (!shouldFlash) { yield break; }
            }
        }
        if (!shouldFlash) { yield break; }
        yield return new WaitForSeconds(3.0f);
        if (!shouldFlash) { yield break; }
        StartCoroutine(flashSequence());
    }

    private IEnumerator timer()
    {
        if(moduleSolved) { yield break; }
        while(timeBeforeNextSequence > 0f)
        {
            timeBeforeNextSequence -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
        if (moduleSolved) { yield break; }
        shouldFlash = true;
        timeBeforeNextSequence = 3.0f;
        if (moduleSolved) { yield break; }
        StartCoroutine(flashSequence());
    }

    private IEnumerator ctrlInputTimerMethod()
    {
        while (timeStopsCtrlInput > 0f)
        {
            timeStopsCtrlInput -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
        Debug.LogFormat("[Simon's Ultimate Showdown #{0}] You ran out of time to press the Control Input! Strike! Resetting stage...", moduleId);
        GetComponent<KMBombModule>().HandleStrike();
        resetButtons = true;
        stage--;
        cooldown = true;
        stopsMissedInput = true;
        for (int i = 0; i < 6; i++)
        {
            stagesLights[i].enabled = false;
            scramblesLights[i].enabled = false;
            screamsLights[i].enabled = false;
            stopsLights[i].enabled = false;
            stagesBases[i].SetActive(true);
        }
        StartCoroutine(inCooldown(true));
    }

    //All of these are here in case speed of flashes needs to be changed for individual modules 
    //in case you were wondering why I dont just use one method for them all
    private IEnumerator singleStagesFlash(Light turnon, GameObject basse)
    {
        turnon.enabled = true;
        basse.SetActive(false);
        if (!(indexOfStop == presses.Count) || allSame)
        {
            yield return new WaitForSeconds(0.5f);
            turnon.enabled = false;
            basse.SetActive(true);
        }
        pressFlash = null;
    }

    private IEnumerator singleScramblesFlash(Light turnon)
    {
        turnon.enabled = true;
        if (!(indexOfStop == presses.Count) || allSame)
        {
            yield return new WaitForSeconds(0.5f);
            turnon.enabled = false;
        }
        pressFlash = null;
    }

    private IEnumerator singleScreamsFlash(Light turnon)
    {
        turnon.enabled = true;
        if (!(indexOfStop == presses.Count) || allSame)
        {
            yield return new WaitForSeconds(0.5f);
            turnon.enabled = false;
        }
        pressFlash = null;
    }

    private IEnumerator singleStopsFlash(Light turnon)
    {
        turnon.enabled = true;
        if (!(indexOfStop == presses.Count) || allSame)
        {
            yield return new WaitForSeconds(0.5f);
            turnon.enabled = false;
        }
        pressFlash = null;
    }

    private IEnumerator singleTashaFlash(Light turnon)
    {
        turnon.enabled = true;
        if (!(indexOfStop == presses.Count) || allSame)
        {
            yield return new WaitForSeconds(0.5f);
            turnon.enabled = false;
        }
        pressFlash = null;
    }

    private IEnumerator singleSimonsFlash(Light turnon)
    {
        turnon.enabled = true;
        if (!(indexOfStop == presses.Count) || allSame)
        {
            yield return new WaitForSeconds(0.5f);
            turnon.enabled = false;
        }
        pressFlash = null;
    }

    //twitch plays
    private string charToColor(char c)
    {
        if (c.Equals('r') || c.Equals('R'))
        {
            return "red";
        }
        else if (c.Equals('b') || c.Equals('B'))
        {
            return "blue";
        }
        else if (c.Equals('y') || c.Equals('Y'))
        {
            return "yellow";
        }
        else if (c.Equals('o') || c.Equals('O'))
        {
            return "orange";
        }
        else if (c.Equals('m') || c.Equals('M'))
        {
            return "magenta";
        }
        else if (c.Equals('g') || c.Equals('G'))
        {
            return "green";
        }
        else if (c.Equals('i') || c.Equals('I'))
        {
            return "pink";
        }
        else if (c.Equals('p') || c.Equals('P'))
        {
            return "purple";
        }
        else if (c.Equals('l') || c.Equals('L'))
        {
            return "lime";
        }
        else if (c.Equals('c') || c.Equals('C'))
        {
            return "cyan";
        }
        else if (c.Equals('w') || c.Equals('W'))
        {
            return "white";
        }
        return "";
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <colors> [Presses the specified colors (no full color names)] | !{0} colorblind [Toggles colorblind mode] | Valid colors are R(ed), B(lue), Y(ellow), O(range), M(agenta), G(reen), (p)I(nk), P(urple), L(ime), C(yan), and W(hite)";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*colorblind\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            Debug.LogFormat("[Simon's Ultimate Showdown #{0}] Toggled colorblind mode! (TP)", moduleId);
            if (colorblindActive)
            {
                colorblindActive = false;
                for(int i = 0; i < 6; i++)
                {
                    cbtexts[i].SetActive(false);
                }
            }
            else
            {
                colorblindActive = true;
                for (int i = 0; i < 6; i++)
                {
                    cbtexts[i].SetActive(true);
                }
            }
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (cooldown)
            {
                yield return "sendtochaterror Cannot press buttons while the module is resetting!";
                yield break;
            }
            else if (parameters.Length > 2)
            {
                for(int i = 2; i < parameters.Length; i++)
                {
                    parameters[1] += parameters[i];
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify which colors need to pressed!";
            }
            char[] allValids = { 'r', 'b', 'y', 'o', 'm', 'g', 'p', 'l', 'c', 'w', 'i' };
            parameters[1] = parameters[1].ToLower();
            for (int i = 0; i < parameters[1].Length; i++)
            {
                if (!allValids.Contains(parameters[1].ElementAt(i)))
                {
                    yield return "sendtochaterror Invalid character detected: '" + parameters[1].ElementAt(i) + "'";
                    yield break;
                }
                if (!colorsOfButtons.Contains(charToColor(parameters[1].ElementAt(i))))
                {
                    yield return "sendtochaterror The color '" + charToColor(parameters[1].ElementAt(i)) + "' is not on any buttons!";
                    yield break;
                }
            }
            yield return null;
            for (int i = 0; i < parameters[1].Length; i++)
            {
                List<int> possiblePresses = new List<int>();
                for (int j = 0; j < colorsOfButtons.Length; j++)
                {
                    if (colorsOfButtons[j].Equals(charToColor(parameters[1].ElementAt(i))))
                    {
                        possiblePresses.Add(j);
                    }
                }
                possiblePresses = possiblePresses.Shuffle();
                int rando = UnityEngine.Random.Range(0, possiblePresses.Count);
                actualButtons[possiblePresses[rando]].OnInteract();
                if (stopsCtrlInput)
                {
                    yield return "sendtochat It's time for Control Input! So far you have inputted " + presses.Count + " correct button presses!";
                    yield break;
                }
                while (pressFlash != null) { yield return new WaitForSeconds(0.1f); }
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        presses.Clear();
        if (stopsCtrlInput)
        {
            stopsCtrlInput = false;
            StopCoroutine(ctrlInputTimer);
            for (int i = 0; i < 6; i++)
            {
                stagesLights[i].enabled = false;
                scramblesLights[i].enabled = false;
                screamsLights[i].enabled = false;
                stopsLights[i].enabled = false;
                stagesBases[i].SetActive(true);
            }
        }
        int start = stage;
        for (int g = start; g < 4; g++)
        {
            while (cooldown || resetButtons) { yield return new WaitForSeconds(0.1f); yield return true; }
            if (allSame)
            {
                allSamePresses[0] = false;
                allSamePresses[1] = false;
                allSamePresses[2] = false;
                allSamePresses[3] = false;
                allSamePresses[4] = false;
                allSamePresses[5] = false;
                if (modulesOfButtons[0] == 3)
                {
                    actualButtons[0].OnInteract();
                }
                while (pressFlash != null) { yield return new WaitForSeconds(0.1f); }
                if (modulesOfButtons[1] == 3)
                {
                    actualButtons[1].OnInteract();
                }
                while (pressFlash != null) { yield return new WaitForSeconds(0.1f); }
                if (modulesOfButtons[2] == 3)
                {
                    actualButtons[2].OnInteract();
                }
                while (pressFlash != null) { yield return new WaitForSeconds(0.1f); }
                if (modulesOfButtons[3] == 3)
                {
                    actualButtons[3].OnInteract();
                }
                while (pressFlash != null) { yield return new WaitForSeconds(0.1f); }
                if (modulesOfButtons[4] == 3)
                {
                    actualButtons[4].OnInteract();
                }
                while (pressFlash != null) { yield return new WaitForSeconds(0.1f); }
                if (modulesOfButtons[5] == 3)
                {
                    actualButtons[5].OnInteract();
                }
                while (pressFlash != null) { yield return new WaitForSeconds(0.1f); }
                List<int> values = new List<int>();
                for (int j = 0; j < 6; j++)
                {
                    if (allSamePresses[j] == false)
                    {
                        values.Add(j);
                    }
                }
                values = values.Shuffle();
                for (int k = 0; k < values.Count; k++)
                {
                    actualButtons[values.ElementAt(k)].OnInteract();
                    while (pressFlash != null) { yield return new WaitForSeconds(0.1f); }
                }
            }
            else
            {
                int temp = correctColorsModified.Count();
                for (int k = 0; k < temp; k++)
                {
                    List<int> possiblePresses = new List<int>();
                    for (int j = 0; j < colorsOfButtons.Length; j++)
                    {
                        if (stopsCtrlInput)
                        {
                            if (colorsOfButtons[j].Equals(correctCtrlInput))
                            {
                                possiblePresses.Add(j);
                            }
                        }
                        else
                        {
                            if (colorsOfButtons[j].Equals(correctColorsModified[k]))
                            {
                                possiblePresses.Add(j);
                            }
                        }
                    }
                    possiblePresses = possiblePresses.Shuffle();
                    int rando = UnityEngine.Random.Range(0, possiblePresses.Count);
                    actualButtons[possiblePresses[rando]].OnInteract();
                    if (stopsCtrlInput)
                    {
                        k--;
                        yield return new WaitForSeconds(0.5f);
                    }
                    while (pressFlash != null) { yield return new WaitForSeconds(0.1f); }
                }
            }
        }
    }
}