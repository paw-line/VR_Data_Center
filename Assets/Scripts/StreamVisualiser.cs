using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class StreamVisualiser : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem ps;

    //public Vector3 streamSize;
    [SerializeField]
    private Transform start = null;
    [SerializeField]
    private Transform finish = null;

    public float speed = 5f;
    public Color color;
    public float particleDencityMultiplier = 2f;
    public float radius = 2f;
    public bool trails = false;

    public bool isControlledBySources = true;
    public float refreshTime = 1f;
    [SerializeField]
    private DataSource speedSource = null;
    [SerializeField]
    private DataSource colorSource = null;
    [SerializeField]
    private DataSource dencitySource = null;
    


    private float DataToSpeed(float data)
    {
        return data;
    }

    private Color DataToColor(float data)
    {
        Color t = UniversalConverter.GetInstance().TempToColor(data);
        t.a = 1f;
        return t;
    }

    private float DataToDencity(float data)
    {
        return data;
    }

    private void RefreshFromSources()
    {
        if (isControlledBySources)
        {
            if (speedSource != null)
            {
                speed = DataToSpeed(speedSource.GetData());
            }
            if (colorSource != null)
            {
                color = DataToColor(colorSource.GetData());
            }
            if (dencitySource != null)
            {
                particleDencityMultiplier = DataToDencity(dencitySource.GetData());
            }
        }
    }

    private void Update()
    {
        //Координаты начала и конца
        ps.transform.position = start.position;
        ps.transform.LookAt(finish);

        //Длинна
        float L = Vector3.Distance(start.position, finish.position);
        var mn = ps.main;
        mn.startLifetime = L / speed / ps.transform.localScale.x ;
        mn.startSpeed = speed;

        //Плотность
        var em = ps.emission;
        em.rateOverTime = ps.main.startSpeed.constant * particleDencityMultiplier;

        //Цвет
        ParticleSystem.MinMaxGradient grad = mn.startColor;
        grad.color = color;
        mn.startColor = grad;

        //Радиус
        var sh = ps.shape;
        sh.radius = radius;

        //Хвосты
        var tr = ps.trails;
        tr.enabled = trails;


    }

    IEnumerator DelayedUpdate(float _time)
    {
        yield return new WaitForSeconds(_time);
        while (true)
        {
            RefreshFromSources();
            yield return new WaitForSeconds(_time);
        }

    }

    void Awake()
    {
        ps = this.GetComponentInChildren<ParticleSystem>();
        StartCoroutine(DelayedUpdate(refreshTime));
    }

    void Start()
    {
        ps = this.GetComponentInChildren<ParticleSystem>();
        StartCoroutine(DelayedUpdate(refreshTime));
    }

    void OnDrawGizmos()
    {
        /*//Работает, но сука мерцает. Сраные матрицы.
        Transform trans = ps.transform;
        trans.Rotate(90.0f, 0, 0, Space.Self);
        Gizmos.matrix = trans.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        float y = ps.startSpeed * ps.startLifetime;
        Vector3 t = new Vector3(ps.shape.radius * 2, y, ps.shape.radius * 2);
        Vector3 pos = ps.transform.position;
        pos.y += y / 2;
        Gizmos.DrawWireCube(pos, t);
        */

        /*//Работает неправильно. Только соосно.
        Gizmos.color = Color.yellow;
        float y = ps.startSpeed * ps.startLifetime * ps.transform.localScale.y;
        Vector3 t = new Vector3(ps.shape.radius * 2 * ps.transform.localScale.x, y, ps.shape.radius * 2 * ps.transform.localScale.z);
        Vector3 pos = ps.transform.position;
        pos.y += y / 2;
        Gizmos.DrawWireCube(pos, t);
        */

        //Рисование линиями границы системы
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start.position + new Vector3(0,0,ps.shape.radius)* ps.transform.localScale.z, finish.position + new Vector3(0, 0, ps.shape.radius) * ps.transform.localScale.z);
        Gizmos.DrawLine(start.position - new Vector3(0, 0, ps.shape.radius) * ps.transform.localScale.z, finish.position - new Vector3(0, 0, ps.shape.radius) * ps.transform.localScale.z);
        Gizmos.DrawLine(start.position + new Vector3(ps.shape.radius, 0, 0) * ps.transform.localScale.x, finish.position + new Vector3(ps.shape.radius, 0, 0) * ps.transform.localScale.x);
        Gizmos.DrawLine(start.position - new Vector3(ps.shape.radius, 0, 0) * ps.transform.localScale.x, finish.position - new Vector3(ps.shape.radius, 0, 0) * ps.transform.localScale.x);
        Gizmos.DrawLine(start.position + new Vector3(0, ps.shape.radius, 0) * ps.transform.localScale.y, finish.position + new Vector3(0, ps.shape.radius, 0) * ps.transform.localScale.y);
        Gizmos.DrawLine(start.position - new Vector3(0, ps.shape.radius, 0) * ps.transform.localScale.y, finish.position - new Vector3(0, ps.shape.radius, 0) * ps.transform.localScale.y);



        if ((start != null) && (finish != null))
        {
            Gizmos.DrawIcon(start.position, "start", true);
            Gizmos.DrawIcon(finish.position, "finish", true);
        }
    }

    void OnDrawGizmosSelected()
    {
        
        /*
        if ((start != null) && (finish != null))
        {
            Gizmos.DrawIcon(start.position, "start", true);
            Gizmos.DrawIcon(finish.position, "finish", true);
        }
        */
    }

}