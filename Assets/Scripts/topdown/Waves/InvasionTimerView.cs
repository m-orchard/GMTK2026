using UnityEngine;
using TMPro;

public class InvasionTimerView : MonoBehaviour
{
    [SerializeField] private Timer timer;
    [SerializeField] private TextMeshProUGUI locationLabel;

    public Timer Timer => timer;

    public void SetLabel(string text)
    {
        if (locationLabel != null)
        {
            locationLabel.text = text;
        }
    }
}
