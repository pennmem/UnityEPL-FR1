using UnityEngine;
using System.Collections;

public enum DistractionType {
    Math,
    Fixation,
}

/// <summary>
/// This struct contains all the settings required to run an FR experiment.
/// 
/// The behavior of the experiment will automatically adjust according to these settings.
/// </summary>
public struct ExperimentSettings
{
    public const string taskVersion = "1.6.13";

    public WordListGenerator wordListGenerator; //how the words for this experiment will be created and organized.  for a full list of parameters and what they do, see the comments for WordListGenerator in WordListGenerator.cs
    public string experimentName; //the name of the experiment.  this will be displayed to the user and sent to ramulator.
    public string version; //the version of the experiment.  for logging purposes.
    public int numberOfLists; //how many lists to display to the participant.
    public int wordsPerList; //how many words in each list.
    public int microphoneTestLength; //how long to record for in the microphone test
    public float wordPresentationLength; //how long each word will be on the screen.  normally 1.6 seconds.
    public float minISI; //the minimum length to wait in between words.
    public float maxISI; //the maximum length to wait in between words.  a random value between min and max will be chosen with uniform distribution.
    public float distractionLength; //how many seconds minimum should the distractor period be.  when this time expires, the distractor period will end after the current problem is submitted.
    public float answerConfirmationTime; //how long to display feedback to the participant for after they submit a distractor answer.  normally 0, as current experiments do not display visual feedback at all.
    public float recallLength; //how many second to record vocal recall responses for after the distraction period.
    public float minOrientationStimulusLength; //minimum amount of time the "+" appears for before words.
    public float maxOrientationStimulusLength; //maximum amount of time the "+" appears for before words. a random value between min and max will be chosen with uniform distribution.
    public float minPauseBeforeRecall; //minimum amount of time to wait after the last distractor is entered before recall begins.
    public float maxPauseBeforeRecall; //maximum amount of time to wait after the last distractor is entered before recall begins. a random value between min and max will be chosen with uniform distribution.
    public float recallTextDisplayLength; //how long to display "****" for at the beginning of the recall period.
    public bool useRamulator; //whether or not the task should try to connect to and send messages to ramulator.
    public bool useElemem; //whether or not the task should try to connect to and send messages to elemem.
    public string stimMode; //what type of stim elemem will use
    public uint clDuration; //duration in ms to collect data for normalization or classification
    public bool isTwoParter; //whether or not the experiment should divide the word pool in two and alternative halves between sessions.
    public bool isCategoryPool; //whether or not the catFR wordpool is used.
    public bool useSessionListSelection; //whether or not the list to begin from can be chosen in the start screen.
    public int listGroupSize; //if > 0, insert pauseBetweenGroups record-only pause between this many lists.
    public float pauseBetweenGroups; //seconds to pause between groupings of listGroupSize lists.
    public string wordpoolFilename; //if set, override wordpool with this one.
    public DistractionType distrationType; //Distraction type for the experiment
}

public static class FRExperimentSettings
{
    private static ExperimentSettings[] activeExperimentSettings = { GetFR1Settings(),                              // IF YOU WANT TO ADD OR REMOVE AN EXPERIMENT
                                                                     GetCatFR1Settings(),                          // CREATE A NEW SETTINGS FETCHER IF NEEDED
                                                                     GetFR5Settings(),                              // AND THEN ADD OR REMOVE THE EXPERIMENT
                                                                     GetCatFR5Settings(),                           // SETTINGS FROM THIS ARRAY.
                                                                     GetFR6Settings(),
                                                                     GetCatFR6Settings(),
                                                                     GetCPSSettings(),
                                                                     GetIFR1Settings(),
                                                                     GetICatFR1Settings(),
                                                                     GetIFR5Settings(),
                                                                     GetICatFR5Settings(),
                                                                     GetIFR6Settings(),
                                                                     GetICatFR6Settings(),
                                                                     GetPS5Settings(),
                                                                     GetCatPS5Settings(),
                                                                     GetPS4Settings(),
                                                                     GetCatPS4Settings(),
                                                                     GetTICLFRSettings(),
                                                                     GetTICLCatFRSettings(),
                                                                     GetTICCLSSettings(),
                                                                     GetTICCLSbSettings(),
                                                                     GetTestFR1Settings() };

