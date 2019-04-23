using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace UnityStandardAssets.Characters.ThirdPerson
{



    public class Chase : MonoBehaviour
    {   
        
        public NavMeshAgent agent;
        public ThirdPersonCharacter character;
        public enum State
        {
            PATROL,
            CHASE 
        }
        public State state;
        private bool alive;

        public GameObject[] waypoints;
        private int waypointInd;
        public float patrolspeed = 0.5f;

        public float chasespeed = 1f;
        public GameObject target;

       void Start()
        {
  
            agent = GetComponent<NavMeshAgent>();
            character = GetComponent<ThirdPersonCharacter>();
            agent.updatePosition = true;
            agent.updateRotation = false;

            waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
            waypointInd = Random.Range(0, waypoints.Length);

            state = Chase.State.PATROL;
            alive = true;

            StartCoroutine("FSM");

     
        }
        IEnumerator FSM()
        {
            while (alive)
            {
                switch (state)
                {
                    case State.PATROL:
                        Patrol();
                        break;
                    case State.CHASE:
                        chase();
                        break;
                }
                yield return null;
            }
        }
        void Patrol() {
            agent.speed = patrolspeed;
            if (Vector3.Distance(this.transform.position, waypoints[waypointInd].transform.position) >= 2)
            {
                agent.SetDestination(waypoints[waypointInd].transform.position);
                character.Move(agent.desiredVelocity, false, false);
            }
            else if (Vector3.Distance(this.transform.position, waypoints[waypointInd].transform.position) <= 2)
            {
                waypointInd = Random.Range(0, waypoints.Length);
            } 
            else
            {
                character.Move(Vector3.zero, false, false);
            }
            
        }
        void chase()
        {
            agent.speed = chasespeed;
            agent.SetDestination(target.transform.position);
            character.Move(agent.desiredVelocity, false, false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                state = Chase.State.CHASE;
                target = other.gameObject;
            }
        }
    }
}
 