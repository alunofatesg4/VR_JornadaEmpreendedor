using UnityEngine;
using System.Collections.Generic;

public class GlueBoard : MonoBehaviour
{
    [Header("ID do Quadro (tema)")]
    public int boardID;

    private List<StickyObject> postIts = new List<StickyObject>();

    /// Verifica se o quadro pode aceitar este post-it
    public bool CanAcceptPostIt(StickyObject postIt)
    {
        if (boardID == 1 || boardID == 2)
        {
            // Quadro só aceita 1 post-it
            return postIts.Count == 0;
        }
        else if (boardID == 3)
        {
            // Quadro pode ter até 3
            return postIts.Count < 3;
        }
        return false;
    }

    public void AddPostIt(StickyObject postIt)
    {
        if (!postIts.Contains(postIt))
        {
            postIts.Add(postIt);
            GameManager.Instance.UpdateScore();
        }
    }

    public void RemovePostIt(StickyObject postIt)
    {
        if (postIts.Contains(postIt))
        {
            postIts.Remove(postIt);
            GameManager.Instance.UpdateScore();
        }
    }

    /// Calcula a pontuação deste quadro
    public int GetScore()
    {
        if (boardID == 1 || boardID == 2)
        {
            if (postIts.Count == 0) return 0;
            if (postIts.Count > 1) return 0; // segurança

            return (postIts[0].noteID == boardID) ? 25 : 0;
        }
        else if (boardID == 3)
        {
            if (postIts.Count == 0) return 0;

            int points = 0;
            foreach (var p in postIts)
            {
                if (p.noteID == boardID)
                    points += 10;
            }

            return points;
        }

        return 0;
    }

}
