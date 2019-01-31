using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField]
    private AudioClip m_TouchBallSound;

    public Unit m_Owner;
    [SerializeField]
    private float m_BallSpeed;
    private Vector2 m_Position;
    private Rigidbody2D m_RigidBody;

    public bool m_CurrentTeam;

    private float m_OwnerTimer;
    [SerializeField]
    private float m_MaxOwnerTime;

    private void Start()
    {
        m_RigidBody = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        m_Position = transform.position;
        m_RigidBody.velocity = Vector2.Lerp(m_RigidBody.velocity, Vector2.zero, 4 * Time.deltaTime);

        m_OwnerTimer += Time.deltaTime;
        if (m_OwnerTimer >= m_MaxOwnerTime)
        {
            m_Owner = null;
        }
    }
    public void SetOwner(Unit owner)
    {
        m_Owner = null;
        m_CurrentTeam = owner.GetUnitTeam();
        m_Owner = owner;
        m_OwnerTimer = 0;
    }
    public Vector2 GetPosition()
    {
        return m_Position;
    }
    public void SetPosition(Vector2 position)
    {
        transform.position = position;
        m_RigidBody.velocity = Vector2.zero;
    }
    public Unit GetOwner()
    {
        return m_Owner;
    }
    public void AddForce(Vector2 position, Vector2 force)
    {
        Vector2 direction = position - force;
        m_RigidBody.AddForce(-direction * m_BallSpeed * Time.deltaTime);
    }
    public void MoveToTarget(Vector2 target)
    {
        transform.position = Vector2.Lerp(transform.position, target, 5 * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Goal")
        {
            SimManager.Instance.UpdateScore(m_CurrentTeam);
            SimManager.Instance.ScoreGoal(m_Owner);
        }
    }
}
