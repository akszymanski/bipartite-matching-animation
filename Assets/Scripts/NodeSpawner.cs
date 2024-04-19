using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using System.Linq;

/// <summary>
/// Class <c>NodeSpawner</c> is responsible for animating the steps of the Hopcroft-Karp algorithm on a bipartite graph.
/// </summary>
public class NodeSpawner : MonoBehaviour
{
    /// <summary>
    /// The parent object for the left set of verticies
    /// </summary>
    public GameObject leftNodeParent;

    /// <summary>
    /// The parent object for the right set of verticies
    /// </summary>
    public GameObject rightNodeParent;

    /// <summary>
    /// The prefab for the nodes
    /// </summary>
    public GameObject nodePrefab;

    /// <summary>
    /// The color of the left set of verticies
    /// </summary>
    public Color leftColor = Color.red;

    /// <summary>
    /// The color of the right set of verticies
    /// </summary>
    public Color rightColor = Color.blue;

    /// <summary>
    /// The text object for the steps
    /// </summary>
    public TextMeshProUGUI stepText;

    /// <summary>
    /// The text object for the current step
    /// </summary>
    public TextMeshProUGUI currentStep;

    /// <summary>
    /// Dictionary to store the nodes
    /// </summary>
    public Dictionary<string, GameObject> nodes;

    // Amount of time between steps
    private float stepTimeDuration = 7.0f;

    // Amount of time it takes for edge color to change from current color to new color
    private float edgeColorChangeDuration = 0.5f;

    // Amount of time it takes for node to be highlighted
    private float nodeHighlightDuration = 0.5f;
    
    // Keeps track of the time for the animation
    private float time = 0;

    // Keeps track of the number of nodes in the graph
    private int numberOfNodes = 0;

    int lowestSpriteOrder = -1;

    /// <summary>
    /// This method initializes the bipartite graph and animates the steps of 
    /// the Hopcroft-Karp algorithm at the start.
    /// </summary>
    public void Start()
    {
        int lowestSpriteOrder = -1;

        //initialize the dictionary to store the nodes
        nodes = new Dictionary<string, GameObject>();

        //get file for the animation steps from its path
        string path = FilePathClass.filePath;
        Debug.Log("path: " + path); 

        //read the file
        List<string> steps = File.ReadAllLines(path).ToList();

        //get the text objects for the steps and current step
        GameObject stepObj = GameObject.Find("StepText");
        GameObject currentStepObj = GameObject.Find("Current Step");
        stepText = stepObj.GetComponent<TextMeshProUGUI>();
        currentStep = currentStepObj.GetComponent<TextMeshProUGUI>();

        //set the vertical spacing between the sets of verticies
        float verticalSpacing = 2.0f;

        //set the font size for the text objects
        stepText.fontSize = 24;
        currentStep.fontSize = 24;

        //set the positions of the parent objects for the left and right verticies
        leftNodeParent.transform.position = new Vector3(leftNodeParent.transform.position.x, verticalSpacing, leftNodeParent.transform.position.z);
        rightNodeParent.transform.position = new Vector3(rightNodeParent.transform.position.x, -verticalSpacing, rightNodeParent.transform.position.z);

        //iterate through the steps and animate them
        foreach (string step in steps) {
            animateStep(step);
        }
    }

    /// <summary>
    /// This method gets the bounds of the gameObject node parent.
    /// Inspired by https://www.youtube.com/watch?v=4URtDoKPu7M
    /// </summary>
    /// <param name="gameObject">The gameObject node parent.</param>
    /// <returns></returns>
    private Bounds GetBounds(GameObject gameObject) {
        Bounds bound = new Bounds(gameObject.transform.position, Vector3.zero);
        var rList = gameObject.GetComponentsInChildren(typeof(Renderer));
        foreach(Renderer r in rList) {
            bound.Encapsulate(r.bounds);
        }
        return bound;
    }
    
