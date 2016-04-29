using UnityEngine;

public class CameraController : MonoBehaviour {

    void Update()
    {
        if (Input.GetKeyDown("left"))
        {
            transform.Translate(Vector3.left);
        }
        else if (Input.GetKeyDown("right"))
        {
            transform.Translate(Vector3.right);
        }
        else if (Input.GetKeyDown("up"))
        {
            transform.Translate(Vector3.up);
        }
        else if (Input.GetKeyDown("down"))
        {
            transform.Translate(Vector3.down);
        }

        transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel"));
    }
}
