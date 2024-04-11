using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PickFile : MonoBehaviour
{
    public void OpenFileDialog() {
        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", "txt", false);
        if (paths.Length > 0) {
            FilePathClass.filePath = new System.Uri(paths[0]).AbsolutePath;
            Debug.Log(FilePathClass.filePath);
            //Load main animation
            SceneManager.LoadScene(1);
        }
    }
}
