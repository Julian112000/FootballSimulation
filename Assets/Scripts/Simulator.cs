using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    Match,
    TournamentMatch
};
public enum playerposition
{
    GK,
    DF,
    MF,
    ST
};
public class Simulator : MonoBehaviour
{
    public static Simulator Instance;
    public GameMode m_GameMode;

    public List<playerposition> m_MainPositions = new List<playerposition>();

    private int m_TeamSize;
    private List<Unit> m_Units = new List<Unit>();
    public List<Unit> GetUnits
    {
        get { return m_Units; }
        set { m_Units = value; }
    }

    [Header("Game Settings")]
    [SerializeField]
    private bool m_IsSimulation;
    [SerializeField]
    private GameObject m_UnitPrefab;
    [SerializeField]
    private GameObject m_EnemyunitPrefab;
    [SerializeField]
    private Transform[] m_Goal;
    [SerializeField]
    private Ball m_Ball;
    public Ball GetBall()
    {
        return m_Ball;
    }
    [SerializeField]
    private Vector3 m_Range = new Vector3(5, 5, 5);
    [SerializeField]
    private int m_TimeMultiplier = 1;

    [Header("Unit Settings")]
    [SerializeField]
    private bool m_Seekgoal = true;
    public bool Seekgoal
    {
        get { return m_Seekgoal; }
        set { m_Seekgoal = value; }
    }
    [SerializeField]
    private bool m_Obedient = true;
    public bool Obedient
    {
        get { return m_Obedient; }
        set { m_Obedient = value; }
    }
    [SerializeField]
    private bool m_Willful = false;
    public bool Willful
    {
        get { return m_Willful; }
        set { m_Willful = value; }
    }
    [Range(0, 200)]
    [SerializeField]
    private float m_NeighBourDistance = 50;
    public float NeighBourDistance
    {
        get { return m_NeighBourDistance; }
        set { m_NeighBourDistance = value; }
    }
    [Range(0.1f, 1)]
    [SerializeField]
    private float m_AntiNeighbourDistance = 1;
    public float AntiNeighbourDistance
    {
        get { return m_AntiNeighbourDistance; }
        set { m_AntiNeighbourDistance = value; }
    }
    [Range(0, 2)]
    [SerializeField]
    private float m_MaxForce = 0.5f;
    public float MaxForce
    {
        get { return m_MaxForce; }
        set { m_MaxForce = value; }
    }
    [Range(0, 5)]
    [SerializeField]
    private float m_MaxVelocity = 2.0f;
    public float MaxVelocity
    {
        get { return m_MaxVelocity; }
        set { m_MaxVelocity = value; }
    }
    [SerializeField]
    private float m_AttackDistance;

    private List<Unit> m_TeamBlue = new List<Unit>();
    private List<Unit> m_TeamRed = new List<Unit>();

    private void Start()
    {
        Instance = this;
        if (m_IsSimulation) StartMatch(GameMode.Match);
    }
    public void StartMatch(GameMode gamemode)
    {
        SimManager.Instance.ResetScore();
        Time.timeScale = m_TimeMultiplier;
        m_GameMode = gamemode;

        for (int i = 0; i < m_MainPositions.Count; i++)
        {
            Vector3 unitPos = new Vector3(Random.Range(-m_Range.x, m_Range.x), Random.Range(-m_Range.y, m_Range.y), 0);
            Unit unit = Instantiate(m_UnitPrefab, this.transform.position + unitPos, Quaternion.identity).GetComponent<Unit>();
            m_TeamBlue.Add(unit);
            unit.SetData(m_Goal[0], m_TeamBlue, true, m_MainPositions[i]);
            m_Units.Add(unit);
        }
        for (int j = 0; j < m_MainPositions.Count; j++)
        {
            Vector3 unitPos = new Vector3(Random.Range(-m_Range.x, m_Range.x), Random.Range(-m_Range.y, m_Range.y), 0);
            Unit unit = Instantiate(m_EnemyunitPrefab, this.transform.position + unitPos, Quaternion.identity).GetComponent<Unit>();
            m_TeamRed.Add(unit);
            unit.SetData(m_Goal[1], m_TeamRed, false, m_MainPositions[j]);
            m_Units.Add(unit);
        }
    }
    private void Update()
    {
        if (m_Ball.m_Owner == null)
        {
            GetClosestUnit(m_Units, m_Ball).m_Status = Status.Attacking;
        }
        for (int i = 0; i < m_Units.Count; i++)
        {
            if (m_Ball.m_CurrentTeam != m_Units[i].GetUnitTeam())
            {
                GetClosestUnit(m_Units, m_Ball).m_Status = Status.Attacking;
            }
        }
    }
    public void EndMatch(int score1, int score2)
    {
        Time.timeScale = 1;
        for (int i = 0; i < m_Units.Count; i++)
        {
            Destroy(m_Units[i]);
        }
        m_Units.Clear();
        if (score1 >= score2) SimManager.Instance.ShowEndResults(true);
        else SimManager.Instance.ShowEndResults(false);
    }
    Unit GetClosestUnit(List<Unit> units, Ball ball)
    {
        Unit tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = ball.GetPosition();
        foreach (Unit t in units)
        {
            float dist = Vector3.Distance(t.GetLocation(), currentPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }
    public float GetBasePosition(playerposition position, bool team)
    {
        float newposition = 0.0f;
        switch (position)
        {
            case playerposition.GK:
                if (team) newposition = 13;
                else newposition = 26;
                break;
            case playerposition.DF:
                if (team) newposition = 16;
                else newposition = 23;
                break;
            case playerposition.MF:
                newposition = 20;
                break;
            case playerposition.ST:
                if (team) newposition = 23;
                else newposition = 16;
                break;
        }
        if (m_Ball.m_CurrentTeam == team)
        {
            if (team) newposition += m_AttackDistance;
            else newposition -= m_AttackDistance;
        }

        return newposition;
    }
    public void ResetBall()
    {
        Vector2 unitPos = new Vector3(Random.Range(-m_Range.x, m_Range.x), Random.Range(-m_Range.y, m_Range.y));
        m_Ball.SetPosition(new Vector2(20, 0) + unitPos);
    }
    public Unit FindRightUnit(Unit thisunit, List<Unit> team, float posX, bool myteam)
    {
        Unit unit = null;
        for (int i = 0; i < team.Count; i++)
        {
            if (team[i] != thisunit)
            {
                if (myteam)
                {
                    if (team[i].GetLocation().x > posX)
                    {
                        unit = team[i];
                    }
                }
                else
                {
                    if (team[i].GetLocation().x < posX)
                    {
                        unit = team[i];
                    }
                }
            }
        }
        return unit;
    }
}
