using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SingleLetterType
{ 
    public char _letter;
    public int _points; // how precious is this letter
    public int _wordBagCount; // How many instances of this letter are added to the wordbag
}

[CreateAssetMenu(fileName = "LetterConfig", menuName = "Settings/LetterConfig")]
public class LetterConfig : ScriptableObject
{
    public SingleLetterType _a;
    public SingleLetterType _b;
    public SingleLetterType _c;
    public SingleLetterType _d;
    public SingleLetterType _e;
    public SingleLetterType _f;
    public SingleLetterType _g;
    public SingleLetterType _h;
    public SingleLetterType _i;
    public SingleLetterType _j;
    public SingleLetterType _k;
    public SingleLetterType _l;
    public SingleLetterType _m;
    public SingleLetterType _n;
    public SingleLetterType _o;
    public SingleLetterType _p;
    public SingleLetterType _q;
    public SingleLetterType _r;
    public SingleLetterType _s;
    public SingleLetterType _t;
    public SingleLetterType _u;
    public SingleLetterType _v;
    public SingleLetterType _w;
    public SingleLetterType _x;
    public SingleLetterType _y;
    public SingleLetterType _z;

    private List<SingleLetterType> _cachedLetterList = null;

    public List<SingleLetterType> GetAllLetters()
    {
        if (_cachedLetterList == null || _cachedLetterList.Count < 1)
        {
            _cachedLetterList = new List<SingleLetterType>
            {
                _a, _b, _c, _d, _e, _f, _g, _h, _i, _j, _k, _l, _m,
                _n, _o, _p, _q, _r, _s, _t, _u, _v, _w, _x, _y, _z
            };
        }

        return _cachedLetterList;
    }
}