using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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
    
    
    void SpawnNodes(int nodeCount, GameObject parentObj, Color nodeColor) {
        float startPosX = -(nodeCount / 2.0f) + 0.5f;
        for (int i = 0; i < nodeCount; i++) {
            // Set position based on parent's position
            Vector3 pos = new Vector3((parentObj.transform.position.x + startPosX + i), parentObj.transform.position.y, parentObj.transform.position.z);

            GameObject currentNode = Instantiate(nodePrefab, pos, parentObj.transform.rotation) as GameObject;
            currentNode.name = "Node" + Convert.ToString(i + 1);

            SpriteRenderer sprite = currentNode.GetComponent<SpriteRenderer>();
            sprite.color = nodeColor;

            TextMeshProUGUI nodeText = currentNode.GetComponentInChildren<TextMeshProUGUI>();
            nodeText.text = Convert.ToString(i + 1);

            // Ensure that this node is a child of the parent object
            currentNode.transform.parent = parentObj.transform;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SpawnNodes(leftNodeCount, leftNodeParent, leftColor);
        SpawnNodes(rightNodeCount, rightNodeParent, rightColor);
        FitOnScreen();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