    /// <summary>
    /// This method fits the graph on the screen.
    /// Inspired by https://www.youtube.com/watch?v=4URtDoKPu7M
    /// </summary>
    private void FitOnScreen() {
        // Get the bounds of the left and right node parents
        Bounds bound = GetBounds(leftNodeParent);
        bound.Encapsulate(GetBounds(rightNodeParent));
        Vector3 boundSize = bound.size;

        // If there are less than 6 nodes, set the camera to orthographic size to fit the graph
        if (numberOfNodes < 6){
            float diagonal = Mathf.Sqrt((boundSize.x * boundSize.x) + (boundSize.y * boundSize.y) + (boundSize.z * boundSize.z));
            Camera.main.orthographicSize = (diagonal / 2.0f);
            transform.position = bound.center;
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
        }
        // If there are less than 15 nodes, set the camera to orthographic size to fit the graph
        else if (numberOfNodes < 15){
            float diagonal = Mathf.Sqrt((boundSize.x * boundSize.x) + (boundSize.y * boundSize.y) + (boundSize.z * boundSize.z));
            Camera.main.orthographicSize = (diagonal / 2.0f) / 1.2f;
            transform.position = bound.center;
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
        }
        // If there are more than 15 nodes, set the camera to orthographic size to fit the graph 
        else {
            // Calculate the aspect ratio of the screen
            float screenRatio = (float)Screen.width / Screen.height;
            Camera.main.orthographicSize = boundSize.x / screenRatio / 2;

            // Calculate the desired y-position of the camera
            float centerY = bound.min.y + (boundSize.y * 3 / 5);

            // Set the camera's y-position to the desired position
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, centerY, Camera.main.transform.position.z);
        }
    }
    
    
    private void SpawnNodes(int nodeCount, GameObject parentObj, Color nodeColor, string[] nodeNames) {
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
            nodes.Add(currentNode.name, currentNode);
        }
    }

    //parse the step and get the first and second set of the bipartite graph
    (string[], string[]) parseInitializeStep(string step) {
        // Remove "Initialize: " from the start of the string, then remove the parentheses
        string sets = step.Replace("Initialize: ", "").Replace("(", "").Replace(")", "");

        // Split the string into two parts on the space
        string[] splitSets = sets.Split(' ');

        // Split each part on the  comma
        string[] firstSet = splitSets[0].Split(',');
        string[] secondSet = splitSets[1].Split(',');

        Debug.Log("First Set: " + firstSet);
        Debug.Log("Second Set: " + secondSet);

        return (firstSet, secondSet);
    }

    void AddEdges(List<(string, string)> edges) {
        // Create a new Material
        Material material = new Material(Shader.Find("Unlit/Color"));
        material.color = Color.white; // Set the color to white

        foreach (var edge in edges) {

            Debug.Log("Adding edges" + edge.Item1 + " " + edge.Item2);
            // Find nodes in pair
            //var node1 = GameObject.Find("Node" + edge.Item1);
            GameObject node1 = nodes["Node" + edge.Item1];
            GameObject node2 = nodes["Node" + edge.Item2];

            var edgeObject = new GameObject("Edge" + edge.Item1 + "_" + edge.Item2);
            var lineRenderer = edgeObject.AddComponent<LineRenderer>();

            lineRenderer.material = material;

            // Get the edge positions
            lineRenderer.SetPosition(0, node1.transform.position);
            lineRenderer.SetPosition(1, node2.transform.position);

            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
        }
    }


    void animateStep(string step) {
        
        //the step is for intializing the bipartite graph
        if (step.StartsWith("Initialize:")){
            string[] firstSet;
            string[] secondSet;

            //parse the step and get the first and second set of the bipartite graph
            (firstSet, secondSet) = parseInitializeStep(step);

            numberOfNodes = firstSet.Length;

            Debug.Log("First Set: " + string.Join(",", firstSet));
            Debug.Log("Second Set: " + string.Join(",", secondSet));
            SpawnNodes(firstSet.Length, leftNodeParent, leftColor, firstSet);
            SpawnNodes(secondSet.Length, rightNodeParent, rightColor, secondSet);
            FitOnScreen();
            stepText.text = InitializeText(firstSet, secondSet);
            Debug.Log("Step text: " + stepText.text);
        } 
        else if (step.StartsWith("Add_edge:")) {
            Debug.Log("Adding edges");
            List<(string, string)> edges = ParseStep(step);
            AddEdges(edges);
            stepText.text += string.Join(" ", AddEdgeText(edges));
            StartCoroutine(StartWaiter());
        }
        else if (step.StartsWith("Add_path:")) {
            List<(string, string)> edges = ParseStep(step);
            time = time + stepTimeDuration;
            for (int i = 0; i < edges.Count; i++) {
                var edge = edges[i];
                Color color = i % 2 == 0 ? Color.green : Color.white;
                StartCoroutine(ChangeEdgeColorWaiter(time, edge, color, augmentedPathText(edges) , 0.08f, -1));
                StartCoroutine(HighlightNodeWaiter(time, edge.Item1));
                StartCoroutine(HighlightNodeWaiter(time, edge.Item2));
            }
        } else if (step.StartsWith("Update_match")){
            List<(string, string)> edges = ParseStep(step);
            time = time + stepTimeDuration;
            foreach (var edge in edges) {
                StartCoroutine(ChangeEdgeColorWaiter(time, edge, Color.yellow, matchText(edges), 0.08f, -1));
                StartCoroutine(HighlightNodeWaiter(time, edge.Item1));
                StartCoroutine(HighlightNodeWaiter(time, edge.Item2));
            }
        } else if (step.StartsWith("Disregard_vertices:")){
            List<(string, string)> edges = ParseStep(step);
            time = time + stepTimeDuration;
            if (edges.Count == 0) {
                StartCoroutine(ChangeTextWaiter(time, "No edges to disregard."));
            } else {
                foreach (var edge in edges) {
                    StartCoroutine(ChangeEdgeColorWaiter(time, edge, Color.grey, disregardVerticesText(edges), 0.05f, --lowestSpriteOrder));
                    StartCoroutine(HighlightNodeWaiter(time, edge.Item1));
                    StartCoroutine(HighlightNodeWaiter(time, edge.Item2));
                }
            }  
        } else if (step.StartsWith("Begin_Phase")){
            time = time + stepTimeDuration;
            string newText = "Begin Phase " + step.Substring(12);
            string currentStepText = "Phase " + step.Substring(12);
            StartCoroutine(ChangeText(newText, currentStepText, time));
        } else if (step.StartsWith("Maximum matching:")){
            time = time + stepTimeDuration;
            StartCoroutine(ChangeText("Finished", "Maximum matching is " + step.Substring(17), time));
        }
    }

    IEnumerator ChangeText(string newText, string currentStepText, float waitTime) {
        yield return new WaitForSeconds(waitTime);
        stepText.text = newText;
        currentStep.text = currentStepText;
    }

    IEnumerator LerpColor(Material material, Color startColor, Color endColor, float duration)
    {
        float smoothness = 0.02f;
        float progress = 0; //This float will serve as the 3rd parameter of the lerp function.
        float increment = smoothness/duration; //The amount of change to apply.

        while(progress < 1)
        {
            material.color = Color.Lerp(startColor, endColor, progress);
            progress += increment;
            yield return new WaitForSeconds(smoothness);
        }
        material.color = endColor;
    }

    IEnumerator LerpSpriteColor(SpriteRenderer spriteRenderer, Color startColor, Color endColor, float duration)
    {
        Debug.Log("changing sprite Begin_Phasecolor!");
        float smoothness = 0.02f;
        float progress = 0; //This float will serve as the 3rd parameter of the lerp function.
        float increment = smoothness/duration; //The amount of change to apply.

        while(progress < 1)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, progress);
            progress += increment;
            yield return new WaitForSeconds(smoothness);
        }
        spriteRenderer.color = endColor;
    }

        
    void ChangeEdgeColor((string, string) edge, Color newColor, float width, int spriteOrder) {
        string edgeName = "Edge" + edge.Item1 + "_" + edge.Item2;

        // Find the edge
        var edgeObject = GameObject.Find(edgeName);

        if (edgeObject == null) {
            Debug.LogError("Edge not found: " + edgeName);
            return;
        }

        var lineRenderer = edgeObject.GetComponent<LineRenderer>();
        lineRenderer.sortingOrder = spriteOrder;

       // Bring the edge to the front of the hierarchy
        edgeObject.transform.SetAsLastSibling();

        Color startColor = lineRenderer.material.color;

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        // Change the color of the edge
        //lineRenderer.material.color = newColor;
        StartCoroutine(LerpColor(lineRenderer.material, startColor, newColor, edgeColorChangeDuration));
    }

    IEnumerator StartWaiter(){
        time = time + 5;
        yield return new WaitForSeconds(time);
    }

    IEnumerator ChangeTextWaiter(float waitTime, string newText) {
        yield return new WaitForSeconds(waitTime);
        stepText.text = newText;
    }

    IEnumerator ChangeEdgeColorWaiter(float waitTime, (string, string) edge, Color newColor, string newText, float width, int spriteOrder) {
        // wait for the time
        yield return new WaitForSeconds(waitTime);
        stepText.text = newText;
        ChangeEdgeColor(edge, newColor, width, spriteOrder);
    }

    IEnumerator HighlightNode(string node) {
        string nodeName = "Node" + node;
        GameObject nodeObject = nodes[nodeName];
        var spriteRenderer = nodeObject.GetComponent<SpriteRenderer>();
        Color startColor = spriteRenderer.color;
        StartCoroutine(LerpSpriteColor(spriteRenderer, startColor, Color.white, nodeHighlightDuration / 2.0f));
        yield return new WaitForSeconds(nodeHighlightDuration / 2.0f);
        StartCoroutine(LerpSpriteColor(spriteRenderer, Color.white, startColor, nodeHighlightDuration / 2.0f));
    }

    IEnumerator HighlightNodeWaiter(float waitTime, string node) {
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(HighlightNode(node));
    }

    List<(string, string)> ParseStep(string step) {
        if (!step.Contains("(") || !step.Contains(")")) {
            return new List<(string, string)>();
        }

        // Remove step before the semicolon
        int colonIndex = step.IndexOf(':');
        string pairs = step.Substring(colonIndex + 1).Trim();
        pairs = pairs.Replace("(", "").Replace(")", "");

        // Split the string into pairs
        string[] splitPairs = pairs.Split(' ');

        // Create a list to hold the parsed pairs
        List<(string, string)> parsedPairs = new List<(string, string)>();

        Debug.Log("Split pairs: " + splitPairs + " Length: " + splitPairs.Length);


        foreach (string pair in splitPairs) {
            string[] nodes = pair.Split(',');
            parsedPairs.Add((nodes[0], nodes[1]));
        }

        return parsedPairs;
    }

    string InitializeText(string[] firstSet, string[] secondSet) {
        string text = "Set up the bipartite graph with the left verticies (";
        text += string.Join(",", firstSet) + ") and the right verticies (";
        text += string.Join(",", secondSet) + ") and create an empty matching. ";

        return text;
    }

    string AddEdgeText(List<(string, string)> edges) {
        string text = "Add edges between ";

        for (int i = 0; i < edges.Count; i++) {
            text += "(" + edges[i].Item1 + "  , " + edges[i].Item2 + ")";
            if (i < edges.Count - 1) {
                text += ", ";
            }
        }

        Debug.Log("Text length: " + text.Length);

        int maxLength = 75; 
        if (text.Length > maxLength) {
            text = "Add the edges.";
        }

        return text;
    }

    string AugmentingPathText(List<(string, string)> edges) {
        string text = "Found an augmenting path from ";

        for (int i = 0; i < edges.Count; i++) {
            text += "Node " + edges[i].Item1 + " to Node " + edges[i].Item2;
            if (i < edges.Count - 1) {
                text += ", ";
            }
        }

        return text;
    }

    string MatchText(List<(string, string)> edges) {
        string text = "Found match(es): ";

        for (int i = 0; i < edges.Count; i++) {
            text += "Node " + edges[i].Item1 + " and Node " + edges[i].Item2;
            if (i < edges.Count - 1) {
                text += ", ";
            }
        }

        return text;
    }

    string DisregardVerticesText(List<(string, string)> edges) {
        string text = "Ignore edges between ";

        if (edges.Count == 0) {
            return "No edges to ignore.";
        } else {
            for (int i = 0; i < edges.Count; i++) {
                text += "Node " + edges[i].Item1 + " and Node " + edges[i].Item2;
                if (i < edges.Count - 1) {
                    text += ", ";
                }
            }
        }
    
        return text;
    }
}

