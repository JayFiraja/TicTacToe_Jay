using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UI_ToggleAlternativeMesh : MonoBehaviour
{
    [SerializeField]
    private PlayerTurn player;
    private PlayerData _playerData;

    private Toggle _toggle;
    
    private void Start()
    {
        TryGetComponent(out _toggle);
        GameManager.S.TryGetPlayerTurnData(player, out _playerData);
        _toggle.isOn = _playerData.usingAlternativeMark;
        _toggle.onValueChanged.AddListener(ToggleValue);
    }

    private void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(ToggleValue);
    }

    public void ToggleValue(bool bValue)
    {
        GlobalActions.UsingAlternativeMarkToggled?.Invoke(_playerData, bValue);
    }

}
