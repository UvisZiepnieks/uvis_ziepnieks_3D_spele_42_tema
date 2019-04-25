using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;
using UnityEngine.UI;



namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]

    public class FirstPersonController : MonoBehaviour
    {
        private double heartbeat;
        private int minuspill;
        private int pluspill;
        private int batteries;
        private int batteryhealth;
        public Text minuspilltext;
        public Text pluspilltext;
        public Text heartbeattext;
        public Text batteriestext;
        public Text batterytext;
        public Text GameEnder;
        public GameObject light;
        float counter = 0;
        float counter2 = 0;
        float counter3 = 0;


        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;
     


        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
            m_MouseLook.Init(transform, m_Camera.transform);
            heartbeat = 85;
            minuspill = 1;
            pluspill = 1;
            batteries = 1;
            batteryhealth = 100;
            SetHeartBeatText();
        

        }


        // Update is called once per frame
        private void Update()
        {
            counter += Time.deltaTime;
            counter2 += Time.deltaTime;
            counter3 += Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftShift))
            {
               

                if (counter >= 1)
                {
                    //Increment Speed by 4
                    incrementSpeed();

                    //RESET Counter
                    counter = 0;
                }
            }


            if (counter2 >= 5)
            {
                //Increment Speed by 4
                incrementSpeed2();

                //RESET Counter
                counter2 = 0;
            }
            if (light.activeInHierarchy == true)
            {
                if (counter3 >= 5)
                {
                    //Increment Speed by 4
                    incrementSpeed3();

                    //RESET Counter
                    counter3 = 0;
                }
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (minuspill > 0)
                {
                    minuspill = minuspill - 1;
                    heartbeat = heartbeat - 15;
                    setMinusPillText();
                    SetHeartBeatText();
                }
            }
            if (Input.GetKeyDown(KeyCode.X))
            {

                if (pluspill > 0)
                {
                    pluspill = pluspill - 1;
                    heartbeat = heartbeat + 15;
                    setPlusPillText();
                    SetHeartBeatText();
                }
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (batteries > 0)
                {
                    batteries = batteries - 1;
                    batteryhealth = 100;
                    setBatteriestext();
                    setBatteryHealthtext();
                }
               
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
               
                if (light.activeInHierarchy == true)
                {
                    light.SetActive(false);

                }
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (light.activeInHierarchy == false)
                {
                    light.SetActive(true);

                }
            }
            if (batteryhealth < 1)
            {
                light.SetActive(false);
            }
            

            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);

            m_MouseLook.UpdateCursorLock();
        }


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            m_MouseLook.LookRotation(transform, m_Camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }

        private void OnTriggerEnter(Collider other)
        {


            if (other.gameObject.CompareTag("Minus_Pill"))
            {
                other.gameObject.SetActive(false);
                minuspill = minuspill + 1;
                setMinusPillText();

            }
            if (other.gameObject.CompareTag("Plus_Pill"))
            {
                other.gameObject.SetActive(false);
                pluspill = pluspill + 1;
                setPlusPillText();

            }
            if (other.gameObject.CompareTag("Battery"))
            {
                other.gameObject.SetActive(false);
                batteries = batteries + 1;
                setBatteriestext();

            }
            if (other.gameObject.CompareTag("Finish"))
            {
                GameEnder.text = "YOU ESCAPED!";
            }
        }
        void SetHeartBeatText()
        {
            heartbeattext.text = "Heart Beat: " + heartbeat.ToString();
            if (heartbeat > 156)
            {
                GameEnder.text = "YOU DIED!";
                Time.timeScale = 0;

            }
            if (heartbeat < 40)
            {
                GameEnder.text = "YOU DIED!";
                Time.timeScale = 0;


            }
        }
        void incrementSpeed()
        {
            if(heartbeat > 40 && heartbeat < 85)
            {
                heartbeat += 5;
            }
            if (heartbeat >= 85 && heartbeat < 120)
            {
                heartbeat += 3;
            }
            if (heartbeat >= 120)
            {
                heartbeat += 5;
            }

            SetHeartBeatText();
        }
        void incrementSpeed2()
        {
            if (heartbeat > 40 && heartbeat < 60)
            {
                heartbeat += 3;
            }
            if (heartbeat >= 85 && heartbeat < 120)
            {
                heartbeat += 1;
            }
            if (heartbeat >= 120)
            {
                heartbeat = heartbeat - 1;
            }

            SetHeartBeatText();
        }
        void incrementSpeed3()
        {
            batteryhealth -= 1;

            setBatteryHealthtext();
        }
        void setMinusPillText()
        {
            minuspilltext.text = "Minus Pill: " + minuspill.ToString();
        }
        void setPlusPillText()
        {
            pluspilltext.text = "Plus Pill: " + pluspill.ToString();
        }
        void setBatteriestext()
        {
            batteriestext.text = "Batteries: " + batteries.ToString();
        }
        void setBatteryHealthtext()
        {
            batterytext.text = "Battery Life: " + batteryhealth.ToString();
        }
      
    }
}

