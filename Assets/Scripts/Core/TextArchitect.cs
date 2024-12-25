using System.Collections;
using UnityEngine;
using TMPro;

// This class is responsible for dynamically building text with effects like typewriter, instant display, or fade.
public class TextArchitect
{
    private TextMeshProUGUI tmpro_ui; // UI-based TextMeshPro reference.
    private TextMeshPro tmpro_world; // World-space TextMeshPro reference

    // Property to return the active TextMeshPro object (UI or world)
    public TMP_Text tmpro => tmpro_ui != null ? tmpro_ui : tmpro_world;

    // Properties for managing text content
    public string currentText => tmpro.text;
    public string targetText { get; private set; } = "";
    public string preText { get; private set; } = "";
    private int preTextLength = 0;

    public string fullTargetText => preText + targetText;

    // Enum defining the available build methods for text animation
    public enum BuildMethod { instant, typewriter, fade}
    public BuildMethod buildMethod = BuildMethod.typewriter;
    public Color textColor {get {return tmpro.color; } set {tmpro.color = value; }}

    // Speed-related properties and calculations
    public float speed {get { return baseSpeed +speedMultiplier;} set { speedMultiplier = value; }}

    private const float baseSpeed = 1; // Base speed for text animation
    private float speedMultiplier = 1; // Multiplier for adjusting speed

    // Determins how many characters to reveal per cycle based on speed
    public int charactersPerCycle { get { return speed <= 2f ? characterMultiplier : speed <= 2.5f ? characterMultiplier * 2 : characterMultiplier * 3; }}
    private int characterMultiplier = 1; // Base number of characters per cycle

    public bool hurryUp = false; // If true, speeds up the build process.

    // Constructor for UI-based TextMeshPro
    public TextArchitect(TextMeshProUGUI tmpro_ui)
    {
        this.tmpro_ui = tmpro_ui;
    }

    // Constructor for world-space TextMeshPro
    public TextArchitect(TextMeshPro tmpro_world)
    {
        this.tmpro_world = tmpro_world;
    }

    // Starts building the specified text
    public Coroutine Build(string text)
    {
        preText = ""; // Clear previously displayed text
        targetText = text; // Set new target text

        Stop(); // Stop any ongoing build process

        // Start building text as a coroutine
        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess; 
    }
    
    // Appends new text to the currently displayed text
    public Coroutine Append(string text)
    {
        preText = tmpro.text; // Retain the currently displayed text.
        targetText = text; // Set the new text to append

        Stop();

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess; 
    }

    private Coroutine buildProcess = null; // Stores the active coroutine
    public bool isBuilding => buildProcess != null; // Indicates if a build process is ongoing

    // Stops any active build process
    public void Stop()
    {
        if(!isBuilding)
            return;
        
        tmpro.StopCoroutine(buildProcess); // Stop the coroutine
        buildProcess = null; // Clear the reference

    }

    // Main coroutine for managing the build process
    IEnumerator Building()
    {
        Prepare(); // Prepare the text for the selected build method

        switch(buildMethod)
        {
            case BuildMethod.typewriter:
                yield return Build_Typewriter(); // Execute typerwriter build
                break;
            case BuildMethod.fade:
                break;
                yield return Build_Fade(); // Execute fade build
        }
    }

    // Handles completion of the build process
    private void OnComplete()
    {
        buildProcess = null; // Clear the active coroutine reference
        hurryUp = false; // Reset hurry-up flag.
    }

    // Forces the build process to complete instantly.
    public void ForceComplete()
    {
        switch(buildMethod)
        {
            case BuildMethod.typewriter:
                tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount; // Reveal all characters instantly
                break;
            case BuildMethod.fade:
                break;
        }

        Stop(); // Stop the active coroutine.
        OnComplete(); // Mark the process as complete
    }

    // Prepares the text object for building based on the build method
    private void Prepare()
    {
        switch(buildMethod)
        {
            case BuildMethod.instant:
                Prepare_Instant();
                break;
            case BuildMethod.typewriter:
                Prepare_Typewriter();
                break;
            case BuildMethod.fade:
                Prepare_Fade();
                break;
        }
    }

    // Prepares for instant text display
    private void Prepare_Instant()
    {
        tmpro.color = tmpro.color;
        tmpro.text = fullTargetText;
        tmpro.ForceMeshUpdate();
        tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
    }

    private void Prepare_Typewriter()
    {
        tmpro.color = tmpro.color;
        tmpro.maxVisibleCharacters = 0;
        tmpro.text = preText;

        if (preText != "")
        {
            tmpro.ForceMeshUpdate();
            tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
        }

        tmpro.text += targetText;
        tmpro.ForceMeshUpdate();
    }

    private void Prepare_Fade()
    {
        
    }

    private IEnumerator Build_Typewriter()
    {
        while(tmpro.maxVisibleCharacters < tmpro.textInfo.characterCount)
        {
            tmpro.maxVisibleCharacters += hurryUp ? charactersPerCycle * 5 : charactersPerCycle;
            yield return new WaitForSeconds(0.015f / speed); 
        }
    }

    private IEnumerator Build_Fade()
    {
        yield return null;
    }
}