    /// <summary>
    /// Gets the FR 1 settings.
    /// 
    /// //Short FR1 & CatFR1 Task Design//
    /// Each session includes one practice list and 12 experimental lists
    /// Word lists are constructed from the word pool according to an algorithm that generates unique lists with mean pairwise LSA similarity within each list of approximately 0.2.
    /// Half of the words in the pool will be seen once in Session 1, and the other half will beseen in Session 2.
    /// 
    /// 
    /// The following SESSION DESIGN specifications apply to all FR experiments:
    ///     A session begins by with an introductory video containing the task instructions
    ///     A 10 second countdown video precedes the encoding phase of each list
    ///     Estimated time to complete each session is approximately 25 min
    /// //LIST ENCODING PHASE//
    ///     Each list starts with a orienting stimulus displayed for 1000–1400 ms (uniformly distributed)
    ///     12 words are then presented for 1600 ms each, each word preceded by an inter-stimulus interval of 750-1000ms.  There is no break after the last word has been displayed for 1600ms- distractors immediate appear on the screen.
    /// //DISTRACTOR PHASE//
    ///     Arithmetic problems are presented for at least 20 s
    ///     Problems are presented in the form A+B+C = ?, where A/B/C are random integers 1–9
    ///     All keystrokes are recorded until the subject presses enter
    ///     After each arithmetic problem, if the 20 s time limit has been exceeded, a tone is presented indicating the end of the distractor phase and the beginning of the retrieval phase.
    ///     There is then a 1000-1400ms pause before recall.
    /// //RECALL PHASE//
    ///     Audio is recorded via the microphone for 30s
    ///     After 30 seconds a tone is presented, indicating the end of the retrieval phase and the end of that list.
    /// 
    /// //Free Recall Word Pool and List Creation//
    ///     The word pool is adapted from the word pool used in the pyFR task, which has been run in a set of over 150 intracranial patients (Burke et al., 2013; Long et al., 2014).
    ///     A set of 288 words was chosen from the pyFR word pool, based on the recall performance of a separate set of participants who completed a large-scale study of free recall.
    ///     Recall performance in this large-scale task was modeled to estimate the effect of each individual word on recall, removing influences of serial position and frequency, concreteness, imageability and length.
    ///     Estimates for the words were used to identify the words at the top and bottom of the distribution for removal.
    /// </summary>
    /// <returns>The FR 1 settings.</returns>
    public static ExperimentSettings GetFR1Settings()
    {
        ExperimentSettings FR1Settings = new ExperimentSettings();
        FR1Settings.experimentName = "FR1";
        FR1Settings.version = "1.1";
        FR1Settings.wordListGenerator = new FRListGenerator(0, 13, 0, 0, 0, 0, 0);
        FR1Settings.isCategoryPool = false;
        FR1Settings.numberOfLists = 13;
        FR1Settings.wordsPerList = 12;
        FR1Settings.wordPresentationLength = 1.6f;
        FR1Settings.minISI = 0.75f;
        FR1Settings.maxISI = 1f;
        FR1Settings.distractionLength = 20f;
        FR1Settings.answerConfirmationTime = 0f;
        FR1Settings.recallLength = 30f;
        FR1Settings.microphoneTestLength = 5;
        FR1Settings.minOrientationStimulusLength = 1f;
        FR1Settings.maxOrientationStimulusLength = 1.4f;
        FR1Settings.minPauseBeforeRecall = 1f;
        FR1Settings.maxPauseBeforeRecall = 1.4f;
        FR1Settings.recallTextDisplayLength = 1f;
        FR1Settings.useRamulator = true;
        FR1Settings.useElemem = false;
        FR1Settings.stimMode = "none";
        FR1Settings.isTwoParter = true;
        FR1Settings.useSessionListSelection = true;
        FR1Settings.distrationType = DistractionType.Math;

        FR1Settings.listGroupSize = 0;
        FR1Settings.pauseBetweenGroups = 0;
        FR1Settings.wordpoolFilename = "";

        return FR1Settings;
    }

