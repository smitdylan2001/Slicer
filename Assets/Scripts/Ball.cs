using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Vector3 move;
    Vector3 prevPos;
    Vector3 direction;
    // Start is called before the first frame update
    void Start()
    {
        prevPos = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKey(KeyCode.W))
		{
            move = new Vector3(-1, move.y, move.z);
		}
        if (Input.GetKey(KeyCode.A))
        {
            move = new Vector3(move.x, move.y, -1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            move = new Vector3(1, move.y, move.z);
        }
        if (Input.GetKey(KeyCode.D))
        {
            move = new Vector3(move.x, move.y, 1);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            move = new Vector3(move.x, 1, move.z);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            move = new Vector3(move.x, -1, move.z);
        }

        transform.position += (move *10* Time.deltaTime);
    }

	private void FixedUpdate()
	{
        direction = prevPos - transform.position;
        prevPos = transform.position;
        
	}

	private void OnCollisionEnter(Collision collision)
	{
        

		if (collision.gameObject.CompareTag("Destructable"))
		{
            var script = collision.gameObject.GetComponent<MeshDestroy>();
            gameObject.GetComponent<Rigidbody>();
            script.IncomingCollision(script.CutCascades, script.ExplodeForce, -direction*10, transform.position );
		}
    }
}
