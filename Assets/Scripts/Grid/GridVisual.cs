using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grid;

public class GridVisual : MonoBehaviour
{
    // Singleton
    private static GridVisual instance;
    public static GridVisual Instance { get { return instance; } }

    //Grid visual
    [SerializeField]
    private List<Material> gridMaterials;
    private float startValue = 0f;
    private float endValue = 500f;
    private float duration = 2f;
    private float valueToLerp;

    [SerializeField]
    private List<Vector2> highlightPositions = new List<Vector2>();

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;
    }

    private void Start()
    {
        List<Vector2> emptyList = new List<Vector2>();
        ConvertHighLightToShader(emptyList);
    }

    public void Spawn(Vector3 startPosition)
    {
        StartCoroutine(SpawnGridVisual(startPosition));
    }

    private IEnumerator SpawnGridVisual(Vector3 startPosition)
    {
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            float t = EaseInCirc(timeElapsed / duration); // Apply easeInOutSine function to interpolation factor
            valueToLerp = Mathf.Lerp(startValue, endValue, t);
            timeElapsed += Time.deltaTime;

            foreach (Material material in gridMaterials)
            {
                material.SetFloat("_GridAreaSize", 110f);
                material.SetFloat("_GridAreaVisibleArea", valueToLerp);
                material.SetVector("_GridPositionOrigin", startPosition);
            }
            yield return null;
        }

        valueToLerp = endValue;
        foreach (Material material in gridMaterials)
        {
            material.SetFloat("_GridAreaVisibleArea", valueToLerp);
        }
    }

    private float EaseInCirc(float x)
    {
        if (x > .2f) x *= 1.1f;
        return 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 4));
    }

    public void ConvertHighLightToShader(List<Vector2> highlightPositions)
    {
        if(highlightPositions.Count == 0)
        {
            foreach (Material material in gridMaterials)
            {
                material.SetInt("_HighlightIsActive", 0); // 0 inactive, 1 active
            }
            return;
        }

        // The arrays in the shader require a specifc length that's always matched, so we always use a list of 100 items
        if(highlightPositions.Count < 100)
        {
            for(int i = highlightPositions.Count; i < 100; i++)
            {
                highlightPositions.Add(new Vector2(1000000, 1000000));
            }
        }

        List<float> highlightPositionsX = new List<float>();
        List<float> highlightPositionsY = new List<float>();

        for (int i = 0; i < highlightPositions.Count; i++)
        {
            highlightPositionsX.Add(highlightPositions[i].x);
            highlightPositionsY.Add(highlightPositions[i].y);
        }

        float[] highlightPositionsArrayX = highlightPositionsX.ToArray();
        float[] highlightPositionsArrayY = highlightPositionsY.ToArray();

        foreach (Material material in gridMaterials)
        {
            //material.SetTexture("_HighlightPositions", highlightPositionsTexture);
            material.SetFloatArray("_HighlightPositionsX", highlightPositionsArrayX);
            material.SetFloatArray("_HighlightPositionsY", highlightPositionsArrayY);
            material.SetInt("_HighlightPositionsCount", highlightPositions.Count);
            material.SetInt("_HighlightIsActive", 1); // 0 inactive, 1 active
        }
    }

    public void HighlightTiles(List<GridTile> tiles, GridLogic grid, int x, int y)
    {
        // Needed for the highlight shader
        List<Vector2> tilePositions = new List<Vector2>();
        for (int i = 0; i < tiles.Count; i++)
        {
            tilePositions.Add(new Vector2Int(tiles[i].WorldPosition.x, tiles[i].WorldPosition.y));
        }
        ConvertHighLightToShader(tilePositions);

        // Additional highlight objects
        List<GameObject> highlightObjects = new List<GameObject>();

        for (int i = 0; i < tiles.Count; i++)
        {
            GameObject highlightObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // TO DO: implement the gridtile scale here instead of 10
            highlightObject.transform.position = new Vector3(tiles[i].WorldPosition.x + 1f / 2f, tiles[i].WorldPosition.y + 1f / 2f, -.5f);
            highlightObject.transform.localScale = new Vector3(.25f, .25f, .01f);
            highlightObjects.Add(highlightObject);
        }
    }
}
