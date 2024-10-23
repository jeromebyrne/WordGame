using TMPro;
using UnityEngine;

public class WorldBonusTile : MonoBehaviour
{
    const string kTripleWordString = "TW";
    const string kTripleLetterString = "TL";
    const string kDoubleWordString = "DW";
    const string kDoubleLetterString = "DL";
    const string kCenterTileString = "+";

    [SerializeField] private TMP_Text _bonusText = null;
    [SerializeField] private SpriteRenderer _spriteRenderer = null;

    public void Populate(TileBonusType bonusType)
    {
        _spriteRenderer.color = GetColorForBonusType(bonusType);
        _bonusText.text = GetStringForBonusType(bonusType);
    }

    Color GetColorForBonusType(TileBonusType bonusType)
    {
        switch (bonusType)
        {
            case TileBonusType.kTripleWord:
                {
                    return Color.magenta;
                }
            case TileBonusType.kTripleLetter:
                {
                    return Color.yellow;
                }
            case TileBonusType.kDoubleWord:
                {
                    return Color.cyan;
                }
            case TileBonusType.kDoubleLetter:
                {
                    return Color.green;
                }
            case TileBonusType.kCenterTile:
                {
                    return Color.red;
                }
            default:
                {
                    Debug.Log("Bonus Tile component has no bonus type!");
                    return Color.white;
                }
        }
    }

    string GetStringForBonusType(TileBonusType bonusType)
    {
        switch (bonusType)
        {
            case TileBonusType.kTripleWord:
                {
                    return kTripleWordString;
                }
            case TileBonusType.kTripleLetter:
                {
                    return kTripleLetterString;
                }
            case TileBonusType.kDoubleWord:
                {
                    return kDoubleWordString;
                }
            case TileBonusType.kDoubleLetter:
                {
                    return kDoubleLetterString;
                }
            case TileBonusType.kCenterTile:
                {
                    return kCenterTileString;
                }
            default:
                {
                    Debug.Log("Bonus Tile component has no bonus type!");
                    return "";
                }
        }
    }
}
