using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;


public class EditableExperiment : CoroutineExperiment
{
    public delegate void StateChange(string stateName, bool on, Dictionary<string, object> extraData);
    public static StateChange OnStateChange;

    private static ushort wordsSeen;
    private static ushort session;
    private static List<IronPython.Runtime.PythonDictionary> words;
    private static ExperimentSettings currentSettings;

    public RamulatorInterface ramulatorInterface;
    public ElememInterface elememInterface;
    public VideoControl countdownVideoPlayer;
    public KeyCode pauseKey = KeyCode.P;
    public GameObject pauseIndicator;
    public ScriptedEventReporter scriptedEventReporter;
    public VoiceActivityDetection VAD;

    private bool paused = false;
    private string current_phase_type = "";

    //List<int> stimListTypes;

    void UncaughtExceptionHandler(object sender, UnhandledExceptionEventArgs args)
    {
        Exception e = (Exception)args.ExceptionObject;
        Debug.Log("UncaughtException: " + e.Message);
        Debug.Log("UncaughtException: " + e);

        Dictionary<string, object> exceptionData = new Dictionary<string, object>()
            { { "name", e.Message },
              { "traceback", e.ToString() } };
        scriptedEventReporter.ReportScriptedEvent("unhandled program exception", exceptionData, false);
    }

    //use update to collect user input every frame
    void Update()
    {
        //check for pause
        if (Input.GetKeyDown(pauseKey))
        {
            paused = !paused;
            pauseIndicator.SetActive(paused);
            elememInterface.SendStateMessage("PAUSED", new Dictionary<string, object> { { "state", paused } });
        }
    }

    private IEnumerator PausableWait(float waitTime)
    {
        float endTime = Time.time + waitTime;
        while (Time.time < endTime)
        {
            if (paused)
                endTime += Time.deltaTime;
            yield return null;
        }
    }

    protected IEnumerator DoCPSVideo()
    {
        string startingPath = Path.Combine(UnityEPL.GetParticipantFolder(), "..", "..", "CPS_Movies");
        var extensions = new[] {
            new SFB.ExtensionFilter("Videos", "mp4", "mov"),
            new SFB.ExtensionFilter("All Files", "*" ),
        };

        string[] videoPaths = new string[0];
        while (videoPaths.Length == 0)
            videoPaths = SFB.StandaloneFileBrowser.OpenFilePanel("Select Video To Watch", startingPath, extensions, false);
        string videoPath = videoPaths[0].Replace("%20", " ");
        yield return videoPlayer.SetVideo(videoPath);

        yield return PressAnyKey("In this experiment, you will watch a short educational film lasting about twenty-five minutes. Please pay attention to the film to the best of your ability. You will be asked a series of questions about the video after its completion. After the questionnaire, you will have the opportunity to take a break.\n\n Press any key to begin watching.");

        var movieInfo = new Dictionary<string, object> {
            { "movie title", Path.GetFileName(videoPath) },
            { "movie path", Path.GetDirectoryName(videoPath)},
            { "movie duration seconds", videoPlayer.VideoDurationSeconds()} };
        scriptedEventReporter.ReportScriptedEvent("movie", movieInfo);
        SetElememState("ENCODING", movieInfo);

        elememInterface.SendCCLStartMessage(videoPlayer.VideoDurationSeconds() - 10); // Remove 10s to not overrun video legnth
        videoPlayer.StartVideo("");
        while (videoPlayer.IsPlaying())
            yield return null;
    }

    IEnumerator DoCPS()
    {
        elememInterface.SendTrialMessage(0, true);

        yield return DoCPSVideo();

        elememInterface.SendExitMessage();
        textDisplayer.DisplayText("display end message", "Woo!  The experiment is over.");
    }

    IEnumerator Start()
    {
        // Exception handling
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UncaughtExceptionHandler);

        Cursor.visible = false;
        Application.runInBackground = true;

        if (currentSettings.Equals(default(ExperimentSettings)))
            throw new UnityException("Please call ConfigureExperiment before loading the experiment scene.");

