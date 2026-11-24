using UnityEngine;

public class SineWaveMovement : MonoBehaviour

{

    public float amplitude = 1f;

    public float frequency = 1f;

    public float zSpeed = 1f;

    public float zLimit = 4f;

    private Vector3 startPosition;

    private Rigidbody rb;

    private bool movingForward = true;

    void Start()

    {

        startPosition = transform.position;

        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;

    }

    void FixedUpdate()

    {

        float x = startPosition.x + Mathf.Sin(Time.fixedTime * frequency) * amplitude;

        float z = transform.position.z + zSpeed * Time.deltaTime * (movingForward ? 1 : -1);

        Vector3 newPosition = new Vector3(x, startPosition.y, z);

        rb.MovePosition(newPosition);

        if (movingForward && z >= startPosition.z + zLimit)

            movingForward = false;

        else if (!movingForward && z <= startPosition.z)

            movingForward = true;

    }

}