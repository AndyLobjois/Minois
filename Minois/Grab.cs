using System.Collections.Generic;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Graphs;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Character;

namespace AndyHellgrim.Minois {
    [NodeType(Id = "AndyHellgrim.Minois.Grab", Title = "Minois - Grab", Category = "Minois", Width = 2f)]
    public class MinoisGrab : Node {
        // Character
        int charID = 0;
        [DataInput, HideLabel] public CharacterAsset Character;
        [DataInput] public float CharacterColliderRadius = 0.5f;
        [DataInput] public Vector3 CharacterColliderOffset = Vector3.zero;
        [DataInput] public bool TriggerInput;
        [DataInput] public Item[] Items;

        Shader shader = Shader.Find("Sprites/Default");
        Color colorCharacter = new Color(0.9f, 0, 0, 0.5f);
        Color colorProps = new Color(1, 1, 1, 0.33f);
        List<GameObject> Triggers = new List<GameObject>();
        List<GameObject> Targets = new List<GameObject>();

        int pickID = 0;
        public Trigger Pick;
        bool displayGizmos = false;

        // [DataOutput] public string Log() {
        //     string log = "";
        //     log += $"Character : {Character.Name}";
        //     log += "<br>";
        //     log += $"Triggers[{Triggers.Count}]";
        //     log += "<br>";
        //     log += $"Targets[{Targets.Count}] (";
        //     foreach (GameObject item in Targets) {
        //         log += item.name + ", ";
        //     }
        //     log += ")";
        //     log += "<br>";
        //     if (Pick) {
        //         log += $"Pick : {Pick.name}<br>";
        //     }
        //     log += $"Pick ID : {pickID}";
        //     return log;
        // }
        
        [Trigger, Label("Update")] public void UpdateProps() {
            // Delete All
            for (int i = 0; i < Triggers.Count; i++) { UnityEngine.Object.Destroy(Triggers[i]); }
            Triggers.Clear();
            Targets.Clear();
            UnloadPick();

            // Create Character Colliders
            CreateTrigger(Character.GameObject, null, CharacterColliderRadius, true, colorCharacter);

            // Create Props Collider
            foreach (var item in Items) {
                CreateTrigger(item.ItemGameObjectAsset.GameObject, item, item.ColliderRadius, false, colorProps);
            }

            // Find Bone per Prop
            for (int i = 0; i < Items.Length; i++) {
                if (Items[i].isHuman) {
                    Items[i].bone = Character.Animator.GetBoneTransform(Items[i].AttachToBoneHumanoid);
                } else {
                    if (Items[i].AttachToBoneGeneric != "") {
                        FindBone(Character.GameObject.transform, Items[i].AttachToBoneGeneric, i);
                    }
                }
            }
        }

        [Trigger] public void ToggleGizmos() {
            displayGizmos = !displayGizmos;

            foreach (var item in Triggers) {
                item.GetComponent<MeshRenderer>().enabled = displayGizmos;
            }
        }

        protected override void OnDestroy() {
            // Remove Triggers
            for (int i = 0; i < Triggers.Count; i++) {
                UnityEngine.Object.Destroy(Triggers[i]);
            }
        }

        public override void OnLateUpdate() {
            base.OnLateUpdate();
            InvokeFlow(null);

            InputPress();
            CheckCharacterID();
            MoveTriggers();
            MovePick();

            // Character Collider Scale
            Triggers[0].transform.localScale = Vector3.one * CharacterColliderRadius;
        }


