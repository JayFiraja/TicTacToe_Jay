using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell_Interact : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private CellData _cellData;
    private GameStateManager _gameStateManager;

    [SerializeField]
    private MonoBehaviour[] SpawnActionRefs = Array.Empty<MonoBehaviour>();
    private List<IAction> _spawnActions;
    
    [SerializeField]
    private MonoBehaviour[] HighlightActionRefs = Array.Empty<MonoBehaviour>();
    private List<IAction> _highlightActions;
    
    [SerializeField]
    private MonoBehaviour[] WinningCellActionRefs = Array.Empty<MonoBehaviour>();
    private List<IAction> _winningCellActions;

    private bool _isHovered = false;

    public void Initialize(CellData cellData)
    {
        _cellData = cellData;
        _gameStateManager = GameManager.S.GameStateManager;

        GlobalActions.PlayAICell += TryPlayAI;
        GlobalActions.OnPlayerTurnChanged += OnPlayerTurnChanged;

        IActionUtil.TryGetAllActions(SpawnActionRefs, out _spawnActions);
        IActionUtil.TryGetAllActions(HighlightActionRefs, out _highlightActions);
        IActionUtil.TryGetAllActions(WinningCellActionRefs, out _winningCellActions);
    }

    private void OnDestroy()
    {
        GlobalActions.PlayAICell -= TryPlayAI;
        GlobalActions.OnPlayerTurnChanged -= OnPlayerTurnChanged;
    }

    private void TryPlayAI(Vector2 coords)
    {
        if (!_cellData.IsMarked && _cellData.Coords == coords)
        {
            PlayMarker();
        }
    }
    
    public void SpawnActions()
    {
        IActionUtil.RunActions(_spawnActions, callDoAction: true);
    }
    
    public void DeSpawnActions()
    {
        _cellData.IsMarked = false;
        // Calls Undo on ALL Actions
        IActionUtil.RunActions(_spawnActions, callDoAction: false);
        IActionUtil.RunActions(_highlightActions, callDoAction: false);
        IActionUtil.RunActions(_winningCellActions, callDoAction: false);
    }

    private void PlayOnHoverEnterActions()
    {
        IActionUtil.RunActions(_highlightActions, callDoAction: true);
    }
    
    private void PlayOnHoveredExitActions()
    {
        IActionUtil.RunActions(_highlightActions, callDoAction: false);
    }

    private void CheckWinningCell()
    {
        IActionUtil.RunActions(_winningCellActions, callDoAction: true);
    }

    private bool CanInteract()
    {
        if (_cellData.IsMarked)
        {
            return false;
        }
        if (!_gameStateManager.CanPlayCell)
        {
            return false;
        }
        if (_gameStateManager.IsAIPlaying && _gameStateManager.PlayerTurn == PlayerTurn.PlayerB)
        {
            return false;
        }

        return true;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        if (!CanInteract())
        {
            return;
        }
        
        PlayOnHoverEnterActions();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        
        if (!CanInteract())
        {
            return;
        }

        PlayOnHoveredExitActions();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        if (!CanInteract())
        {
            return;
        }

        PlayMarker();
    }

    private void PlayMarker()
    {
        _cellData.IsMarked = true;
        PlayOnHoveredExitActions();
        GlobalActions.CellDataSelected?.Invoke(_cellData);
    }

    private void OnPlayerTurnChanged(PlayerTurn newPlayerTurn, bool isSinglePlayer)
    {
        if (_isHovered && CanInteract())
        {
            PlayOnHoverEnterActions();
        }
    }
}
