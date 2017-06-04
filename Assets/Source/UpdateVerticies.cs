using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateVerticies : MonoBehaviour {

	public float radius = 1.0F;
	public float force = 5.0F;

	private MeshFilter unappliedMesh;
	private List<Vector3> previousMousePos = new List<Vector3>();

	public enum FallOff { Gauss, Linear, Needle }
	public FallOff fallOff = FallOff.Needle;

	static float LinearFalloff (float distance , float inRadius) {
		return Mathf.Clamp01(1.0F - distance / inRadius);
	}

	static float GaussFalloff (float distance , float inRadius) {
		return Mathf.Clamp01 (Mathf.Pow (360.0F, -Mathf.Pow (distance / inRadius, 2.5F) - 0.01F));
	}

	static float NeedleFalloff (float dist, float inRadius) {
		return -(dist*dist) / (inRadius * inRadius) + 1.0F;
	}

	private void DeformMesh (Mesh mesh, Vector3 position, float power, float inRadius) {
		var vertices = mesh.vertices;
		var normals = mesh.normals;
		float sqrRadius = inRadius * inRadius;
		float sqrMagnitude, distance, falloff;

		// Calculate the movement vector	
		Vector3 movementVect = Vector3.zero;
		if (previousMousePos.Count > 0) {
			movementVect = position - previousMousePos[0];
			movementVect = movementVect.normalized;
			previousMousePos.RemoveAt (0);
		}
			
		// Deform vertices 
		for (int i=0;i<vertices.Length;i++) {
			sqrMagnitude = (vertices[i] - position).sqrMagnitude;
			// Early out if too far away
			if (sqrMagnitude > sqrRadius)
				continue;

			distance = Mathf.Sqrt(sqrMagnitude);
			switch (fallOff) {
			case FallOff.Gauss:
				falloff = GaussFalloff(distance, inRadius);
				break;
			case FallOff.Needle:
				falloff = NeedleFalloff(distance, inRadius);
				break;
			default:
				falloff = LinearFalloff(distance, inRadius);
				break;
			}

			vertices[i] += movementVect * falloff * power;
		}

		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		previousMousePos.Add(position);
	}

	void Update () {

		// When no button is pressed we update the mesh collider
		if (!Input.GetMouseButton (0)) {
			// Apply collision mesh when we let go of button
			ApplyMeshCollider();
			return;
		}

		// Did we hit the surface?
		RaycastHit hit;
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast (ray, out hit)) {
			MeshFilter filter = hit.collider.GetComponent<MeshFilter>();
			if (filter) {
				// Don't update mesh collider every frame since physX
				// does some heavy processing to optimize the collision mesh.
				// So this is not fast enough for real time updating every frame
				if (filter != unappliedMesh) {
					ApplyMeshCollider();
					unappliedMesh = filter;
				}

				// Deform mesh
				var relativePoint = filter.transform.InverseTransformPoint(hit.point);
				DeformMesh(filter.mesh, relativePoint, force * Time.deltaTime, radius);
			}
		}
	}

	private void ApplyMeshCollider () {
		if (unappliedMesh && unappliedMesh.GetComponent<MeshFilter>()) {
			unappliedMesh.GetComponent<MeshFilter>().mesh = unappliedMesh.mesh;
		}
		unappliedMesh = null;
	}
}