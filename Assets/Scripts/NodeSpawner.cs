using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using System.Linq;

public class NodeSpawner : MonoBehaviour
{

    public int leftNodeCount = 0;
    public int rightNodeCount = 0;
    public GameObject leftNodeParent;
    public GameObject rightNodeParent;
    public GameObject nodePrefab;
    public Color leftColor = Color.red;
    public Color rightColor = Color.blue;


    // Grabbed from https://www.youtube.com/watch?v=4URtDoKPu7M
    private Bounds GetBounds(GameObject gameObject) {
        Bounds bound = new Bounds(gameObject.transform.position, Vector3.zero);
        var rList = gameObject.GetComponentsInChildren(typeof(Renderer));
        foreach(Renderer r in rList) {
            bound.Encapsulate(r.bounds);
        }
        return bound;
    }
    
    // Also grabbed from https://www.youtube.com/watch?v=4URtDoKPu7M
    private void FitOnScreen() {
        Bounds bound = GetBounds(leftNodeParent);
        bound.Encapsulate(GetBounds(rightNodeParent));
        Vector3 boundSize = bound.size;
        float diagonal = Mathf.Sqrt((boundSize.x * boundSize.x) + (boundSize.y * boundSize.y) + (boundSize.z * boundSize.z));
        Camera.main.orthographicSize = (diagonal / 2.0f) / 1.5f;
        transform.position = bound.center;
    }
    
    
    void SpawnNodes(int nodeCount, GameObject parentObj, Color nodeColor, string[] nodeNames) {
        float startPosX = -(nodeCount / 2.0f) + 0.5f;
        for (int i = 0; i < nodeCount; i++) {
            // Set position based on parent's position
            Vector3 pos = new Vector3((parentObj.transform.position.x + startPosX + i), parentObj.transform.position.y, parentObj.transform.position.z);

            GameObject currentNode = Instantiate(nodePrefab, pos, parentObj.transform.rotation) as GameObject;
            currentNode.name = "Node" + nodeNames[i];

            SpriteRenderer sprite = currentNode.GetComponent<SpriteRenderer>();
            sprite.color = nodeColor;

            TextMeshProUGUI nodeText = currentNode.GetComponentInChildren<TextMeshProUGUI>();
            nodeText.text = nodeNames[i];

            // Ensure that this node is a child of the parent object
            currentNode.transform.parent = parentObj.transform;
        }
    }

    //parse the step and get the first and second set of the bipartite graph
    (string[], string[]) parseInitializeStep(string step) {
        // Remove "Initialize: " from the start of the string, then remove the parentheses
        string sets = step.Replace("Initialize: ", "").Replace("(", "").Replace(")", "");

        // Split the string into two parts on the space character
        string[] splitSets = sets.Split(' ');

        // Split each part on the "," character to get the individual numbers and letters
        string[] firstSet = splitSets[0].Split(',');
        string[] secondSet = splitSets[1].Split(',');

        Debug.Log("First Set: " + firstSet);
        Debug.Log("Second Set: " + secondSet);

        return (firstSet, secondSet);
    }

    void animateStep(string step) {
        //parse the step and animate it

        
        //the step is for intializing the bipartite graph
        if (step.StartsWith("Initialize:")){
            string[] firstSet;
            string[] secondSet;

            //parse the step and get the first and second set of the bipartite graph
            (firstSet, secondSet) = parseInitializeStep(step);

            Debug.Log("First Set: " + string.Join(",", firstSet));
            Debug.Log("Second Set: " + string.Join(",", secondSet));
            SpawnNodes(firstSet.Length, leftNodeParent, leftColor, firstSet);
            SpawnNodes(secondSet.Length, rightNodeParent, rightColor, secondSet);
        }

        //TODO: add more steps
    }

    // Start is called before the first frame update
    void Start()
    {
        //get file for the animation steps from its path
        string path = Path.Combine(Application.dataPath,"InputFiles", "InputText.txt");

        //read the file
        List<string> steps = File.ReadAllLines(path).ToList();

        //iterate through the steps and animate them
        foreach (string step in steps) {
            animateStep(step);
        }

        FitOnScreen();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
