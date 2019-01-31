using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Status
{
    Attacking,
    Defending,
    Returning
};
public class Unit : MonoBehaviour
{
    private Simulator m_Manager;
    public playerposition m_PlayerPosition;
    public Status m_Status;

    //Position
    private Vector2 m_Location = Vector2.zero;
    public Vector2 GetLocation()
    {
        return m_Location;
    }
    private float m_MainBasePosition;

    //Physics
    private Vector2 m_Velocity;
    private Vector2 m_CurrentForce;
    private Rigidbody2D m_Rigidbody;

    [SerializeField]
    private Transform m_Goal;
    [SerializeField]
    private bool m_UnitTeam;
    public bool GetUnitTeam()
    {
        return m_UnitTeam;
    }

    private List<Unit> m_TeamMates;

    private bool m_Returning;

    void Awake()
    {
        m_Manager = Simulator.Instance;
        m_Velocity = new Vector2(Random.Range(0.01f, 0.1f), Random.Range(0.01f, 0.1f));
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }
    public void SetData(Transform goal, List<Unit> team, bool myteam, playerposition position)
    {
        name = "AI - " + Random.Range(0, 555);
        m_PlayerPosition = position;

        m_UnitTeam = myteam;
        m_Goal = goal;
        m_TeamMates = team;
        m_MainBasePosition = m_Manager.GetBasePosition(m_PlayerPosition, myteam);
    }
    Vector2 Seak(Vector2 target)
    {
        return (target - m_Location);
    }
    Vector2 SetDirection(Transform target, bool isgoal)
    {
        Vector2 direction = Vector2.zero;
        direction = (target.position - transform.position);
        if (isgoal) direction += new Vector2(0, Random.Range(-4, 4));

        return direction;
    }
    void ApplyForce(Vector2 f)
    {
        Vector3 force = new Vector3(f.x, f.y, 0);
        if (force.magnitude > m_Manager.MaxForce)
        {
            force = force.normalized;
            force *= m_Manager.MaxForce;
        }
        this.GetComponent<Rigidbody2D>().AddForce(force);

        if (this.GetComponent<Rigidbody2D>().velocity.magnitude > m_Manager.MaxVelocity)
        {
            this.GetComponent<Rigidbody2D>().velocity = this.GetComponent<Rigidbody2D>().velocity.normalized;
            this.GetComponent<Rigidbody2D>().velocity *= m_Manager.MaxVelocity;
        }

        Debug.DrawRay(this.transform.position, force, Color.white);
    }
    Vector2 align()
    {
        float neighbourDist = m_Manager.NeighBourDistance;
        Vector2 sum = Vector2.zero;
        foreach (Unit other in m_Manager.GetUnits)
        {
            if (other == this.gameObject) continue;

            float d = Vector2.Distance(m_Location, other.m_Location);
        }
        return Vector2.zero;
    }
    void Flock(Vector2 goal)
    {
        m_Location = this.transform.position;
        m_Velocity = this.GetComponent<Rigidbody2D>().velocity;

        foreach (Unit other in m_Manager.GetUnits)
        {
            if (other == this.gameObject) continue;

            float d = Vector2.Distance(m_Location, other.m_Location);
        }
        if (m_Manager.Obedient)
        {
            Vector2 ali = align();
            Vector2 gl;
            if (m_Manager.Seekgoal)
            {
                gl = Seak(goal);
                m_CurrentForce = gl + ali;
            }
            else
            {
                m_CurrentForce = ali;
            }
        }
        if (m_Manager.Willful && Random.Range(0, 50) <= 1)
        {
            if (Random.Range(0, 50) <= 1)
            {
                m_CurrentForce = new Vector2(Random.Range(0.01f, 0.1f), Random.Range(0.01f, 0.1f));
            }
        }

        ApplyForce(m_CurrentForce);
    }
    void ReturnToBasePosition(Vector2 position)
    {
        if (m_Location != position)
            m_Returning = true;
        else m_Returning = false;

        if (m_Returning)
        {
            Flock(position += new Vector2(0, Random.Range(-3, 3)));
        }
        else
        {
            m_Status = Status.Defending;
        }
    }
    void Update()
    {
        m_Location = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
        if (m_UnitTeam)
        {
            if (transform.position.x < m_MainBasePosition && m_Manager.GetBall().m_CurrentTeam == m_UnitTeam || m_Status == Status.Attacking)
            {
                Flock(m_Manager.GetBall().GetPosition());
            }
            else
            {
                ReturnToBasePosition(new Vector2(m_MainBasePosition, 0));
            }
        }
        else
        {
            if (transform.position.x > m_MainBasePosition && m_Manager.GetBall().m_CurrentTeam != m_UnitTeam || m_Status == Status.Attacking)
            {
                Flock(m_Manager.GetBall().GetPosition());
            }
            else
            {
                ReturnToBasePosition(new Vector2(m_MainBasePosition, 0));
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            Ball ball = collision.gameObject.GetComponent<Ball>();
            Unit unit = m_Manager.FindRightUnit(this, m_TeamMates, m_Location.x, m_UnitTeam);
            ball.SetOwner(this);
            m_Status = Status.Defending;

            if (unit)
            {
                ball.MoveToTarget(unit.m_Location);    
            }
            else
            {
                ball.AddForce(transform.position, m_Goal.position);
            }
        }
    }
}
