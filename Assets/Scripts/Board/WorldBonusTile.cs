using TMPro;
using UnityEngine;

public class WorldBonusTile : MonoBehaviour
{
    const string kTripleWordString = "TW";
    const string kTripleLetterString = "TL";
    const string kDoubleWordString = "DW";
    const string kDoubleLetterString = "DL";
    const string kCenterTileString = "+";

    // Define the colors for the bonus tiles
    readonly Color kTripleWordColor = new Color(128f / 255f, 0f / 255f, 32f / 255f, 0.75f);  // Rich Burgundy (RGB: 128, 0, 32)
    readonly Color kTripleLetterColor = new Color(65f / 255f, 105f / 255f, 225f / 255f, 0.75f);  // Royal Blue (RGB: 65, 105, 225)
    readonly Color kDoubleWordColor = new Color(212f / 255f, 175f / 255f, 55f / 255f, 0.75f);  // Premium Gold (RGB: 212, 175, 55)
    readonly Color kDoubleLetterColor = new Color(229f / 255f, 228f / 255f, 226f / 255f, 0.75f);  // Platinum (RGB: 229, 228, 226)
    readonly Color kCenterTileColor = new Color(255f / 255f, 0f / 255f, 0f / 255f, 0.75f);

    readonly Color kTripleWordLabelColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);  
    readonly Color kTripleLetterLabelColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);  
    readonly Color kDoubleWordLabelColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);  
    readonly Color kDoubleLetterLabelColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);  

    [SerializeField] private TMP_Text _bonusText = null;
    [SerializeField] private SpriteRenderer _spriteRenderer = null;

    public void Populate(TileBonusType bonusType)
    {
        _spriteRenderer.color = GetColorForBonusType(bonusType);
        _bonusText.text = GetStringForBonusType(bonusType);
        _bonusText.color = GetColorForBonusLabel(bonusType);
    }

    Color GetColorForBonusType(TileBonusType bonusType)
    {
        switch (bonusType)
        {
            case TileBonusType.kTripleWord:
                {
                    return kTripleWordColor;
                }
            case TileBonusType.kTripleLetter:
                {
                    return kTripleLetterColor;
                }
            case TileBonusType.kDoubleWord:
                {
                    return kDoubleWordColor;
                }
            case TileBonusType.kDoubleLetter:
                {
                    return kDoubleLetterColor;
                }
            case TileBonusType.kCenterTile:
                {
                    return kCenterTileColor;
                }
            default:
                {
                    Debug.Log("Bonus Tile component has no bonus type!");
                    return Color.white;
                }
        }
    }

    Color GetColorForBonusLabel(TileBonusType bonusType)
    {
        switch (bonusType)
        {
            case TileBonusType.kTripleWord:
                {
                    return kTripleWordLabelColor;
                }
            case TileBonusType.kTripleLetter:
                {
                    return kTripleLetterLabelColor;
                }
            case TileBonusType.kDoubleWord:
                {
                    return kDoubleWordLabelColor;
                }
            case TileBonusType.kDoubleLetter:
                {
                    return kDoubleLetterLabelColor;
                }
            case TileBonusType.kCenterTile:
                {
                    return Color.white;
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
