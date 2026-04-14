using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Audio.ProcessorInstance;

public class HelperPieceViewer : MonoBehaviour
{
    private InputAction actionShiftLeft;
    private InputAction actionShiftRight;

    public PieceRenderer pieceRenderer;
    private Texture2D pieceTexture;
    private PieceData piece;
    [SerializeField]
    private PieceData[] allPieces;
    private int index = 0;
    private TextMeshPro textMesh;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        actionShiftLeft = InputSystem.actions.FindAction("ShiftLeft");
        actionShiftRight = InputSystem.actions.FindAction("ShiftRight");
        textMesh = GetComponentInChildren<TextMeshPro>();

        pieceTexture = new Texture2D(256, 256, TextureFormat.ARGB32, false); // only needs to be created once
        piece = allPieces[index];
        DrawPiece(); // dont draw every update frame, only when the shop piece visual needs to be updated
    }

    // Update is called once per frame
    void Update()
    {
        if(actionShiftLeft.WasPressedThisFrame())
        {
            index -= 1;
            if (index < 0)
            {
                index = allPieces.Length - 1;
            }
            piece = allPieces[index];
            DrawPiece();
        }
        else if (actionShiftRight.WasPressedThisFrame())
        {
            index += 1;
            if (index >= allPieces.Length)
            {
                index = 0;
            }
            piece = allPieces[index];
            DrawPiece();
        }
    }

    void DrawPiece()
    {
        pieceRenderer.RenderToTexture2D(piece, pieceTexture);

        // Convert the new Texture2D to a Sprite
        Sprite sprite = Sprite.Create(pieceTexture, new Rect(new Vector2(0, 0), new Vector2(256, 256)), new Vector2(0.5f, 0.5f));

        GetComponent<SpriteRenderer>().sprite = sprite; // replace the get component with whatever image you want to overwrite
        textMesh.text = piece.name;
    }
}