        bool inputPressed = false;
        void InputPress() {
            if (TriggerInput && !inputPressed) {
                inputPressed = true;

                if (!Pick) {
                    Triggers[0].GetComponent<SphereCollider>().enabled = true;
                    Triggers[0].GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(colorCharacter.r, colorCharacter.g, colorCharacter.b, colorCharacter.a + 0.15f));
                } else {
                    UnloadPick();
                }
            } else if (!TriggerInput && inputPressed) {
                inputPressed = false;
                Triggers[0].GetComponent<SphereCollider>().enabled = false;
                Triggers[0].GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(colorCharacter.r, colorCharacter.g, colorCharacter.b, colorCharacter.a));
            }
        }


        /// TOOLS ------------------------------------------------------------------------------------------------------
        ///
        
        void CheckCharacterID() {
            if (charID != Character.GameObject.GetInstanceID()) {
                charID = Character.GameObject.GetInstanceID();
                UpdateProps();

                // Set Humanoid (or not) to All Props
                foreach (var item in Items) {
                        item.isHuman = Character.Animator.isHuman;
                }
            }
        }

        void MoveTriggers() {
            for (int i = 0; i < Triggers.Count; i++) {
                Triggers[i].transform.position = Targets[i].transform.position;
            }
        }

        void MovePick() {
            if (Pick) {
                // Parent Pick
                if (pickID != Pick.gameObject.GetInstanceID()) {
                    pickID = Pick.gameObject.GetInstanceID();

                    // Parent Props to Pick.bone
                    Pick.Item.ItemGameObjectAsset.GameObject.transform.SetParent(Pick.Item.bone);
                }

                // Offset Pick
                Pick.Item.ItemGameObjectAsset.Transform.Position = Pick.Item.OffsetPickPOS;
                Pick.Item.ItemGameObjectAsset.Transform.Rotation = Pick.Item.OffsetPickROT;
            }
        }

        void CreateTrigger(GameObject target, Item item, float radius, bool addRigidBody, Color col) {
            // Create Sphere with Shader
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"Trigger_{Triggers.Count}";
            go.GetComponent<MeshRenderer>().material.shader = shader;
            go.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(col.r, col.g, col.b, col.a));
            go.GetComponent<MeshRenderer>().enabled = displayGizmos;
            go.transform.localScale = Vector3.one * radius;

            // Minois Trigger
            go.AddComponent<Trigger>().Main = this;
            go.GetComponent<Trigger>().Item = item;
            go.GetComponent<Trigger>().Parent = go.transform.parent;

            // Add/Set Components
            if (addRigidBody) {
                go.AddComponent<Rigidbody>().isKinematic = true;

                go.GetComponent<Trigger>().isChar = true;
                go.GetComponent<SphereCollider>().isTrigger = true;
                go.GetComponent<SphereCollider>().enabled = false;
            }

            // Add to Lists
            Triggers.Add(go);
            Targets.Add(target);
        }
        
        void UnloadPick() {
            if (Pick) {
                Pick.Item.ItemGameObjectAsset.GameObject.transform.SetParent(Pick.Parent);
                Pick.Item.ItemGameObjectAsset.Transform.Position = Character.Transform.Position + Pick.Item.OffsetDropPOS;
                Pick.Item.ItemGameObjectAsset.Transform.Rotation = Character.Transform.Rotation + Pick.Item.OffsetDropROT;
            }
            
            Pick = null;
            pickID = 0;
        }

        void FindBone(Transform target, string search, int index) {
            foreach (Transform child in target) {
                if (child.name == search) {
                    Items[index].bone = child.gameObject.transform;
                    break;
                }

                if (target.childCount > 0) {
                    FindBone(child, search, index);
                }
            }
        }
    }

    public class Trigger : MonoBehaviour {
        public MinoisGrab Main;
        public Item Item;
        public Transform Parent;
        public bool isChar = false;

        void OnTriggerEnter(Collider other) {
            if (isChar) {
                if (other.GetComponent<Trigger>()) {
                    Main.Pick = other.GetComponent<Trigger>();
                }
            }
        }
    }

    public class Item : StructuredData, ICollapsibleStructuredData {
        [DataInput, HideLabel] public GameObjectAsset ItemGameObjectAsset;
        [DataInput] public float ColliderRadius = 0.2f;

        // Bone Target
        [DataInput, Hidden] public bool isHuman = true;
        [DataInput, HiddenIf("isHuman", Is.True), Label("Attach To Bone")] public string AttachToBoneGeneric = "";
        [DataInput, HiddenIf("isHuman", Is.False), Label("Attach To Bone")] public HumanBodyBones AttachToBoneHumanoid;
        public Transform bone = null;

        // Offsets
        [DataInput, Label("Pick Offset"), Description("Position")] public Vector3 OffsetPickPOS = Vector3.zero;
        [DataInput, HideLabel, Description("Rotation")] public Vector3 OffsetPickROT = Vector3.zero;
        [DataInput, Label("Drop Offset"), Description("Position")] public Vector3 OffsetDropPOS = Vector3.zero;
        [DataInput, HideLabel, Description("Rotation")] public Vector3 OffsetDropROT = Vector3.zero;

        public string GetHeader() {
            if (ItemGameObjectAsset.IsNonNullAndActiveAndEnabled()) { return ItemGameObjectAsset.Name; }
            return "Empty";
        }
    }
}