    /// <summary>
    /// Gets the cat FR 1 settings.
    /// 
    /// The following information applies to all CatFR experiments:
    /// //Categorized Free Recall Word Pool and List Creation//
    ///     The word pool was generated via a series of pilot studies conducted on Amazon Mechanical Turk. 
    ///     First, a set of 40 participants generated lists of exemplars from 28 categories.
    ///     The resulting exemplars were sorted according to the number of participants that produced that specific exemplar.
    ///     The top 25 exemplars from each category were then selected for the second pilot study (# of participants that produced each exemplar in the top 25 set, M = 19; range = 7-40).
    ///     In the second pilot study, a set of 45 participants rated (on a 1-7 scale) each of the 25 items from the 28 categories in terms of how ‘typical’ the item is of all members of the category.
    ///     From these ratings, the 12 most prototypical items from each category were selected. The three categories with the lowest mean prototypicality across the 12 exemplars were discarded, leaving 25 categories for inclusion in the study.
    ///     This process ensured that the exemplars that were chosen for the word pool were drawn only from highly prototypical members of each category (mean prototypicality rating across the final set = 6.3).
    ///     The 25 lists for the first session were constructed by randomly selecting three categories from the set and then randomly selecting four exemplars from each of those categories.
    ///     Category members were presented in pairs, with the order determined such that two pairs of items from the same category were never presented consecutively.
    ///     Lists will also have the constraint that half of the words from each category will be presented in the first half of the list, and the other half of the words will be presented in the second half of the list.
    /// </summary>
    /// <returns>The cat FR 1 settings.</returns>
    public static ExperimentSettings GetCatFR1Settings()
	{
        ExperimentSettings CatFR1Settings = GetFR1Settings();
        CatFR1Settings.experimentName = "CatFR1";
        CatFR1Settings.isCategoryPool = true;
        return CatFR1Settings;
    }

    /// <summary>
    /// Gets the FR 5 settings.
    /// 
    /// FR5 is a stim experiment.  Lists are handled as in FR1.  No stimulation is applied in list 0 (practice list) or lists 1-3 (baseline lists)/
    /// 
    /// The remaining 22 lists are randomly interleaved: 11 stim and 11 no stim.
    /// </summary>
    /// <returns>The FR 5 settings.</returns>
    public static ExperimentSettings GetFR5Settings()
    {
        ExperimentSettings FR5Settings = new ExperimentSettings();

        FR5Settings.experimentName = "FR5";
        FR5Settings.version = "5.1";
        FR5Settings.wordListGenerator = new FRListGenerator(16, 6, 4, 16, 0, 0, 1);
        FR5Settings.isCategoryPool = false;
        FR5Settings.numberOfLists = 26;
        FR5Settings.wordsPerList = 12;
        FR5Settings.wordPresentationLength = 1.6f;
        FR5Settings.minISI = 0.75f;
        FR5Settings.maxISI = 1f;
        FR5Settings.distractionLength = 20f;
        FR5Settings.answerConfirmationTime = 0f;
        FR5Settings.recallLength = 30f;
        FR5Settings.microphoneTestLength = 5;
        FR5Settings.minOrientationStimulusLength = 1f;
        FR5Settings.maxOrientationStimulusLength = 1.4f;
        FR5Settings.minPauseBeforeRecall = 1f;
        FR5Settings.maxPauseBeforeRecall = 1.4f;
        FR5Settings.recallTextDisplayLength = 1f;
        FR5Settings.useRamulator = true;
        FR5Settings.useElemem = false;
        FR5Settings.stimMode = "closed";
        FR5Settings.clDuration = 1000;
        FR5Settings.isTwoParter = false;
        FR5Settings.useSessionListSelection = false;

        FR5Settings.listGroupSize = 0;
        FR5Settings.pauseBetweenGroups = 0;
        FR5Settings.wordpoolFilename = "";
        return FR5Settings;
    }

    /// <summary>
    /// Gets the CatFR 5 settings.
    /// 
    /// CatFR5 is identical to FR5, but uses the category word pool described above.
    /// </summary>
    /// <returns>The CatFR 5 settings.</returns>
    public static ExperimentSettings GetCatFR5Settings()
    {
        ExperimentSettings CatFR5Settings = GetFR5Settings();
        CatFR5Settings.experimentName = "CatFR5";
        CatFR5Settings.isCategoryPool = true;
        return CatFR5Settings;
    }

