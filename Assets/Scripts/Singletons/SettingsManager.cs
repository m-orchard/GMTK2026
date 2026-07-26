using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : Singleton<SettingsManager>
{
    public bool tutorialEnabled = true;

    public void ToggleTutorial() {
        tutorialEnabled = !tutorialEnabled;
    }
}
