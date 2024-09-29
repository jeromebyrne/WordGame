using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SingleLetterInfo
{
    public char _letter;
    public int _points; // how precious is this letter
    public int _wordBagCount; // How many instances of this letter are added to the wordbag
}

[CreateAssetMenu(fileName = "LetterConfig", menuName = "Settings/LetterConfig")]
public class LetterConfig : ScriptableObject
{
    public SingleLetterInfo _a;
    public SingleLetterInfo _b;
    public SingleLetterInfo _c;
    public SingleLetterInfo _d;
    public SingleLetterInfo _e;
    public SingleLetterInfo _f;
    public SingleLetterInfo _g;
    public SingleLetterInfo _h;
    public SingleLetterInfo _i;
    public SingleLetterInfo _j;
    public SingleLetterInfo _k;
    public SingleLetterInfo _l;
    public SingleLetterInfo _m;
    public SingleLetterInfo _n;
    public SingleLetterInfo _o;
    public SingleLetterInfo _p;
    public SingleLetterInfo _q;
    public SingleLetterInfo _r;
    public SingleLetterInfo _s;
    public SingleLetterInfo _t;
    public SingleLetterInfo _u;
    public SingleLetterInfo _v;
    public SingleLetterInfo _w;
    public SingleLetterInfo _x;
    public SingleLetterInfo _y;
    public SingleLetterInfo _z;

    private List<SingleLetterInfo> _cachedLetterList = null;

    public List<SingleLetterInfo> GetAllLetters()
    {
        if (_cachedLetterList == null || _cachedLetterList.Count < 1)
        {
            _cachedLetterList = new List<SingleLetterInfo>
            {
                _a, _b, _c, _d, _e, _f, _g, _h, _i, _j, _k, _l, _m,
                _n, _o, _p, _q, _r, _s, _t, _u, _v, _w, _x, _y, _z
            };
        }

        return _cachedLetterList;
    }
}