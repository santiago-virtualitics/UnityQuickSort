using UnityEngine;
using System.Collections;

class Point2I
{
    public int i;
    public int j;
    public Point2I(int newi, int newj)
    {
        i = newi;
        j = newj;
    }
    public bool isZero()
    {
        return i == 0 && j == 0;
    }
}

public class SceneTreeManager : MonoBehaviour {
    public GameObject spark;

    double tstart;

    double[] datavalues;
    int[] order;
    public int sizedata = 360;
    bool debug = false;
    bool roundvalues = false;

    void head(int n, string s)
    {
        for (int i = 0; i < n; i++)
        {
            print(s + "[" + i + "]:" + datavalues[order[i]]);
        }
    }

    void printdata()
    {
        string s = "| ";
        for (int i=0; i<sizedata; i++) s = s + ((int)(datavalues[order[i]]*100.0))/100f + " | ";
        print(s);
    }

     Point2I QuickSortSplit(int i, int j)
    {
        if (debug) printdata();
        if (debug)  print("PQS: "+i + ".." + j);
        int pivot = i;
        double pivotvalue = datavalues[order[pivot]];
        while (i < j)
        {
            while (datavalues[order[i]] < pivotvalue) i++;
            while (datavalues[order[j]] > pivotvalue) j--;
            if (i >= j)
            {
                return new Point2I(j, j + 1);
            } else if (datavalues[order[i]]==datavalues[order[j]])
            {
                // WRONG!
                shortcircuitcount++;
                return new Point2I(i-1, j + 1);
            }
            //if (datavalues[order[i]]== datavalues[order[j]])
            int temp = order[j];
            order[j] = order[i];
            order[i] = temp;
        }
        return null;
    }


    int qcount;
    int shortcircuitcount;
    bool QuickSort(int i, int j) {
        qcount++;
        if (i >= j) return true;
        Point2I p = QuickSortSplit(i,j);
        if (p==null || qcount > sizedata*sizedata) {
            print("MAXED OUT.");
            return false;
        }
        return QuickSort(i, p.i) && QuickSort(p.j, j);
    }

    float map(float v, float so, float sf, float fo, float ff)
    {
        if (fo == ff) return so;
        return fo+(v - so) * (ff - fo) / (sf - so);
    }


    void SortNow()
    {
        double to = Time.fixedTime;
        QuickSort(0, sizedata - 1);
        double tf = Time.fixedTime;
        if (debug) printdata();
        print("sorting " + sizedata + " took " + (((int)((tf - to) * 10000.0)) / 10000.0) + "s");
        print("calls " + qcount + " with " + shortcircuitcount + " short circuits");
    }



    // Use this for initialization
    void Start () {
        tstart = Time.fixedTime;
        if (sizedata <= 0) sizedata = 360;
        datavalues = new double[sizedata];
        order = new int[sizedata];
        for (int i=0; i<sizedata; i++)
        {
            order[i] = i;
            datavalues[i] = Random.Range(0, sizedata + 1f - (float.Epsilon * 2f));
            if (roundvalues) datavalues[i] = (int)(datavalues[i]);
            GameObject newspark= Object.Instantiate(spark);
            float angle=(float)i * 360f / (float)sizedata;
            newspark.transform.Rotate(Vector3.forward, angle);
            newspark.transform.Translate(0, map((float)datavalues[order[i]],0,sizedata,4,5), 0);
            newspark.SetActive(true);
        }
        if (debug) printdata();
    }
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.forward, Time.deltaTime*(-31.4f));
        if (Input.GetKeyDown("space"))
            SortNow();
    }

}