    /// <summary>
    /// Gets the FR 6 settings.
    /// 
    /// FR6 is a stim experiment.  Lists are handled as in FR1.  No stimulation is applied in list 0 (practice list) or lists 1-3 (baseline lists)/
    /// 
    /// The remaining 22 lists are randomly interleaved: Five stimming on one channel, five stimming on another, six stimming on both channels, and six with no stim.
    /// </summary>
    /// <returns>The FR 6 settings.</returns>
    public static ExperimentSettings GetFR6Settings()
    {
        ExperimentSettings FR6Settings = new ExperimentSettings();
        FR6Settings.experimentName = "FR6";
        FR6Settings.version = "6.1";
        FR6Settings.wordListGenerator = new FRListGenerator(16, 6, 4, 5, 5, 6, 1);
        FR6Settings.isCategoryPool = false;
        FR6Settings.numberOfLists = 26;
        FR6Settings.wordsPerList = 12;
        FR6Settings.wordPresentationLength = 1.6f;
        FR6Settings.minISI = 0.75f;
        FR6Settings.maxISI = 1f;
        FR6Settings.distractionLength = 20f;
        FR6Settings.answerConfirmationTime = 0f;
        FR6Settings.recallLength = 30f;
        FR6Settings.microphoneTestLength = 5;
        FR6Settings.minOrientationStimulusLength = 1f;
        FR6Settings.maxOrientationStimulusLength = 1.4f;
        FR6Settings.minPauseBeforeRecall = 1f;
        FR6Settings.maxPauseBeforeRecall = 1.4f;
        FR6Settings.recallTextDisplayLength = 1f;
        FR6Settings.useRamulator = true;
        FR6Settings.useElemem = false;
        FR6Settings.stimMode = "closed";
        FR6Settings.clDuration = 1000; 
        FR6Settings.isTwoParter = false;
        FR6Settings.useSessionListSelection = false;

        FR6Settings.listGroupSize = 0;
        FR6Settings.pauseBetweenGroups = 0;
        FR6Settings.wordpoolFilename = "";

        return FR6Settings;
    }

    /// <summary>
    /// Gets the CatFR 6 settings.
    /// 
    /// CatFR6 is identical to FR6, but uses the category word pool described above.
    /// </summary>
    /// <returns>The CatFR 6 settings.</returns>
    public static ExperimentSettings GetCatFR6Settings()
    {
        ExperimentSettings CatFR6Settings = GetFR6Settings();
        CatFR6Settings.experimentName = "CatFR6";
        CatFR6Settings.isCategoryPool = true;
        return CatFR6Settings;
    }

    /// <summary>
    /// Gets the IFR 1 settings.
    /// 
    /// IFR1 is identical to FR1, except for the number of lists
    /// and the distraction is a fixation instead of math problems
    /// </summary>
    /// <returns>The IFR 1 settings.</returns>
    public static ExperimentSettings GetIFR1Settings() {
        ExperimentSettings IFR1Settings = GetFR1Settings();
        IFR1Settings.experimentName = "IFR1";
        IFR1Settings.distractionLength = 10f;
        IFR1Settings.isTwoParter = false;
        IFR1Settings.numberOfLists = 26;
        IFR1Settings.wordListGenerator = new FRListGenerator(0, 26, 0, 0, 0, 0, 0);
        IFR1Settings.distrationType = DistractionType.Fixation;

        return IFR1Settings;
    }

    /// <summary>
    /// Gets the ICatFR 1 settings.
    /// 
    /// ICatFR1 is identical to CatFR1, except for the number of lists
    /// and the distraction is a fixation instead of math problems
    /// </summary>
    /// <returns>The ICatFR 1 settings.</returns>
    public static ExperimentSettings GetICatFR1Settings() {
        ExperimentSettings ICatFR1Settings = GetCatFR1Settings();
        ICatFR1Settings.experimentName = "ICatFR1";
        ICatFR1Settings.distractionLength = 10f;
        ICatFR1Settings.isTwoParter = false;
        ICatFR1Settings.numberOfLists = 26;
        ICatFR1Settings.wordListGenerator = new FRListGenerator(0, 26, 0, 0, 0, 0, 0);
        ICatFR1Settings.distrationType = DistractionType.Fixation;

        return ICatFR1Settings;
    }

