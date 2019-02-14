using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBall : Ball
{
    public float force;

    // Update is called once per frame
    void Update()
    {
        // Input handling
        if (Input.GetKeyDown("space"))
        {
            rb.AddForce(transform.forward * force,ForceMode.Impulse);
        }
    }
}
