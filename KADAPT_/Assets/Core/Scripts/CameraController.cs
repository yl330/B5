using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float xspeed = -0.05f; 
    private float yspeed = 0.1f;   

    private Vector3 center; 

    enum RotationAxes { MouseXAndY, MouseX, MouseY }
    RotationAxes axes = RotationAxes.MouseXAndY;

    float sensitivityX = 5;
    float sensitivityY = 5;
    float sensitivityC = 12;
    float minimumY = -80;
    float maximumY = 80;
    private float rotationY = 0;
    public float min_distance = 1; //×î´óÐ¡¾àÀë
    public float max_distance = 150; // ×î´ó¾àÀë

    public float panSpead = 20f;
    public float panBorderThickness = 10f;

    float inputx;
    public float inputy;

    private void Start()
    {
        center = Vector3.zero;
    }

    private void Update()
    {
        Vector3 pos = transform.position;
        if (Input.GetKey("w"))
        {
            pos.z += panSpead * Time.deltaTime;
        }
        if (Input.GetKey("s"))
        {
            pos.z -= panSpead * Time.deltaTime;
        }
        if (Input.GetKey("a"))
        {
            pos.x -= panSpead * Time.deltaTime;
        }
        if (Input.GetKey("d"))
        {
            pos.x += panSpead * Time.deltaTime;
        }
        if (Input.GetKey("space"))
        {
            pos.y += panSpead * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightShift))
        {
            pos.y -= panSpead * Time.deltaTime;
        }
        transform.position = pos;
        inputx = Input.GetAxis("Horizontal");
        inputy = Input.GetAxis("Vertical");
        
        var c = Camera.main;
        if (Input.GetAxis("Mouse ScrollWheel") > 0) // ÊÓ½ÇÀ­½ü
        {
            float d = Vector3.Distance(center, transform.position);
            if (d >= min_distance)
            {
                var dir = transform.position - center;
                dir = dir.normalized * (d - 10 * sensitivityC * Time.deltaTime);
                transform.position = dir + center;
                if (d <= min_distance)
                {
                    dir = dir.normalized * (min_distance);
                    transform.position = dir + center;
                }
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0) // ÊÓ½ÇÀ­Ô¶
        {
            float d = Vector3.Distance(center, transform.position);
            if (Camera.main.fieldOfView <= max_distance)
            {
                var dir = transform.position - center;
                dir = dir.normalized * (d + 10 * sensitivityC * Time.deltaTime);
                transform.position = dir + center;
                if (c.fieldOfView >= max_distance)
                {
                    dir = dir.normalized * (max_distance);
                    transform.position = dir + center;
                }
            }
        }

    }
}


