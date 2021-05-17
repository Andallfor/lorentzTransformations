using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class master : MonoBehaviour
{
    public GameObject lp, parent, lineStatic, lineShifted, pointPrefab, lightConePrefab;
    public speedMaster sm;
    public TextMeshProUGUI positionDisplay;
    public Vector2 lineEndStatic, lineEndShifted, drEnd, dlEnd, yEnd;
    public List<GameObject> axises = new List<GameObject>(); // can be list since we dont need to reference any specific one/we change them at the same time
    public bool shifted = false, inTermsC = true, showTimeLines = false, showLightCones = false;
    public Dictionary<int, point> points = new Dictionary<int, point>();
    void Start()
    {
        // establish grid

        // x axis
        fastCreateLine(new Vector2(-8, 0), new Vector2(8, 0), Color.black);

        // y axis
        fastCreateLine(new Vector2(0, 0), new Vector2(0, 8.25f), Color.black);
        yEnd = new Vector2(0, 8.25f);

        // cones
        float hyp = Mathf.Sqrt(Mathf.Pow(8.25f, 2) + Mathf.Pow(8, 2));
        float r = Mathf.Deg2Rad * 45f;
        float y = hyp * Mathf.Sin(r);
        drEnd = new Vector2(hyp * Mathf.Cos(r), y) / 1.25f;
        dlEnd = new Vector2(-hyp * Mathf.Cos(r), y) / 1.25f;
        fastCreateLine(Vector2.zero, drEnd, Color.black);
        fastCreateLine(Vector2.zero, dlEnd, Color.black);
    }

    public void Update()
    {
        positionDisplay.text = $"{(quickConvert2(Camera.main.ScreenToWorldPoint(Input.mousePosition)) * 10f) + new Vector2(0, 35)}";

        if (Input.GetMouseButton(0))
        {
            // draw line
            Vector2 m = quickConvert2(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (m.y > 0)
            {
                Destroy(lineStatic);
                Destroy(lineShifted);

                GameObject l = fastCreateLine(Vector2.zero, m, Color.magenta);
                lineStatic = l;
                lineEndStatic = m;     // this is the normal line
                if (shifted) redraw();

                l.GetComponent<lineMaster>().setLabelText(drawSpeed(m));
            }
        }

        if (Input.GetMouseButton(1))
        {
            // draw points

            // yes im tech recreating this everything time
            // but are you really going to complain about 
            //  this when the *rest* of the code exists
            List<string> accepted = new List<string>() {"1", "2", "3", "4", "5", "6", "7", "8", "9", "0"};
            List<Color> colors = new List<Color>() {Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, new Color(0.2f, 1, 0.2f), new Color(0.4f, 0.4f, 1),  new Color(0.56f, 0.74f, 0.56f), new Color(0.4f, 0.4f, 0)};

            foreach (string c in accepted)
            {
                if (Input.GetKeyUp(c))
                {
                    int index = Convert.ToInt32(c);
                    if (points.ContainsKey(index))
                    {
                        Destroy(points[index].staticGO);
                        if (!ReferenceEquals(points[index].shiftedGO, null)) Destroy(points[index].shiftedGO);

                        // clear time lines
                        Destroy(points[index].staticGoTime);

                        // clear light cones
                        Destroy(points[index].lightCone);
                    } 
                    points[index] = new point();

                    // im writing this at 2:30 am
                    // its fineeeeeeeee
                    points[index].staticPosition = quickConvert2(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    points[index].staticGO = Instantiate(pointPrefab, parent.transform);
                    points[index].staticGO.transform.position = points[index].staticPosition;
                    points[index].staticGO.GetComponent<RawImage>().color = colors[index];

                    redraw();
                }
            }
        }
    }
    public void resetVel()
    {
        sm.sl.value = 0;
    }
    public void slValueChange()
    {
        if (sm.sl.value == 0) shifted = false;
        else if (sm.sl.value != 0) shifted = true;

        redraw();
    }

    public void clear()
    {
        // clear lines
        Destroy(lineStatic);
        Destroy(lineShifted);
        lineEndStatic = Vector2.zero;
        lineEndShifted = Vector2.zero;

        // clear points
        clearTime();
        clearLightCones();
        foreach (point p in points.Values)
        {
            Destroy(p.shiftedGO);
            Destroy(p.staticGO);
        }
        points = new Dictionary<int, point>();
    }

    public void redraw()
    {
        // redraw shifted based on static

        // time lines
        // redundant yes, but here just in case
        clearTime();
        clearLightCones();
        if (showTimeLines) drawTime();
        if (showLightCones) drawLightCones();

        // shifted line
        if (!ReferenceEquals(lineShifted, null)) Destroy(lineShifted);
        lineEndShifted = new Vector2(xPrime(lineEndStatic.y, lineEndStatic.x, sm.sl.value), timePrime(lineEndStatic.y, lineEndStatic.x, sm.sl.value));
        lineShifted = fastCreateLine(Vector2.zero, lineEndShifted, new Color(1, 0, 1, 0.5f));

        // points
        foreach (point p in points.Values)
        {
            Vector2 pos = new Vector2(xPrime(p.staticPosition.y, p.staticPosition.x, sm.sl.value), timePrime(p.staticPosition.y, p.staticPosition.x, sm.sl.value));
            if (!ReferenceEquals(p.shiftedGO, null)) Destroy(p.shiftedGO);
            p.shiftedGO = Instantiate(pointPrefab, parent.transform);
            p.shiftedGO.transform.position = pos;
            RawImage ri = p.shiftedGO.GetComponent<RawImage>();
            ri.color = p.staticGO.GetComponent<RawImage>().color;
            ri.color = new Color(ri.color.r, ri.color.g, ri.color.b, 0.25f);
        }

        // axises
        foreach (GameObject go in axises) Destroy(go);
        axises.Add(fastCreateLine(Vector2.zero, toPrime(drEnd), Color.gray));
        axises.Add(fastCreateLine(Vector2.zero, toPrime(dlEnd), Color.gray));
        axises.Add(fastCreateLine(Vector2.zero, toPrime(yEnd), Color.gray));

        
    }
    public void drawTime()
    {
        foreach (point p in points.Values)
        {
            Color _c = p.staticGO.GetComponent<RawImage>().color;
            Color c = _c / 0.75f;
            c.a = 0.5f;
            p.staticGoTime = fastCreateLine(new Vector2(-100, p.staticGO.transform.position.y), new Vector2(100, p.staticGO.transform.position.y), c);
        }
    }
    public void clearTime()
    {
        foreach (point p in points.Values) Destroy(p.staticGoTime);
    }
    public void toggleTime()
    {
        clearTime();
        showTimeLines = !showTimeLines;
        if (showTimeLines) drawTime();
    }
    
    public void toggleLightCones()
    {
        clearLightCones();
        showLightCones = !showLightCones;
        if (showLightCones) drawLightCones();
    }
    public void clearLightCones()
    {
        foreach (point p in points.Values) Destroy(p.lightCone);
    }
    public void drawLightCones()
    {
        foreach (point p in points.Values)
        {
            p.lightCone = Instantiate(lightConePrefab, parent.transform);
            p.lightCone.transform.position = p.staticGO.transform.position;
        }
    }
    public string drawSpeed(Vector2 v)
    {
        float slope = v.y / v.x;
        float coef = (float) Math.Round((double) (1f / slope), 2);
        return (inTermsC) ? $"{coef}c m/s" : $"{300_000_000 * coef} m/s";
    }
    
    public Vector2 toPrime(Vector2 v) => new Vector2(xPrime(v.y, v.x, sm.sl.value), timePrime(v.y, v.x, sm.sl.value));

    public void changeTermC(TextMeshProUGUI txt)
    {
        if (lineStatic == null) return;
        inTermsC = !inTermsC;
        if (inTermsC) txt.text = "Relative Velocity";
        else txt.text = "Absolute Velocity";

        lineStatic.GetComponent<lineMaster>().setLabelText(drawSpeed(endPos(lineStatic)));
    }

    private Vector2 endPos(GameObject go) => quickConvert2(go.GetComponent<lineMaster>().lr.GetPosition(1));
    private GameObject fastCreateLine(Vector2 a, Vector2 b, Color c)
    {
        GameObject l = Instantiate(lp, parent.transform);
        LineRenderer lr = l.GetComponent<LineRenderer>();

        lr.SetPositions(new Vector3[2]{a, b});
        lr.startColor = c;
        lr.endColor = c;

        return l;
    }

    private Vector3 quickConvert(Vector2 v) => new Vector3(v.x, v.y, 0);

    private Vector2 quickConvert2(Vector3 v) => new Vector2(v.x, v.y);

    // prime is shifted, regular is static
    // c is set to 1 in these equations
    //      makes calc + showing easier
    // and v is relative to that

    // this is fine as i'm not actually measuring distance
    // and i'm only using it as a conversion factor in a closed system

    // huge credit to https://www.desmos.com/calculator/2j8f6yhv9v
    //      this is what i based my answers off of
    private float timeRegular(float t1, float x1, float v) => gamma(v) * (t1 + ((v * x1)));
    private float timePrime(float t, float x, float v) => gamma(v) * (t - ((v * x)));
    private float xRegular(float t1, float x1, float v) => (gamma(v) * (x1 + (v * t1)));
    private float xPrime(float t, float x, float v) => (gamma(v) * (x - (v * t)));
    private float gamma(float v) => 1f / (Mathf.Sqrt(1 - (v * v)));

}

public class point
{
    public Vector2 staticPosition, shiftedPosition;
    public GameObject shiftedGO, staticGO, staticGoTime, lightCone;
}