    /// <summary>
    /// Gets the IFR 5 settings.
    /// 
    /// IFR5 is identical to FR5, except the distraction is a
    /// fixation instead of math problems
    /// </summary>
    /// <returns>The IFR 5 settings.</returns>
    public static ExperimentSettings GetIFR5Settings() {
        ExperimentSettings IFR5Settings = GetFR5Settings();
        IFR5Settings.experimentName = "IFR5";
        IFR5Settings.distractionLength = 10f;
        IFR5Settings.distrationType = DistractionType.Fixation;

        return IFR5Settings;
    }

    /// <summary>
    /// Gets the ICatFR 5 settings.
    /// 
    /// ICatFR5 is identical to CatFR5, except the distraction is a
    /// fixation instead of math problems
    /// </summary>
    /// <returns>The ICatFR 5 settings.</returns>
    public static ExperimentSettings GetICatFR5Settings() {
        ExperimentSettings ICatFR5Settings = GetCatFR5Settings();
        ICatFR5Settings.experimentName = "ICatFR5";
        ICatFR5Settings.distractionLength = 10f;
        ICatFR5Settings.distrationType = DistractionType.Fixation;

        return ICatFR5Settings;
    }

    /// <summary>
    /// Gets the IFR 6 settings.
    /// 
    /// IFR6 is identical to FR6, except the distraction is a
    /// fixation instead of math problems
    /// </summary>
    /// <returns>The IFR 6 settings.</returns>
    public static ExperimentSettings GetIFR6Settings() {
        ExperimentSettings IFR6Settings = GetFR6Settings();
        IFR6Settings.experimentName = "IFR6";
        IFR6Settings.distractionLength = 10f;
        IFR6Settings.distrationType = DistractionType.Fixation;

        return IFR6Settings;
    }

    /// <summary>
    /// Gets the ICatFR 6 settings.
    /// 
    /// ICatFR6 is identical to CatFR6, except the distraction is a
    /// fixation instead of math problems
    /// </summary>
    /// <returns>The ICatFR 6 settings.</returns>
    public static ExperimentSettings GetICatFR6Settings() {
        ExperimentSettings ICatFR6Settings = GetCatFR6Settings();
        ICatFR6Settings.experimentName = "ICatFR6";
        ICatFR6Settings.distractionLength = 10f;
        ICatFR6Settings.distrationType = DistractionType.Fixation;

        return ICatFR6Settings;
    }

    /// <summary>
    /// Gets the PS5 settings.
    /// 
    /// PS5 is the same as FR1, with the following exceptions:
    ///     All lists (except the practice list) are stim lists
    ///     Stimulation happens on three amplitudes.  Amplitudes are the same within a list.  The exact amplitudes are chosen by ramulator.  The task is responsible for sending an index (0, 1, or 2).
    ///     Each amplitude is used for four separate lists.
    /// </summary>
    /// <returns>The PS5 settings.</returns>
    public static ExperimentSettings GetPS5Settings()
    {
        ExperimentSettings PS5Settings = GetFR1Settings();
        PS5Settings.wordListGenerator = new FRListGenerator(12, 0, 1, 12, 0, 0, 3);
        PS5Settings.experimentName = "PS5_FR";
        return PS5Settings;
    }

    /// <summary>
    /// Gets the CatPS5 settings.
    /// 
    /// CatPS5 is the same as PS5, but uses the cat wordpool.
    /// </summary>
    /// <returns>The PS5 settings.</returns>
    public static ExperimentSettings GetCatPS5Settings()
    {
        ExperimentSettings CatPS5Settings = GetPS5Settings();
        CatPS5Settings.experimentName = "PS5_CatFR";
        CatPS5Settings.isCategoryPool = true;
        return CatPS5Settings;
    }

        /// <summary>
    /// Gets the PS4 settings.
    /// 
    /// PS4 is 26 lists, 4 baseline and then 22 PS.  No other special parameters are required.
    /// </summary>
    /// <returns>The PS4 settings.</returns>
    public static ExperimentSettings GetPS4Settings()
    {
        ExperimentSettings PS4Settings = GetFR6Settings();
        PS4Settings.wordListGenerator = new FRListGenerator(0, 0, 4, 0, 0, 0, 1, NEW_PS_LIST_COUNT: 22);
        PS4Settings.experimentName = "PS4_FR5";
        PS4Settings.version = "4.2";
        return PS4Settings;
    }

