using System;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;
using Warudo.Core.Server;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Character;

namespace AndyHellgrim.Minois {
    [NodeType(Id = "AndyHellgrim.Minois.Look", Title = "Minois - Look", Category = "Minois", Width = 1.5f)]
    public class MinoisLook : Node {
        // Character
        [DataInput, HideLabel] public CharacterAsset Character;
        [DataInput] public GameObjectAsset TargetOverride;
        [DataInput, Hidden] public bool isHumanoid = false;

        // Generic Armature
        [DataInput, HiddenIf("isHumanoid", Is.True), Description("Eye.L, Eye.R, Head, Body")] public string[] BoneFilters = new string[4] { "eye.l", "eye.r", "head", "upperchest" };
        [DataInput, HiddenIf("isHumanoid", Is.True)] public Vector3 EyesOffset = new Vector3(0, 0, 0);
        [Trigger, HiddenIf("isHumanoid", Is.True), Label("Find Bones (Generic)")] public void FindGenericBones() {
            // Reset
            allBones = "";
            eyel = eyer = head = body = null;

            // Search
            RecursiveSearch(Character.GameObject.transform);

            // Notification
            isSearch = true;
            Notification();
        }

        // Humanoid Armature
        public enum BodyBone { None, Hips, Spine, Chest, UpperChest, Neck }
        [DataInput, HiddenIf("isHumanoid", Is.False), Label("Body Bone")] public BodyBone bodyBone = BodyBone.UpperChest;
        [Trigger, HiddenIf("isHumanoid", Is.False), Label("Find Bones (Humanoid)")] public void FindHumanoidBones() {
            // Reset
            allBones = "";
            eyel = eyer = head = body = null;

            // Search (only for allBones string)
            RecursiveSearch(Character.GameObject.transform);

            // Search
            if (Character.Animator.GetBoneTransform(HumanBodyBones.LeftEye)) { eyel = Character.Animator.GetBoneTransform(HumanBodyBones.LeftEye).gameObject; }
            if (Character.Animator.GetBoneTransform(HumanBodyBones.RightEye)) { eyer = Character.Animator.GetBoneTransform(HumanBodyBones.RightEye).gameObject; }
            if (Character.Animator.GetBoneTransform(HumanBodyBones.Head)) { head = Character.Animator.GetBoneTransform(HumanBodyBones.Head).gameObject; }

            if (bodyBone == BodyBone.Hips) { if (Character.Animator.GetBoneTransform(HumanBodyBones.Hips)) { body = Character.Animator.GetBoneTransform(HumanBodyBones.Hips).gameObject; } }
            if (bodyBone == BodyBone.Spine) { if (Character.Animator.GetBoneTransform(HumanBodyBones.Spine)) { body = Character.Animator.GetBoneTransform(HumanBodyBones.Spine).gameObject; } }
            if (bodyBone == BodyBone.Chest) { if (Character.Animator.GetBoneTransform(HumanBodyBones.Chest)) { body = Character.Animator.GetBoneTransform(HumanBodyBones.Chest).gameObject; } }
            if (bodyBone == BodyBone.UpperChest) { if (Character.Animator.GetBoneTransform(HumanBodyBones.UpperChest)) { body = Character.Animator.GetBoneTransform(HumanBodyBones.UpperChest).gameObject; } }
            if (bodyBone == BodyBone.Neck) { if (Character.Animator.GetBoneTransform(HumanBodyBones.Neck)) { body = Character.Animator.GetBoneTransform(HumanBodyBones.Neck).gameObject; } }

            // Notification
            isSearch = true;
            Notification();
        }

        // Show All Bones
        [Trigger] public void ShowAllBones() { Context.Service.PromptMessage("All Bones:", allBones); }

        // Bones Weights
        [DataInput, FloatSlider(0, 1)] public float EyesWeight = 0.2f;
        [DataInput, FloatSlider(0, 1)] public float HeadWeight = 0.5f;
        [DataInput, FloatSlider(0, 1)] public float BodyWeight = 0.1f;

        // Target Parameters
        [DataInput, FloatSlider(0, 2)] public Vector2 TargetRangeFactor = new Vector2(0.5f, 0.5f);
        [DataInput] public Vector2 RandomFactor = new Vector2(0.1f, 0.1f);
        [DataInput, FloatSlider(0.1f, 3)] public float TargetSpeed = 1f;
        [DataInput, FloatSlider(0, 1)] public float TargetEyesAdjustement = 0;
        [DataInput, FloatSlider(0, 1)] public float Deadzone = 0.1f;
        [DataInput] public Vector3 TargetPosition;
        [DataInput] public bool TargetSwitch = false;

