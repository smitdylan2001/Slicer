using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Vector3 move;
    Vector3 prevPos;
    Vector3 direction;
    static AudioSource ac;

    // Start is called before the first frame update
    void Start()
    {
        ac = GameObject.Find("Audio Source").GetComponent<AudioSource>();

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
        if (Input.GetKeyUp(KeyCode.W))
        {
            move = new Vector3(0, move.y, move.z);
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            move = new Vector3(move.x, move.y, 0);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            move = new Vector3(0, move.y, move.z);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            move = new Vector3(move.x, move.y, 0);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            move = new Vector3(move.x, 0, move.z);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            move = new Vector3(move.x, 0, move.z);
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
            script.IncomingCollision(script.CutCascades, script.ExplodeForce, -direction * 10, transform.position);
		}
    }

    public void StartRoutine()
	{
        StartCoroutine(StopAudio());

    }

    public IEnumerator StopAudio()
    {
        yield return new WaitForSeconds(4f);
        ac.volume /= 1.03f;
        yield return new WaitForSeconds(3f);
        ac.volume /= 1.06f;
        yield return new WaitForSeconds(5f);
        ac.volume /= 1.07f;

        if (ac.volume < 0.01)
        {
            ac.Stop();
            ac.volume = 0;
        }
        yield return null;
    }
}