    /// <summary>
    /// Gets the CatPS4 settings.
    /// 
    /// CatPS4 is the same as P45, but uses the cat wordpool.
    /// </summary>
    /// <returns>The PS5 settings.</returns>
    public static ExperimentSettings GetCatPS4Settings()
    {
        ExperimentSettings CatPS4Settings = GetPS4Settings();
        CatPS4Settings.experimentName = "PS4_CatFR5";
        CatPS4Settings.isCategoryPool = true;
        return CatPS4Settings;
    }

    /// <summary>
    /// Gets the TICL_FR settings.
    /// 
    /// TICL_FR consist of 25 EMBEDDED lists.
    /// </summary>
    /// <returns>The PS5 settings.</returns>
    public static ExperimentSettings GetTICLFRSettings()
    {
        ExperimentSettings TICLFRSettings = GetFR6Settings();
        TICLFRSettings.wordListGenerator = new FRListGenerator(11, 11, 4, 11, 0, 0, 1);
        TICLFRSettings.experimentName = "TICL_FR";
        return TICLFRSettings;
    }

    /// <summary>
    /// Gets the TICL_CatFR settings.
    /// 
    /// This is the same as TICL_FR, but uses the cat wordpool.
    /// </summary>
    /// <returns>The PS5 settings.</returns>
    public static ExperimentSettings GetTICLCatFRSettings()
    {
        ExperimentSettings TICLCatFRSettings = GetTICLFRSettings();
        TICLCatFRSettings.experimentName = "TICL_CatFR";
        TICLCatFRSettings.isCategoryPool = true;
        return TICLCatFRSettings;
    }

    /// <summary>
    /// Gets the TICCLS settings.
    /// 
    /// This is the same as TICL_CatFR, but with a different word list.
    /// </summary>
    /// <returns>The PS5 settings.</returns>
    public static ExperimentSettings GetTICCLSSettings()
    {
        ExperimentSettings TICCLSSettings = GetTICLCatFRSettings();
        TICCLSSettings.experimentName = "TICCLS";
        TICCLSSettings.version = "1.0";
        TICCLSSettings.numberOfLists = 26;
        TICCLSSettings.wordListGenerator = new FRListGenerator(15, 0, 6, 15, 0, 0, 1, 0, 5);
        //TICCLSSettings.wordpoolFilename = "ram_categorized_300_en";
        TICCLSSettings.listGroupSize = 5;
        TICCLSSettings.pauseBetweenGroups = 50*60;
        return TICCLSSettings;
    }

    /// <summary>
    /// Gets the TICCLSb settings.
    /// 
    /// This is the same as TICCLS, but the first delay and subsequent list
    /// are the sham periods, rather than the last.
    /// </summary>
    /// <returns>The PS5 settings.</returns>
    public static ExperimentSettings GetTICCLSbSettings()
    {
        ExperimentSettings TICCLSbSettings = GetTICCLSSettings();
        TICCLSbSettings.wordListGenerator = new FRListGenerator(15, 0, 11, 15, 0, 0, 1, 0, 0);
        TICCLSbSettings.experimentName = "TICCLSb";
        return TICCLSbSettings;
    }

    /// <summary>
    /// Gets the test ps5 settings.
    /// </summary>
    /// <returns>The test PS5 settings.</returns>
    public static ExperimentSettings GetTestPS5Settings()
    {
        ExperimentSettings testPS5Settings = GetPS5Settings();
        testPS5Settings.experimentName = "PS5_test";
        testPS5Settings.useRamulator = false;
        return testPS5Settings;
    }

    /// <summary>
    /// Gets the test FR 1 settings.
    /// 
    /// This is used for rapid visual confirmation of experiment behavior.
    /// </summary>
    /// <returns>The test FR 1 settings.</returns>
    public static ExperimentSettings GetTestFR1Settings()
    {
        ExperimentSettings testFR1Settings = GetFR1Settings();
        testFR1Settings.experimentName = "FR1_test";
        //testFR1Settings.countdownTick = 0.01f;
        //testFR1Settings.wordPresentationLength = 0.01f;
        //testFR1Settings.minISI = 0.005f;
        //testFR1Settings.maxISI = 0.01f;
        //testFR1Settings.distractionLength = 0.1f;
        //testFR1Settings.recallLength = 0.1f;
        //testFR1Settings.minOrientationStimulusLength = 0.05f;
        //testFR1Settings.maxOrientationStimulusLength = 0.1f;
        //testFR1Settings.minPauseBeforeRecall = 0.1f;
        //testFR1Settings.maxPauseBeforeRecall = 0.2f;
        testFR1Settings.useRamulator = false;
        return testFR1Settings;
    }

