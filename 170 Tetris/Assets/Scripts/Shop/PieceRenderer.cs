using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PieceRenderer : MonoBehaviour
{
    private Camera cameraComponent;
    [SerializeField]
    private RenderTexture renderTexture;
    [SerializeField]
    private Tilemap tilemap;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        renderTexture = new RenderTexture(renderTexture.descriptor);
        cameraComponent.targetTexture = renderTexture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Renders a PieceData to a Texture2D, ONLY supports 256 x 256 currently
    public void RenderToTexture2D(PieceData pieceData, Texture2D texture)
    {
        tilemap.ClearAllTiles();
        foreach (PieceBlock block in pieceData.GetBlocks())
        {
            tilemap.SetTile(new Vector3Int(block.position.x - pieceData.GetCenter().x, block.position.y - pieceData.GetCenter().y, 0), block.block);
            tilemap.SetColor(new Vector3Int(block.position.x - pieceData.GetCenter().x, block.position.y - pieceData.GetCenter().y, 0), block.color);
        }
        cameraComponent.Render();
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        texture.Apply();
        tilemap.ClearAllTiles();
    }
}