        //write versions to logfile
        Dictionary<string, object> versionsData = new Dictionary<string, object>();
        versionsData.Add("UnityEPL version", Application.version);
        versionsData.Add("Task version", ExperimentSettings.taskVersion);
        versionsData.Add("Experiment version", currentSettings.version);
        versionsData.Add("Logfile version", "1");
        scriptedEventReporter.ReportScriptedEvent("versions", versionsData);

        // Sys 4
        currentSettings.useElemem = UnityEPL.GetUseElemem();
        if (currentSettings.useElemem)
            currentSettings.useRamulator = false;
        yield return elememInterface.BeginNewSession(session, !currentSettings.useElemem);

        // Sys 3
        if (Config.noRamulator)
            currentSettings.useRamulator = false;
        if (currentSettings.useRamulator)
            yield return ramulatorInterface.BeginNewSession(session);

        // Sys 1
        if (!Config.noSyncbox && !currentSettings.useElemem && !currentSettings.useRamulator)
            GameObject.Find("Syncbox").GetComponentInChildren<Syncbox>().enabled = true;


        // CPS
        if (currentSettings.experimentName == "CPS")
        {
            yield return DoCPS();
            yield break;
        }

        // TESTING CODE
        //elememInterface.SendCLMessage("CLNORMALIZE", currentSettings.clDuration);
        //yield return new WaitForSeconds(7);
        //elememInterface.SendCLMessage("CLNORMALIZE", currentSettings.clDuration);
        //yield return new WaitForSeconds(7);
        //elememInterface.SendCLMessage("CLSTIM", currentSettings.clDuration);

        //stimListTypes = GenStimLists();


        VAD.DoVAD(false);

        //starting from the beginning of the latest uncompleted list, do lists until the experiment is finished or stopped
        int startList = wordsSeen / currentSettings.wordsPerList;

        for (int i = startList; i < currentSettings.numberOfLists; i++)
        {
            current_phase_type = (string)words[wordsSeen]["phase_type"];

            ramulatorInterface.BeginNewTrial(i);
            elememInterface.SendTrialMessage(i, current_phase_type == "STIM");
            //elememInterface.SendTrialMessage(i, stimListTypes[i] == 1);

            if (startList == 0 && i == 0)
            {
                yield return DoIntroductionVideo();
                yield return DoSubjectSessionQuitPrompt(UnityEPL.GetSessionNumber());
                yield return DoMicrophoneTest();
                yield return PressAnyKey("Press any key for practice trial.");
            }

            if (i == 1 && i != startList)
            {
                yield return PressAnyKey("Please let the experimenter know \n" +
                "if you have any questions about \n" +
                "what you just did.\n\n" +
                "If you think you understand, \n" +
                "Please explain the task to the \n" +
                "experimenter in your own words.\n\n" +
                "Press any key to continue \n" +
                "to the first list.");
            }

            if (i != 0)
                yield return PressAnyKey("Press any key for trial " + i.ToString() + ".");

            SetRamulatorState("COUNTDOWN", true, new Dictionary<string, object>() { { "current_trial", i } });
            SetElememState("COUNTDOWN");
            yield return DoCountdown();
            SetRamulatorState("COUNTDOWN", false, new Dictionary<string, object>() { { "current_trial", i } });

            SetRamulatorState("ENCODING", true, new Dictionary<string, object>() { { "current_trial", i } });
            SetElememState("ENCODING");
            yield return DoEncoding(i);
            SetRamulatorState("ENCODING", false, new Dictionary<string, object>() { { "current_trial", i } });

            SetRamulatorState("DISTRACT", true, new Dictionary<string, object>() { { "current_trial", i } });
            SetElememState("DISTRACT");
            yield return DoDistractor();
            SetRamulatorState("DISTRACT", false, new Dictionary<string, object>() { { "current_trial", i } });

            yield return PausableWait(UnityEngine.Random.Range(currentSettings.minPauseBeforeRecall, currentSettings.maxPauseBeforeRecall));

            // NOTE: This is a fix for the delay of retrieval issue, where the pre recall prompt delays
            // the start of recording. To keep the clarity of the Ramulator state structure, the delay
            // is now owned by the PreRecall function
            yield return DoPreRecall();
            SetRamulatorState("RETRIEVAL", true, new Dictionary<string, object>() { { "current_trial", i } });
            SetElememState("RETRIEVAL");
            yield return DoRecall();
            SetRamulatorState("RETRIEVAL", false, new Dictionary<string, object>() { { "current_trial", i } });

            if (currentSettings.pauseBetweenGroups > 0) {
                if (currentSettings.listGroupSize == 0 ||
                    (i>0 && (i % currentSettings.listGroupSize) == 0)) {
                    
                    string state = "DELAYSHAM";
                    if (currentSettings.experimentName == "TICCLSb") {
                        if (i / currentSettings.listGroupSize >= 2) {
                            state = "DELAY";
                        }
                    }
                    else if (currentSettings.experimentName == "TICCLS") {
                        if (currentSettings.numberOfLists - i < currentSettings.listGroupSize) {
                            state = "DELAY";
                        }
                    }

                    SetRamulatorState(state, true, new Dictionary<string, object>() { { "current_trial", i } });
                    textDisplayer.DisplayText("display wait message", "The next list will begin after the waiting period.");
                    yield return PausableWait(currentSettings.pauseBetweenGroups);
                    SetRamulatorState(state, false, new Dictionary<string, object>() { { "current_trial", i } });
                }
            }
        }

