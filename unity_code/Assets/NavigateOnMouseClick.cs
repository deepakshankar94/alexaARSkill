// Based on: http://wiki.unity3d.com/index.php/Click_To_Move_C by Vinicius Rezendrix
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavigateOnMouseClick : MonoBehaviour
{

    public enum MouseButtonType { Left, Right, Middle };
    public MouseButtonType mouseButton = MouseButtonType.Left;
    public string speedParameter = "moving";
    public float distanceThreshold = 0.5f;
    public Transform opponent;

    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private Quaternion defaultRotation;

    void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        defaultRotation = transform.rotation;
    }

    void Update()
    {
        var speed = (navMeshAgent.remainingDistance < distanceThreshold) ? 0 : 1;

        float step;

        if (speed == 0)
        {
            step = 100 * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, defaultRotation, step);
        }
        else
        {
            step = 0;    
        }
        
        if (animator != null) animator.SetInteger(speedParameter, speed);

        // Moves the Player if the Mouse Button was clicked:
        if (Input.GetMouseButtonDown((int)mouseButton) && GUIUtility.hotControl == 0)
        {
            Plane playerPlane = new Plane(Vector3.up, transform.position);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float hitdist = 0.0f;
            if (playerPlane.Raycast(ray, out hitdist))
            {
                navMeshAgent.SetDestination(ray.GetPoint(hitdist));
            }
        }

        // Moves the player if the mouse button is held down:
        else if (Input.GetMouseButton((int)mouseButton) && GUIUtility.hotControl == 0)
        {
            Plane playerPlane = new Plane(Vector3.up, transform.position);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float hitdist = 0.0f;
            if (playerPlane.Raycast(ray, out hitdist))
            {
                navMeshAgent.SetDestination(ray.GetPoint(hitdist));
            }
        }
    }
}