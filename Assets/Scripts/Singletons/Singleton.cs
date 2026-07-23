using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T _instance;

    [SerializeField]
    private bool _isGlobal = false;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Debug.Log($"Destroying singleton {gameObject.name}");
            Debug.Log($"Singleton already exists with name {_instance.name}");
            Destroy(transform.gameObject);
        }

        if (_isGlobal) {
            DontDestroyOnLoad(transform.gameObject);
        }
    }

    public static T Instance {
        get {
            if (_instance == null) {
                _instance = (T)FindFirstObjectByType(typeof(T));

                if (_instance == null) {
                    //GameObject obj = new GameObject();
                    //_instance = obj.AddComponent<T>();
                    //obj.name = typeof(T).ToString();
                }
            }
            return _instance;
        }
    }
}