        ramulatorInterface.SendExitMessage();
        elememInterface.SendExitMessage();
        textDisplayer.DisplayText("display end message", "Woo!  The experiment is over.");
    }

    private IEnumerator DoCountdown()
    {
        countdownVideoPlayer.StartVideo();
        while (countdownVideoPlayer.IsPlaying())
            yield return null;
        //      for (int i = 0; i < currentSettings.countdownLength; i++)
        //      {
        //          textDisplayer.DisplayText ("countdown display", (currentSettings.countdownLength - i).ToString ());
        //          yield return PausableWait (currentSettings.countdownTick);
        //      }

    }

    private List<int> GenStimLists() {
        List<int> subList = Enumerable.Repeat(1, ((FRListGenerator)currentSettings.wordListGenerator).STIM_LIST_COUNT)
                                      .Concat(Enumerable.Repeat(2, ((FRListGenerator)currentSettings.wordListGenerator).NONSTIM_LIST_COUNT))
                                      .ToList();
        subList.Shuffle(new System.Random());

        return Enumerable.Repeat(-1, 1)
                         .Concat(Enumerable.Repeat(0, ((FRListGenerator)currentSettings.wordListGenerator).BASELINE_LIST_COUNT))
                         .Concat(subList).ToList();

        //elememInterface.SendStimSelectMessage(); // TODO: JPB: (need) Fill in StimSelectMessage
    }


    private IEnumerator DoEncoding(int listNum)
    {
        int currentList = wordsSeen / currentSettings.wordsPerList;
        wordsSeen = (ushort)(currentList * currentSettings.wordsPerList);
        Debug.Log("Beginning list index " + currentList.ToString());
        SetRamulatorState("ORIENT", true, new Dictionary<string, object>());
        SetElememState("ORIENT");
        string expName = UnityEPL.GetExperimentName();

        if (expName == "FR6" || expName == "IFR6" ||
            expName == "CatFR6" || expName == "ICatFR6") {

            if (current_phase_type == "STIM") {
                string stimTag = words[wordsSeen]["stim_channels"].ToString();
                elememInterface.SendStimSelectMessage(stimTag);
                var stim_selection = new Dictionary<string, object> {
                    { "stim_channels", stimTag } };
                scriptedEventReporter.ReportScriptedEvent("stim_selection",
                    stim_selection);
                Debug.Log("This Trial is using " + stimTag + " for stim");
            }
        }
        textDisplayer.DisplayText("orientation stimulus", "+");
        yield return PausableWait(UnityEngine.Random.Range(currentSettings.minOrientationStimulusLength, currentSettings.maxOrientationStimulusLength));
        textDisplayer.ClearText();
        SetRamulatorState("ORIENT", false, new Dictionary<string, object>());

        for (int i = 0; i < currentSettings.wordsPerList; i++)
        {
            yield return PausableWait(UnityEngine.Random.Range(currentSettings.minISI, currentSettings.maxISI));
            string word = (string)words[wordsSeen]["word"];
            textDisplayer.DisplayText("word stimulus", word);

            if (expName == "FR5" || expName == "FR6" ||
                expName == "CatFR5" || expName == "CatFR6" ||
                expName == "IFR5" || expName == "IFR6" ||
                expName == "ICatFR5" || expName == "ICatFR6") {
                if (current_phase_type == "STIM")
                {
                    elememInterface.SendCLMessage("CLSTIM", currentSettings.clDuration);
                }
                else
                {
                    elememInterface.SendCLMessage("CLNORMALIZE", currentSettings.clDuration);
                }

                //if (stimListTypes[listNum] == 0)
                //{
                //    elememInterface.SendCLMessage("CLNORMALIZE", currentSettings.clDuration);
                //}
                //else if (stimListTypes[listNum] == 1)
                //{
                //    elememInterface.SendCLMessage("CLSTIM", currentSettings.clDuration);
                //}
                //else if (stimListTypes[listNum] == 2)
                //{
                //    elememInterface.SendCLMessage("CLSHAM", currentSettings.clDuration);
                //}
            }

            SetRamulatorWordState(true, words[wordsSeen]);
            SetElememWordState(words[wordsSeen], i, false);
            yield return PausableWait(currentSettings.wordPresentationLength);
            textDisplayer.ClearText();
            SetRamulatorWordState(false, words[wordsSeen]);
            IncrementWordsSeen();
        }
    }

    private void SetRamulatorWordState(bool state, IronPython.Runtime.PythonDictionary wordData)
    {
        Dictionary<string, object> dotNetWordData = new Dictionary<string, object>();
        foreach (string key in wordData.Keys)
            dotNetWordData.Add(key, wordData[key] == null ? "" : wordData[key].ToString());
        SetRamulatorState("WORD", state, dotNetWordData);
    }

    //WAITING, INSTRUCT, COUNTDOWN, ENCODING, WORD, DISTRACT, RETRIEVAL
    protected override void SetRamulatorState(string stateName, bool state, Dictionary<string, object> extraData)
    {
        if (OnStateChange != null)
            OnStateChange(stateName, state, extraData);
        if (!stateName.Equals("WORD"))
            extraData.Add("phase_type", current_phase_type);
        ramulatorInterface.SetState(stateName, state, extraData);
    }

    private void SetElememWordState(IronPython.Runtime.PythonDictionary wordData, int serialPos, bool stim)
    {
        Dictionary<string, object> dotNetWordData = new Dictionary<string, object>();
        foreach (string key in wordData.Keys)
            if (key != "amplitude_index" && key != "stim_channels")
                dotNetWordData.Add(key, wordData[key] == null ? "" : wordData[key].ToString());
        elememInterface.SendWordMessage((string) dotNetWordData["word"], serialPos, stim, dotNetWordData);
    }

    // NO INPUT:  REST, ORIENT, COUNTDOWN, TRIALEND, DISTRACT, INSTRUCT, WAITING, SYNC, ENCODING
    // INPUT:     ISI (float duration), RECALL (float duration)
    protected override void SetElememState(string stateName, Dictionary<string, object> extraData = null)
    {
        if (extraData == null)
            extraData = new Dictionary<string, object>();
        extraData.Add("phase_type", current_phase_type);
        elememInterface.SendStateMessage(stateName, extraData);
    }

