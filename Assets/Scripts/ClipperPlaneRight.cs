using UnityEngine;

//[ExecuteAlways]
public class ClipperPlaneRight : MonoBehaviour
{
    //materials we pass the values to
    public Material L1_mat;
    public Material L2_mat;
    public Material L3_mat;
    public Material L4_mat;
    public Material L5_mat;
    public Material L6_mat;
    public Material L7_mat;
    public Material L8_mat;
    public Material L9_mat;
    public Material R1_mat;
    public Material R2_mat;
    public Material R3_mat;
    public Material R4_mat;
    public Material R5_mat;
    public Material R6_mat;
    public Material R7_mat;
    public Material R8_mat;
    public Material R9_mat;

    //execute every frame
    void Update()
    {
        //create plane
        Plane plane = new Plane(transform.up, transform.position);
        //transfer values from plane to vector4
        Vector4 planeRepresentation = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
        //pass vector to shader
        L1_mat.SetVector("_PlaneRight", planeRepresentation);
        L2_mat.SetVector("_PlaneRight", planeRepresentation);
        L3_mat.SetVector("_PlaneRight", planeRepresentation);
        L4_mat.SetVector("_PlaneRight", planeRepresentation);
        L5_mat.SetVector("_PlaneRight", planeRepresentation);
        L6_mat.SetVector("_PlaneRight", planeRepresentation);
        L7_mat.SetVector("_PlaneRight", planeRepresentation);
        L8_mat.SetVector("_PlaneRight", planeRepresentation);
        L9_mat.SetVector("_PlaneRight", planeRepresentation);
        R1_mat.SetVector("_PlaneRight", planeRepresentation);
        R2_mat.SetVector("_PlaneRight", planeRepresentation);
        R3_mat.SetVector("_PlaneRight", planeRepresentation);
        R4_mat.SetVector("_PlaneRight", planeRepresentation);
        R5_mat.SetVector("_PlaneRight", planeRepresentation);
        R6_mat.SetVector("_PlaneRight", planeRepresentation);
        R7_mat.SetVector("_PlaneRight", planeRepresentation);
        R8_mat.SetVector("_PlaneRight", planeRepresentation);
        R9_mat.SetVector("_PlaneRight", planeRepresentation);
    }
}