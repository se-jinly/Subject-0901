using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearUI : MonoBehaviour
{
    public void Restart()
    {
        SceneManager.LoadScene("Battle");
    }
    public void GoTitle()
    {
        SceneManager.LoadScene("Title");
    }
}
