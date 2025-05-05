using UnityEngine;

public class FreeFlight : MonoBehaviour
{
	[SerializeField]
	public float mouseSpeed = 3;

	[SerializeField]
	public float moveSpeed = 8;

	private float _xRot, _yRot;

	private void Start()
	{
		Vector3 currentRot = transform.rotation.eulerAngles;
		_xRot = currentRot.x;
		_yRot = currentRot.y;
	}

	private void Update()
	{
		float mx = Input.GetAxis("Mouse X");
		float my = Input.GetAxis("Mouse Y");

		if (Input.GetMouseButton(1))
		{
			_xRot -= my * mouseSpeed;
			_yRot += mx * mouseSpeed;
		}

		transform.rotation = Quaternion.Euler(_xRot, _yRot, 0);

		float dx = Input.GetAxis("Horizontal");
		float dy = Input.GetAxis("Vertical");
		float dz = (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftShift) ? -1 : 0) + (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) ? 1 : 0);
		float multiplier = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 3 : 1;
		Vector3 transformForward = transform.forward * (moveSpeed * dy);
		Vector3 transformRight = transform.right * (moveSpeed * dx);
		Vector3 transformUp = transform.up * (moveSpeed * dz);
		transform.position += multiplier * Time.deltaTime * (transformForward + transformRight + transformUp);
	}
}
