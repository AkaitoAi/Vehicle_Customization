using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AkaitoAi.Customization
{
    public class SelectionHandler : MonoBehaviour
    {
        [SerializeField] private IntVariable index;
        [SerializeField] private GameObject[] objs;
        [SerializeField] private Button increment, decrement, loadSceneButton;

        private int currentIndex;

        public static event Action<int> OnCurrentIndexUpdate;

        private void Start()
        {
            DisableAllObjs();
            objs[currentIndex].SetActive(true);
            OnCurrentIndexUpdate?.Invoke(currentIndex);
            
            decrement.onClick.AddListener(OnDecrementButton);
            increment.onClick.AddListener(OnIncrementButton);
            loadSceneButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
        }

        private void DisableAllObjs()
        {
            foreach (GameObject obj in objs)
                obj.SetActive(false);
        }

        private void OnIncrementButton()
        {
            DisableAllObjs();

            currentIndex++;

            if (currentIndex > objs.Length - 1)
                currentIndex = 0;

            objs[currentIndex].SetActive(true);

            OnCurrentIndexUpdate?.Invoke(currentIndex);

            index.value = currentIndex;
        }

        private void OnDecrementButton()
        {
            DisableAllObjs();

            currentIndex--;

            if (currentIndex <= 0)
                currentIndex = objs.Length - 1;

            objs[currentIndex].SetActive(true);

            OnCurrentIndexUpdate?.Invoke(currentIndex);

            index.value = currentIndex;
        }
    }
}