private IEnumerator DoDistractor()
    {
        // Fixation Distractor
        if (currentSettings.distrationType == DistractionType.Fixation) {
            textDisplayer.DisplayText("display distractor fixation cross", "+");
            yield return new WaitForSeconds(currentSettings.distractionLength);
            textDisplayer.ClearText();
            yield break;
        }

        // Math Distractor
        float endTime = Time.time + currentSettings.distractionLength;

        string distractor = "";
        string answer = "";

        float displayTime = 0;
        float answerTime = 0;

        bool answered = true;

        int[] distractorProblem = DistractorProblem();

        while (Time.time < endTime || answered == false)
        {
            if (paused)
            {
                endTime += Time.deltaTime;
            }
            if (paused && answered)
            {
                answerTime += Time.deltaTime;
            }
            if (Time.time - answerTime > currentSettings.answerConfirmationTime && answered)
            {
                answered = false;
                distractorProblem = DistractorProblem();
                distractor = distractorProblem[0].ToString() + " + " + distractorProblem[1].ToString() + " + " + distractorProblem[2].ToString() + " = ";
                answer = "";
                textDisplayer.DisplayText("display distractor problem", distractor);
                displayTime = Time.time;
            }
            else
            {
                int numberInput = GetNumberInput();
                if (numberInput != -1)
                {
                    answer = answer + numberInput.ToString();
                    textDisplayer.DisplayText("modify distractor answer", distractor + answer);
                }
                if (Input.GetKeyDown(KeyCode.Backspace) && !answer.Equals(""))
                {
                    answer = answer.Substring(0, answer.Length - 1);
                    textDisplayer.DisplayText("modify distractor answer", distractor + answer);
                }
                if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !answer.Equals(""))
                {
                    answered = true;
                    int result;
                    bool correct;
                    if (int.TryParse(answer, out result) && result == distractorProblem[0] + distractorProblem[1] + distractorProblem[2])
                    {
                        //textDisplayer.ChangeColor (Color.green);
                        correct = true;
                        lowBeep.Play();
                    }
                    else
                    {
                        //textDisplayer.ChangeColor (Color.red);
                        correct = false;
                        lowerBeep.Play();
                    }
                    ReportDistractorAnswered(correct, distractor, answer);
                    answerTime = Time.time;

                    int responseTime = (int)((answerTime - displayTime) * 1000);
                    ramulatorInterface.SendMathMessage(distractor, answer, responseTime, correct);
                    elememInterface.SendMathMessage(distractor, answer, responseTime, correct);
                }
            }
            yield return null;
        }
        textDisplayer.OriginalColor();
        textDisplayer.ClearText();
    }

    private void ReportDistractorAnswered(bool correct, string problem, string answer)
    {
        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        dataDict.Add("correctness", correct.ToString());
        dataDict.Add("problem", problem);
        dataDict.Add("answer", answer);
        scriptedEventReporter.ReportScriptedEvent("distractor answered", dataDict);
    }

    private IEnumerator DoPreRecall() {
        highBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "high beep" }, { "sound duration", highBeep.clip.length.ToString() } });

        textDisplayer.DisplayText("display recall text", "*******");
        yield return PausableWait(currentSettings.recallTextDisplayLength);
        textDisplayer.ClearText();
    }

    private IEnumerator DoRecall()
    {
        SetElememState("RECALL", new Dictionary<string, object> { { "duration", currentSettings.recallLength } });
        VAD.DoVAD(true);
        //path
        int listno = (wordsSeen / 12) - 1;
        string output_directory = UnityEPL.GetDataPath();
        string wavFilePath = System.IO.Path.Combine(output_directory, listno.ToString() + ".wav");
        string lstFilePath = System.IO.Path.Combine(output_directory, listno.ToString() + ".lst");
        WriteLstFile(lstFilePath);
        soundRecorder.StartRecording(wavFilePath);
        yield return PausableWait(currentSettings.recallLength);

        soundRecorder.StopRecording();
        textDisplayer.ClearText();
        lowBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "low beep" }, { "sound duration", lowBeep.clip.length.ToString() } });
        VAD.DoVAD(false);
    }

    private void WriteLstFile(string lstFilePath)
    {
        string[] lines = new string[currentSettings.wordsPerList];
        int startIndex = wordsSeen - currentSettings.wordsPerList;
        for (int i = startIndex; i < wordsSeen; i++)
        {
            IronPython.Runtime.PythonDictionary word = words[i];
            lines[i - (startIndex)] = (string)word["word"];
        }
        System.IO.FileInfo lstFile = new System.IO.FileInfo(lstFilePath);
        lstFile.Directory.Create();
        WriteAllLinesNoExtraNewline(lstFile.FullName, lines);
    }

    private int GetNumberInput()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
            return 0;
        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
            return 1;
        if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
            return 2;
        if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
            return 3;
        if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
            return 4;
        if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5))
            return 5;
        if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6))
            return 6;
        if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7))
            return 7;
        if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8))
            return 8;
        if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9))
            return 9;
        return -1;
    }

    private int[] DistractorProblem()
    {
        return new int[] { UnityEngine.Random.Range(1, 9), UnityEngine.Random.Range(1, 9), UnityEngine.Random.Range(1, 9) };
    }

    private static void IncrementWordsSeen()
    {
        wordsSeen++;
        SaveState();
    }

    public static void SaveState()
    {
        string filePath = SessionFilePath(session, UnityEPL.GetParticipants()[0]);
        string[] lines = new string[currentSettings.numberOfLists * currentSettings.wordsPerList + 3];
        lines[0] = session.ToString();
        lines[1] = wordsSeen.ToString();
        lines[2] = (currentSettings.numberOfLists * currentSettings.wordsPerList).ToString();
        if (words == null)
            throw new UnityException("I can't save the state because a word list has not yet been generated");
        int i = 3;
        foreach (IronPython.Runtime.PythonDictionary word in words)
        {
            foreach (string key in word.Keys)
            {
                string value_string = word[key] == null ? "" : word[key].ToString();
                lines[i] = lines[i] + key + ":" + value_string + ";";
            }
            i++;
        }
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
        System.IO.File.WriteAllLines(filePath, lines);
    }

    public static string SessionFilePath(int sessionNumber, string participantName)
    {
        string filePath = ParticipantFolderPath(participantName);
        filePath = System.IO.Path.Combine(filePath, sessionNumber.ToString() + ".session");
        return filePath;
    }

    public static string ParticipantFolderPath(string participantName)
    {
        return System.IO.Path.Combine(CurrentExperimentFolderPath(), participantName);
    }

    public static string CurrentExperimentFolderPath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, UnityEPL.GetExperimentName());
    }

    public static bool SessionComplete(int sessionNumber, string participantName)
    {
        string sessionFilePath = EditableExperiment.SessionFilePath(sessionNumber, participantName);
        if (!System.IO.File.Exists(sessionFilePath))
            return false;
        string[] loadedState = System.IO.File.ReadAllLines(sessionFilePath);
        int wordsSeenInFile = int.Parse(loadedState[1]);
        int wordCount = int.Parse(loadedState[2]);
        return wordsSeenInFile >= wordCount;
    }

    public static void ConfigureExperiment(ushort newWordsSeen, ushort newSessionNumber, IronPython.Runtime.List newWords = null)
    {
        wordsSeen = newWordsSeen;
        session = newSessionNumber;
        currentSettings = FRExperimentSettings.GetSettingsByName(UnityEPL.GetExperimentName());
        bool isEvenNumberSession = newSessionNumber % 2 == 0;
        bool isTwoParter = currentSettings.isTwoParter;
        if (words == null) {
            Debug.Log("Setting Words");
            SetWords(currentSettings.wordListGenerator.GenerateListsAndWriteWordpool(currentSettings.numberOfLists, currentSettings.wordsPerList, currentSettings.isCategoryPool, isTwoParter, isEvenNumberSession, UnityEPL.GetParticipants()[0], currentSettings.wordpoolFilename));
            Debug.Log("Words were set.");
        }
        SaveState();
        Debug.Log("State saved.");
    }

    private static void SetWords(IronPython.Runtime.List newWords)
    {
        List<IronPython.Runtime.PythonDictionary> dotNetWords = new List<IronPython.Runtime.PythonDictionary>();
        foreach (IronPython.Runtime.PythonDictionary word in newWords)
            dotNetWords.Add(word);
        SetWords(dotNetWords);
    }

    private static void SetWords(List<IronPython.Runtime.PythonDictionary> newWords)
    {
        words = newWords;
    }

    //thanks Virtlink from stackoverflow
    protected static void WriteAllLinesNoExtraNewline(string path, params string[] lines)
    {
        if (path == null)
            throw new UnityException("path argument should not be null");
        if (lines == null)
            throw new UnityException("lines argument should not be null");

        using (var stream = System.IO.File.OpenWrite(path))
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream))
            {
                if (lines.Length > 0)
                {
                    for (int i = 0; i < lines.Length - 1; i++)
                    {
                        writer.WriteLine(lines[i]);
                    }
                    writer.Write(lines[lines.Length - 1]);
                }
            }
        }
    }
}
