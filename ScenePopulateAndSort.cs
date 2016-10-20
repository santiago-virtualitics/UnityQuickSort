using UnityEngine;
using System.Collections;


//                                           point2i
// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
// this class is used to carry along the fat pivot.
// if we use a stack, we can get around creating
// these points all over the place.
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

//                                scene tree manager
// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
public class SceneTreeManager : MonoBehaviour
{
    public GameObject spark;
    public Transform tickmarksTransform;

    double[] datavalues;
    int[] order;
    public int sizedata = 360;
    bool deepdebug = false;
    bool roundvalues = true;

    bool checkIfSorted()
    {
        bool sorted = true;
        for (int i = 0; sorted && i < sizedata - 1; i++)
        {
            sorted = datavalues[order[i]] <= datavalues[order[i + 1]];
        }
        return sorted;
    }

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
        for (int i = 0; i < sizedata; i++) s = s + ((int)(datavalues[order[i]] * 100.0)) / 100f + " | ";
        print(s);
    }

    // quicksortsplit needs to folded into quicksort
    Point2I QuickSortSplit(int start, int end)
    {
        if (start >= end || start < 0 || end >= sizedata) return null;
        int i = start;

        int j = end;
        int pivot = i;
        double pivotvalue = datavalues[order[pivot]];
        int iclone = i;

        if (deepdebug) print("QuickSort: Partition: [" + i + "].." + j);
        if (deepdebug) print("  moving data left and right of " + pivotvalue);

        int numpivots = 0;
        double v;
        int temp;

        while (i < j)
        {
            while (i <= end && (v = datavalues[order[i]]) <= pivotvalue)
            {
                if (v == pivotvalue)
                {
                    if (iclone < i)
                    {
                        if (deepdebug) print("moving clone from " + i + " to " + iclone);
                        temp = order[iclone];
                        order[iclone] = order[i];
                        order[i] = temp;
                    }
                    if (deepdebug) printdata();
                    numpivots++;
                    iclone++;
                }
                i++;
            }
            while (j >= start && datavalues[order[j]] > pivotvalue) j--;
            if (i >= j)
            {
                if (numpivots > 1)
                {
                    if (deepdebug) { print("  scooting " + numpivots + " clones to spot ending at " + j); }
                    if (end - start >= numpivots)
                        for (int t = 0; t < numpivots; t++)
                        {
                            temp = order[j - t];
                            order[j - t] = order[start + t];
                            order[start + t] = temp;
                        }
                    if (deepdebug) { print("  returning " + (j - numpivots) + " with " + (j + 1)); }
                    return new Point2I(j - numpivots, j + 1);
                }
                else
                {
                    if (deepdebug) { print("  swapping " + order[j] + " with " + order[start]); }
                    temp = order[start];
                    order[start] = order[j];
                    order[j] = temp;
                    if (deepdebug) { print("  returning " + (j - 1) + " with " + (j + 1)); }
                    return new Point2I(j - 1, j + 1);
                }
            }
            temp = order[j];
            order[j] = order[i];
            order[i] = temp;
        }
        return null;
    }


    int qcount;
    int shortcircuitcount;
    // quicksort needs to be folded into a linear structure
    bool QuickSort(int i, int j)
    {
        qcount++;
        if (i >= j) return true;
        Point2I p = QuickSortSplit(i, j);
        if (deepdebug) printdata();
        if (p == null) return true;
        if (qcount > sizedata * sizedata)
        {
            print("MAXED OUT.");
            return false;
        }
        if (p.i < 0) return QuickSort(p.j, j);
        if (p.j >= sizedata) return QuickSort(i, p.i);
        return QuickSort(i, p.i) && QuickSort(p.j, j);
    }

    float map(float v, float so, float sf, float fo, float ff)
    {
        if (fo == ff) return so;
        return fo + (v - so) * (ff - fo) / (sf - so);
    }

    void SortNow()
    {
        qcount = 0;
        shortcircuitcount = 0;
        if (checkIfSorted())
        {
            print("Sorted before it started!");
            return;
        }

        double to = Time.fixedTime;
        if (deepdebug) printdata();
        //if (!doesNotHelp) print("start time " + to);
        QuickSort(0, sizedata - 1);
        double tf = Time.fixedTime;
        //if (!doesNotHelp) print("end time   " + tf);
        if (deepdebug) printdata();

        print("sorting " + sizedata + " took " + ((double)((int)((tf - to) * 10000.0)) / 10000.0) + "s");
        print("calls " + qcount + " with " + shortcircuitcount + " short circuits");

        if (checkIfSorted())
        {
            print("Sorted!");
        }
        else
        {
            print("WARNING: NOT SORTED CORRECTLY!" + seed);
        }
    }

    void RepositionSparks()
    {
        for (int i = 0; i < sizedata; i++)
        {
            float angle = (float)i * 360f / (float)sizedata;
            GameObject spark = tickmarksTransform.GetChild(i).gameObject;
            //spark.transform.Reset();
            spark.transform.Rotate(Vector3.forward, angle);
            spark.transform.Translate(0, map((float)datavalues[order[i]], 0, sizedata, 4, 5), 0);
        }

    }

    // Use this for initialization
    void Start()
    {
        if (sizedata <= 0) sizedata = 360;
        datavalues = new double[sizedata];
        order = new int[sizedata];
        UnsortNow();
        for (int i = 0; i < sizedata; i++)
        {
            if (roundvalues) datavalues[i] = (int)(datavalues[i]);
            GameObject newspark = Object.Instantiate(spark);

            if (tickmarksTransform != null)
                newspark.transform.parent = tickmarksTransform;

            newspark.SetActive(true);
        }
        RepositionSparks();
        if (deepdebug) printdata();
    }

    int seed = 0;

    void UnsortNow()
    {
        Random.InitState(++seed);
        for (int i = 0; i < sizedata; i++)
        {
            order[i] = i;
            datavalues[i] = Random.Range(0, sizedata + 1f - (float.Epsilon * 2f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward, Time.deltaTime * (-31.4f));
        if (Input.GetKeyDown("space"))
        {
            SortNow();
            RepositionSparks();
            UnsortNow();
        }
    }

}
