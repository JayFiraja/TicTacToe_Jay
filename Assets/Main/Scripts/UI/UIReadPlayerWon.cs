using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UIReadPlayerWon : MonoBehaviour
{
    [SerializeField]
    private string DrawDisplay = "Draw!";
    [SerializeField]
    private string WinDisplay = " wins!";
    [SerializeField]
    private string AIDisplay = "AI";
    private TextMeshProUGUI _textMesh;
    
    #region Monobehaviour
    public void Awake()
    {
        TryGetComponent(out _textMesh);
        GlobalActions.PlayerWon += OnPlayerWon;
        GlobalActions.GameEndedInDraw += OnGameEndedInDraw;
    }

    private void OnDestroy()
    {
        GlobalActions.PlayerWon -= OnPlayerWon;
        GlobalActions.GameEndedInDraw -= OnGameEndedInDraw;
    }
    
    #endregion Monobehaviour

    private void OnPlayerWon(PlayerTurn winningPlayer, bool isSinglePlayer)
    {
        if (winningPlayer == PlayerTurn.None)
        {
            _textMesh.SetText(string.Empty);
            return;
        }
        
        // we use string builder to avoid allocating heap memory AND garbage 
        StringBuilder sb = new StringBuilder();
        if (isSinglePlayer && winningPlayer == PlayerTurn.PlayerB)
        {
            sb.Append(AIDisplay);
        }
        else
        {
            sb.Append(winningPlayer);
        }
        sb.Append(WinDisplay);
        _textMesh.SetText(sb.ToString());
        sb.Clear();
    }

    private void OnGameEndedInDraw()
    {
        _textMesh.SetText(DrawDisplay);
    }
}