        // Toggle Gizmo
        [Trigger] public void ToggleGizmo() { target.SetActive(!target.activeSelf); targetEyeL.SetActive(!targetEyeL.activeSelf); targetEyeR.SetActive(!targetEyeR.activeSelf); }

        // Parameters (Local)
        private MinoisData data;
        private GameObject target;
        private GameObject targetEyeL;
        private GameObject targetEyeR;
        private Vector3 joy = Vector3.zero;
        private Vector3 rand = Vector3.zero;
        private float randDelay = 0f;
        private float randDelayMax = UnityEngine.Random.Range(0.5f, 5f);
        private string allBones = "";
        private bool switchPress = false;
        private bool overState = false;

        // Bones References
        private GameObject eyel;
        private GameObject eyer;
        private GameObject head;
        private GameObject body;

        // Functions
        protected override void OnCreate() {
            // Shader
            Shader shader = Shader.Find("Sprites/Default");

            // Main Target
            target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            target.name = "AndyHellgrim_Look_Target";
            UnityEngine.Object.Destroy(target.GetComponent<SphereCollider>());
            target.GetComponent<MeshRenderer>().material.shader = shader;
            target.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(0.8f, 0, 0, 0.5f));
            target.SetActive(false);

            // Target Eye Left
            targetEyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            targetEyeL.transform.SetParent(target.transform);
            targetEyeL.name = "AndyHellgrim_Look_TargetEyeLeft";
            UnityEngine.Object.Destroy(targetEyeL.GetComponent<SphereCollider>());
            targetEyeL.GetComponent<MeshRenderer>().material.shader = shader;
            targetEyeL.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 0.5f));
            targetEyeL.SetActive(false);

            // Target Eye Right
            targetEyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            targetEyeR.transform.SetParent(target.transform);
            targetEyeR.name = "AndyHellgrim_Look_TargetEyeRight";
            UnityEngine.Object.Destroy(targetEyeR.GetComponent<SphereCollider>());
            targetEyeR.GetComponent<MeshRenderer>().material.shader = shader;
            targetEyeR.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 0.5f));
            targetEyeR.SetActive(false);

            // Scales
            target.transform.localScale = Vector3.one * 0.02f;
            targetEyeL.transform.localScale = Vector3.one * 0.5f;
            targetEyeR.transform.localScale = Vector3.one * 0.5f;
        }

        protected override void OnDestroy() {
            UnityEngine.Object.Destroy(target);
            UnityEngine.Object.Destroy(targetEyeL);
            UnityEngine.Object.Destroy(targetEyeR);
        }

        bool isSearch = false;
        int charID = 0;
        public override void OnLateUpdate() {
            base.OnLateUpdate();
            InvokeFlow(null);

            // Get Data
            if (data == null) {
                data = GameObject.Find("Minois_Emotes").gameObject.GetComponent<MinoisData>();
            }

            // Check Character Humanoid/Generic
            if (!isSearch) {
                if (Character.Animator.isHuman) {
                    isHumanoid = true;
                    FindHumanoidBones();
                } else {
                    isHumanoid = false;
                    FindGenericBones();
                }
            }

            // Check if Character have been changed
            if (charID != Character.Source.GetHashCode()) {
                charID = Character.Source.GetHashCode();
                isSearch = false;
            }

            // Target
            TargetsMove();

            // Look
            CharacterLook();
        }

        void RecursiveSearch(Transform target, int depth = 0) {
            foreach (Transform child in target) {
                depth++;
                string spaces = "";
                for (int i = 0; i < depth; i++)
                    spaces += "  ";
                if (!child.GetComponent<SkinnedMeshRenderer>()) {
                    allBones += spaces + "• " + child.name + "\n";
                }

                if (!isHumanoid) {
                    if (child.name == BoneFilters[0]) { eyel = child.gameObject; }
                    if (child.name == BoneFilters[1]) { eyer = child.gameObject; }
                    if (child.name == BoneFilters[2]) { head = child.gameObject; }
                    if (child.name == BoneFilters[3]) { body = child.gameObject; }
                }

                if (target.childCount > 0) {
                    RecursiveSearch(child, depth);
                }
            }
        }

        void Notification() {
            string rslt = "";
            if (eyel != null) { rslt += "✅EyeL "; } else { rslt += "❌EyeL "; }
            if (eyer != null) { rslt += "✅EyeR "; } else { rslt += "❌EyeR "; }
            if (head != null) { rslt += "✅Head "; } else { rslt += "❌Head "; }
            if (body != null) { rslt += "✅Body"; } else { rslt += "❌Body"; }
            Context.Service.Toast(ToastSeverity.Success, "Find Bones :", rslt, "", TimeSpan.FromSeconds(10));
        }

        void TargetsMove() {
            Transform ct = Character.GameObject.transform;
            randDelay += Time.deltaTime;

            // Target Eyes
            targetEyeL.transform.localPosition = new Vector3(TargetEyesAdjustement * -50, 0, 0);
            targetEyeR.transform.localPosition = new Vector3(TargetEyesAdjustement * 50, 0, 0);

            // Random
            if (randDelay > randDelayMax) {
                randDelay = 0;
                randDelayMax = UnityEngine.Random.Range(0.5f, 5f);
                rand = new Vector3(UnityEngine.Random.Range(-RandomFactor.x, RandomFactor.x), UnityEngine.Random.Range(-RandomFactor.y, RandomFactor.y), 0);
            }

            // Deadzone
            if (Vector3.Distance(Vector3.zero, TargetPosition) <= Deadzone) {
                joy = Vector3.zero;
            } else {
                joy = new Vector3(TargetPosition.x * TargetRangeFactor.x, TargetPosition.y * TargetRangeFactor.y, 0) * -1;
            }

            // Result
            if (overState) {
                target.transform.position = Vector3.Lerp(
                    target.transform.position,
                    TargetOverride.GameObject.transform.position,
                    Time.deltaTime * 10f * TargetSpeed);
            } else {
                target.transform.position = Vector3.Lerp(
                    target.transform.position,
                    ct.forward + head.transform.position + joy + rand,
                    Time.deltaTime * 10f * TargetSpeed);
            }

            // Rotate
            target.transform.rotation = Character.GameObject.transform.rotation;
        }

        void CharacterLook() {
            if (data.LookEnable) {
                // Angle Factor
                float angleChar = Vector3.Angle(Character.GameObject.transform.forward, Vector3.forward);
                float angleOver = Vector3.Angle(TargetOverride.GameObject.transform.forward, Vector3.forward);
                float angleFactor = Mathf.Abs((angleOver - angleChar) / 180 - 1);

                // Switch TargetOverride state
                if (TargetSwitch && !switchPress) {
                    switchPress = true;
                    overState = !overState;
                } else if (!TargetSwitch && switchPress) {
                    switchPress = false;
                }
                int overrideBool = Convert.ToInt32(overState);


                // Look
                if (body) {
                    body.transform.LookAt(Vector3.Lerp(body.transform.position + body.transform.forward, target.transform.position, BodyWeight - angleFactor * overrideBool));
                }

                if (head) {
                    head.transform.LookAt(Vector3.Lerp(head.transform.position + head.transform.forward, target.transform.position, HeadWeight - angleFactor * overrideBool));
                }

                if (eyel) {
                    if (isHumanoid) {
                        eyel.transform.LookAt(Vector3.Lerp(eyel.transform.position + eyel.transform.forward, targetEyeL.transform.position, EyesWeight - angleFactor * overrideBool));
                    } else {
                        eyel.transform.LookAt(Vector3.Lerp(eyel.transform.position + eyel.transform.up, targetEyeL.transform.position, EyesWeight - angleFactor * overrideBool));
                        eyel.transform.Rotate(EyesOffset.x, EyesOffset.y, EyesOffset.z, Space.Self);
                    }
                }

                if (eyer) {
                    if (isHumanoid) {
                        eyer.transform.LookAt(Vector3.Lerp(eyer.transform.position + eyer.transform.forward, targetEyeR.transform.position, EyesWeight - angleFactor * overrideBool));
                    } else {
                        eyer.transform.LookAt(Vector3.Lerp(eyer.transform.position + eyer.transform.up, targetEyeR.transform.position, EyesWeight - angleFactor * overrideBool));
                        eyer.transform.Rotate(EyesOffset.x, EyesOffset.y, EyesOffset.z, Space.Self);
                    }
                }
            }
        }
    }
}