using TMPro;
using UnityEngine;

public class TextHealthCurrent : MonoBehaviour
{
    private TextMeshProUGUI healthText;
    // Start is called before the first frame update
    void Start()
    {
        healthText = GetComponent<TextMeshProUGUI>();
    }
    // Update is called once per frame
    void Update()
    {
        GameObject player = GameObject.Find("Player");
        healthText.text = player.GetComponent<Health>().GetCurrentHealth().ToString();
    }
}