    /// <summary>
    /// Gets the test cat FR 1 settings.
    /// 
    /// This is used for rapid visual confirmation of experiment behavior.
    /// </summary>
    /// <returns>The test cat FR 1 settings.</returns>
    public static ExperimentSettings GetTestCatFR1Settings()
    {
        ExperimentSettings testCatFR1Settings = GetTestFR1Settings();
        testCatFR1Settings.experimentName = "CatFR1_test";
        testCatFR1Settings.isCategoryPool = true;
        return testCatFR1Settings;
    }

    /// <summary>
    /// Gets the test FR 6 settings.
    /// 
    /// This is used for rapid visual confirmation of experiment behavior.
    /// </summary>
    /// <returns>The test FR 6 settings.</returns>
    public static ExperimentSettings GetTestFR6Settings()
    {
        ExperimentSettings testFR6Settings = GetFR6Settings();
        testFR6Settings.experimentName = "FR6_test";
        //testFR6Settings.countdownTick = 0.01f;
        //testFR6Settings.wordPresentationLength = 0.01f;
        //testFR6Settings.minISI = 0.005f;
        //testFR6Settings.maxISI = 0.01f;
        //testFR6Settings.distractionLength = 0.1f;
        //testFR6Settings.recallLength = 0.1f;
        //testFR6Settings.minOrientationStimulusLength = 0.05f;
        //testFR6Settings.maxOrientationStimulusLength = 0.1f;
        //testFR6Settings.minPauseBeforeRecall = 0.1f;
        //testFR6Settings.maxPauseBeforeRecall = 0.2f;
        testFR6Settings.useRamulator = false;
        return testFR6Settings;
    }

    /// <summary>
    /// Gets the test cat FR 6 settings.
    /// 
    /// This is used for rapid visual confirmation of experiment behavior.
    /// </summary>
    /// <returns>The test cat FR 6 settings.</returns>
    public static ExperimentSettings GetTestCatFR6Settings()
    {
        ExperimentSettings testCatFR6Settings = GetTestFR6Settings();
        testCatFR6Settings.experimentName = "CatFR6_test";
        testCatFR6Settings.isCategoryPool = true;
        return testCatFR6Settings;
    }

    public static ExperimentSettings GetCPSSettings()
    {
        ExperimentSettings CPSSettings = GetFR1Settings();
        CPSSettings.experimentName = "CPS";
        CPSSettings.version = "1.0";
        CPSSettings.useRamulator = false;
        CPSSettings.useElemem = true;
        CPSSettings.stimMode = "closed";

        return CPSSettings;
    }


    /// <summary>
    /// Gets the experiment names.
    /// </summary>
    /// <returns>The experiment names as an array of strings.  These array elements will be presented to the user as the available selection of experiments.</returns>
    public static string[] GetExperimentNames()
    {
        string[] experimentNames = new string[activeExperimentSettings.Length];
        for (int i = 0; i < activeExperimentSettings.Length; i++)
        {
            experimentNames[i] = activeExperimentSettings[i].experimentName;
        }
        return experimentNames;
    }

    /// <summary>
    /// Takes a string name of an experiment.
    /// </summary>
    /// <returns>The ExperimentSettings associated with that name.</returns>
    /// <param name="name">Name.</param>
    public static ExperimentSettings GetSettingsByName(string name)
    {
        foreach (ExperimentSettings setting in activeExperimentSettings)
        {
            if (setting.experimentName.Equals(name))
                return setting;
        }
        throw new UnityException("No settings found with that name.");
    }

    /// <summary>
    /// Takes the name of an experiment.
    /// </summary>
    /// <returns>The name of the unity scene used to run that experiment.</returns>
    /// <param name="name">Name.</param>
    public static string ExperimentNameToExperimentScene(string name)
    {
        switch (name)
        {
            default:
                return "ram_fr";
        }
    }
}
