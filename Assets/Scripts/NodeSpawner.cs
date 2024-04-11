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
    public GameObject leftNodeParent;
    public GameObject rightNodeParent;
    public GameObject nodePrefab;
    public Color leftColor = Color.red;
    public Color rightColor = Color.blue;
    // Amount of time between steps
    public float stepTimeDuration = 1.0f;
    // Amount of time it takes for edge color to change from current color to new color
    public float edgeColorChangeDuration = 0.5f;
    public float nodeHighlightDuration = 0.5f;
    TextMeshProUGUI stepText;
    float time = 0;
    Dictionary<string, GameObject> nodes;


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

            Debug.Log("Adding edgees" + edge.Item1 + " " + edge.Item2);
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
            FitOnScreen();
        } 
        else if (step.StartsWith("Add_edge:")) {
            Debug.Log("Adding edges");
            List<(string, string)> edges = ParseStep(step);
            AddEdges(edges);
        }
        else if (step.StartsWith("Add_path:")) {
            List<(string, string)> edges = ParseStep(step);
            time = time + stepTimeDuration;
            for (int i = 0; i < edges.Count; i++) {
                var edge = edges[i];
                Color color = i % 2 == 0 ? Color.green : Color.black;
                StartCoroutine(ChangeEdgeColorWaiter(time, edge, Color.green, step));
                StartCoroutine(HighlightNodeWaiter(time, edge.Item1));
                StartCoroutine(HighlightNodeWaiter(time, edge.Item2));
            }
            // Animate step text
            ///StartCoroutine(ChangeStepText(time, step));
        } else if (step.StartsWith("Update_match")){
            List<(string, string)> edges = ParseStep(step);
            time = time + stepTimeDuration;
            foreach (var edge in edges) {
                StartCoroutine(ChangeEdgeColorWaiter(time, edge, Color.yellow, step));
                StartCoroutine(HighlightNodeWaiter(time, edge.Item1));
                StartCoroutine(HighlightNodeWaiter(time, edge.Item2));
            }
            // Animate step text
            //StartCoroutine(ChangeStepText(time, step));
        } else if (step.StartsWith("Disregard_vertices:")){
            List<(string, string)> edges = ParseStep(step);
            time = time + stepTimeDuration;
            foreach (var edge in edges) {
                StartCoroutine(ChangeEdgeColorWaiter(time, edge, Color.grey, step));
                StartCoroutine(HighlightNodeWaiter(time, edge.Item1));
                StartCoroutine(HighlightNodeWaiter(time, edge.Item2));
            }
            // Animate step text
            ///StartCoroutine(ChangeStepText(time, step));
        }

        //TODO: add more steps
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
        Debug.Log("changing sprite color!");
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

    void ChangeEdgeColor((string, string) edge, Color newColor) {
        
        string edgeName = "Edge" + edge.Item1 + "_" + edge.Item2;
        // Highlight Node{Item1} and Node{Item2} by briefly changing their colors to white and then back to their original color 

        // find the edge
        var edgeObject = GameObject.Find(edgeName);

        if (edgeObject == null) {
            Debug.LogError("Edge not found: " + edgeName);
            return;
        }

        var lineRenderer = edgeObject.GetComponent<LineRenderer>();
        Color startColor = lineRenderer.material.color;
        // Change the color of the edge
        //lineRenderer.material.color = newColor;
        StartCoroutine(LerpColor(lineRenderer.material, startColor, newColor, edgeColorChangeDuration));
    }

    IEnumerator ChangeEdgeColorWaiter(float waitTime, (string, string) edge, Color newColor, string newText) {
        // wait for the time
        yield return new WaitForSeconds(waitTime);
        stepText.text = newText;
        ChangeEdgeColor(edge, newColor);
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

        // Remove step before the semicolon
        int colonIndex = step.IndexOf(':');
        string pairs = step.Substring(colonIndex + 1).Trim();
        pairs = pairs.Replace("(", "").Replace(")", "");

        // Split the string into pairs
        string[] splitPairs = pairs.Split(' ');

        // Create a list to hold the parsed pairs
        List<(string, string)> parsedPairs = new List<(string, string)>();

        // Parse each pair
        foreach (string pair in splitPairs) {
            string[] nodes = pair.Split(',');
            parsedPairs.Add((nodes[0], nodes[1]));
        }

        return parsedPairs;
    }

    // Start is called before the first frame update
    void Start()
    {
        nodes = new Dictionary<string, GameObject>();
        //get file for the animation steps from its path
        //string path = Path.Combine(Application.dataPath,"InputFiles", "Input2.txt");
        string path = FilePathClass.filePath;
        Debug.Log("path: " + path); 
        //read the file
        List<string> steps = File.ReadAllLines(path).ToList();
        GameObject stepObj = GameObject.Find("StepText");
        stepText = stepObj.GetComponent<TextMeshProUGUI>();

        // This is how you change the step text in the top left
        stepText.text = "Begin animation";

        //iterate through the steps and animate them
        foreach (string step in steps) {
            // Display current step on screen
            //stepText.text = step;
            animateStep(step);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    }

