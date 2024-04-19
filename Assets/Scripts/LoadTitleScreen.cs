using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class <c>LoadTitleScreen</c> is a class that loads the title screen.
/// </summary>
public class LoadTitleScreen : MonoBehaviour
{
    // Start is called before the first frame update
    public void LoadTitle() {
        FilePathClass.filePath = "";
        SceneManager.LoadScene(0);
    }
}
