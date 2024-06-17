using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UIReadPlayerTurn : MonoBehaviour
{
    [SerializeField]
    private string playerTurnSuffix = "'s turn";
    [SerializeField] 
    private string aI_Name = "AI";
    private TextMeshProUGUI _textMesh;
    
    #region Monobehaviour
    public void Awake()
    {
        TryGetComponent(out _textMesh);
        GlobalActions.OnPlayerTurnChanged += OnPlayerTurnChanged;
    }

    private void OnDestroy()
    {
        GlobalActions.OnPlayerTurnChanged -= OnPlayerTurnChanged;
    }
    #endregion Monobehaviour

    private void OnPlayerTurnChanged(PlayerTurn newPlayer, bool isSinglePlayer)
    {
        if (newPlayer == PlayerTurn.None)
        {
            _textMesh.SetText(string.Empty);
            return;
        }
        
        // we use string builder to avoid allocating heap memory AND garbage 
        StringBuilder sb = new StringBuilder();
        if (isSinglePlayer && newPlayer == PlayerTurn.PlayerB)
        {
            sb.Append(aI_Name);
        }
        else
        {
            sb.Append(newPlayer);
        }
        sb.Append(playerTurnSuffix);
        _textMesh.SetText(sb.ToString());
        sb.Clear();
    }
}
