using UnityEngine;

public class RawInputTest : MonoBehaviour
{
    void Update()
    {
        if (Input.anyKey)
        {
            Debug.Log("ANY KEY PRESSED");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("SPACE DOWN");
        }
    }
}
