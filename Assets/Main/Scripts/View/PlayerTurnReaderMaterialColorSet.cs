using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PlayerTurnReaderMaterialColorSet : MonoBehaviour
{
    [SerializeField]
    private string MaterialColorName = "_Color";
    
    private Renderer _renderer;
    private GameManager _gameManager;
    private MaterialPropertyBlock _propertyBlock;
    
    private void Start()
    {
        TryGetComponent(out _renderer);
        _gameManager = GameManager.S;
        _propertyBlock = new MaterialPropertyBlock();
        GlobalActions.OnPlayerTurnChanged += OnPlayerTurnChanged;
    }

    private void OnDestroy()
    {
        GlobalActions.OnPlayerTurnChanged -= OnPlayerTurnChanged;
    }

    private void OnPlayerTurnChanged(PlayerTurn playerTurn, bool isSimplePlayer)
    {
        UpdateMaterialColor();
    }

    private void UpdateMaterialColor()
    {
        if (!_gameManager.TryGetCurrentPlayerTurnData(out PlayerData playerData))
        {
            return;
        }
        
        Color newColor = playerData.Color;
        
        // Use material property block to change this material instance color from _renderer
        _renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(MaterialColorName, newColor);
        _renderer.SetPropertyBlock(_propertyBlock);
    }


}
