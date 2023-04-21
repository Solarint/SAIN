using UnityEngine;

namespace SAIN.Movement.Helpers
{
    public static class Draw
    {
        //All Credit to Drakia here, even if its simple
        public static GameObject Sphere(Vector3 position, float size, Color color)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.GetComponent<Renderer>().material.color = color;
            sphere.GetComponent<Collider>().enabled = false;
            sphere.transform.position = new Vector3(position.x, position.y, position.z); ;
            sphere.transform.localScale = new Vector3(size, size, size);

            return sphere;
        }
        public static GameObject Line(Vector3 start, Vector3 end, float size, Color color)
        {
            var line = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //line.GetComponent<Renderer>().material.color = color;
            //line.GetComponent<Collider>().enabled = false;
            //line.transform.position = new Vector3(position.x, position.y, position.z); ;
            //line.transform.localScale = new Vector3(size, size, size);

            return line;
        }
    }
}
