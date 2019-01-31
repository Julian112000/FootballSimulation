using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimManager : MonoBehaviour
{
    public static SimManager Instance;
    [SerializeField]
    private AudioClip m_ScoreAudio;


    private float m_GameTime;
    private int m_EnemyScore;
    private int m_PlayerScore;

    [Header("Game Settings")]
    [SerializeField]
    private float m_MaxGameTime;
    [SerializeField]
    private float m_TimeMultiplier;

    [Header("UI Settings")]
    [SerializeField]
    private Text m_ScoreText;
    [SerializeField]
    private Text m_TimeText;
    [SerializeField]
    private Text m_ResultText;
    [SerializeField]
    private GameObject m_ResultPanel;

    public bool m_GameRunning;

    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        m_TimeText.text = "" + (int)m_GameTime + "'";

        if (m_GameTime < m_MaxGameTime)
        {
            m_GameRunning = true;
            m_GameTime += Time.deltaTime * m_TimeMultiplier;
        }
        else
        {
            Simulator.Instance.EndMatch(m_PlayerScore, m_EnemyScore);
        }
    }
    public void ShowEndResults(bool team)
    {
        m_ResultPanel.SetActive(true);
        if (team)
        {
            m_ResultText.text = "Team Yellow Wins!";
        }
        else
        {
            m_ResultText.text = "Team Red Wins!";
        }
    }
    public void Restart()
    {
        Application.LoadLevel(0);
    }
    public void ScoreGoal(Unit unit)
    {
        Debug.Log("Scored: " + unit.gameObject.name);
    } 
    public void ResetScore()
    {
        m_PlayerScore = 0;
        m_EnemyScore = 0;
        m_GameTime = 0;

        m_ScoreText.text = m_PlayerScore + " - " + m_EnemyScore;
    }
    public void UpdateScore(bool team)
    {
        if (team)
            m_PlayerScore++;
        else m_EnemyScore++;

        m_ScoreText.text = m_PlayerScore + " - " + m_EnemyScore;
        Simulator.Instance.ResetBall();
    }
}
