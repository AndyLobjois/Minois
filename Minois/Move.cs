using System;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;
using Warudo.Plugins.Core.Assets.Character;

namespace AndyHellgrim.Minois {
    [NodeType(Id = "AndyHellgrim.Minois.Move", Title = "Minois - Move", Category = "Minois", Width = 1.5f)]
    public class MinoisMove : Node {
        // Character
        [DataInput, HideLabel] public CharacterAsset Character;

        // Parameters
        [DataInput, FloatSlider(0, 10)] public float Speed = 2;
        [DataInput] public Vector2 StageLimits = new Vector2(2, 2);

        // Animations Lists
        [DataInput] public Emote Idle;
        [DataInput] public Emote Run;
        [DataInput, Label("Run Sound Repeat (s)")] public float RunSoundRepeat = 0;
        [DataInput] public Emote Sit;

        // Animation Parameters
        [DataInput, FloatSlider(0, 1)] public float Deadzone = 0.1f;

        // Inputs
        [DataInput, Label("Sit")] public bool SitButton = false;
        [DataInput] public Vector3 CharacterPosition;

        // Outputs
        [FlowOutput] public Continuation TriggerAnimation;
        [FlowOutput] public Continuation TriggerSound;
        [DataOutput] public string AnimationClip() { return animationClip; }
        [DataOutput] public string AudioClip() { return audioClip; }

        // Toggle Gizmo
        [Trigger] public void ToggleGizmo() { floor.SetActive(!floor.activeSelf); }

        // Parameters
        MinoisData data;
        GameObject floor;
        float distance = 0;
        string animationClip = "";
        string audioClip = "";
        bool isIdle = false;
        bool isRun = false;
        bool isSit = false;
        bool sitButtonPress = false;


        protected override void OnCreate() {
            floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
            floor.name = "AndyHellgrim_Move_Floor";
            UnityEngine.Object.Destroy(floor.GetComponent<MeshCollider>());
            floor.transform.eulerAngles = new Vector3(90, 0, 0);
            floor.GetComponent<MeshRenderer>().material.shader = Shader.Find("Sprites/Default");
            floor.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(0.8f, 0.1f, 0.1f, 0.25f));
            floor.SetActive(false);
        }

        protected override void OnDestroy() {
            UnityEngine.Object.Destroy(floor);
        }

        bool isStart = false;
        public override void OnLateUpdate() {
            base.OnLateUpdate();
            InvokeFlow(null);

            // Get Data
            if (data == null) {
                data = GameObject.Find("Minois_Emotes").gameObject.GetComponent<MinoisData>();
            }

            // Main Logic
            Move();
            PlayAnimationAndSound();
            CheckRunSoundRepeat();
            Floor();

            // Start
            if (!isStart) {
                isStart = true;
                InvokeFlow("TriggerAnimation");
            }
        }

        void Move() {
            Vector2 joy = new Vector2(CharacterPosition.x * -1, CharacterPosition.y);
            distance = Vector2.Distance(Vector2.zero, joy);

            // Deadzone
            if (distance < Deadzone) {
                joy = new Vector2(0, 0);
            }

            // Position
            Character.Transform.Position = Vector3.Lerp(
                Character.GameObject.transform.position,
                Character.GameObject.transform.position + new Vector3(joy.x, 0, joy.y) * Speed * distance * Convert.ToInt32(!isSit),
                Time.deltaTime);

            // Clamp
            Character.Transform.Position.x = Mathf.Clamp(Character.Transform.Position.x, -StageLimits.x * 0.5f, StageLimits.x * 0.5f);
            Character.Transform.Position.z = Mathf.Clamp(Character.Transform.Position.z, -StageLimits.y * 0.5f, StageLimits.y * 0.5f);

            // Rotation
            if (distance > Deadzone) {
                Character.Transform.Rotation = Quaternion.LookRotation(new Vector3(joy.x, 0, joy.y), Vector3.up).eulerAngles * Convert.ToInt32(!isSit);
            }
        }

        void PlayAnimationAndSound() {
            /// Idle / Run
            if (!isSit) { // Check sit
                if (distance < Deadzone) { // Check deadzone
                    if (!isIdle) {
                        isIdle = true;
                        isRun = false;
                        CheckTrigger(ref Idle);
                    }
                } else {
                    if (!isRun) {
                        isIdle = false;
                        isRun = true;
                        CheckTrigger(ref Run);
                    }
                }
            }

            /// Sit
            if (SitButton && !sitButtonPress) {
                sitButtonPress = true;
                isSit = !isSit;
                isIdle = isRun = false;
                CheckTrigger(ref Sit);
            } else if (!SitButton && sitButtonPress) {
                sitButtonPress = false;
            }
        }

        void CheckTrigger(ref Emote emote) {
            if (emote.Animation != null) {
                animationClip = emote.Animation;
                InvokeFlow("TriggerAnimation");
            }

            if (emote.Sound != null) {
                audioClip = emote.Sound;

                if (emote != Run) {
                    InvokeFlow("TriggerSound");
                }
            }

            // Look Enable/Disable
            data.LookEnable = emote.LookEnable;
        }

        float timer = 0;
        void CheckRunSoundRepeat() {
            if (isRun) {
                timer += Time.deltaTime;

                if (RunSoundRepeat != 0) {
                    if (timer > RunSoundRepeat) {
                        timer = 0;
                        InvokeFlow("TriggerSound");
                    }
                }
            }
        }

        void Floor() {
            floor.transform.localScale = new Vector3(StageLimits.x, StageLimits.y, 1);
        }
    }
}