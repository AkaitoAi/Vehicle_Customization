using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnableSelected : MonoBehaviour
{
    [SerializeField] private IntVariable index;
    [SerializeField] private Transform parent;
    [SerializeField] private Button loadSceneButton;

    private void Start()
    {
        parent.GetChild(index.value).gameObject.SetActive(true);

        loadSceneButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1));
    }
}
