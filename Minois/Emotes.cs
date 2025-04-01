using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;

namespace AndyHellgrim.Minois {
    [NodeType(Id = "AndyHellgrim.Minois.Emotes", Title = "Minois - Emotes", Category = "Minois", Width = 1.5f)]
    public class MinoisEmotes : Node {
        // Inputs
        [DataInput] public bool ShowButton;
        [DataInput] public bool NextButton;
        [DataInput] public bool PreviousButton;

        [DataInput, Label("Button 1 (Up)")] public bool Button1;
        [DataInput, Label("Button 2 (Right)")] public bool Button2;
        [DataInput, Label("Button 3 (Down)")] public bool Button3;
        [DataInput, Label("Button 4 (Left)")] public bool Button4;
        [DataInput] public string[] AllEmotes;
        public string[] allEmotes;
        [DataInput, Hidden] public int pageIndex;

        // Outputs
        [FlowOutput] public Continuation TriggerAnimation;
        [FlowOutput] public Continuation TriggerSound;
        string animationClip = "";
        [DataOutput] public string AnimationClip() {
            return animationClip;
        }
        string audioClip = "";
        [DataOutput] public string AudioClip() {
            return audioClip;
        }

        //[DataOutput] public string Log() {
        //    string log = "";
        //    log += "Index : " + pageIndex.ToString() + "</br>";
        //    log += "</br>";
        //    log += "Show Button : " + ShowButton.ToString() + "</br>";
        //    log += "isShow : " + isShow.ToString() + "</br>";
        //    log += "</br>";
        //    log += "Up Button : " + UpButton.ToString() + "</br>";
        //    log += "</br>";
        //    log += "Down Button : " + DownButton.ToString() + "</br>";
        //    log += "</br>";
        //    log += "Right Button : " + RightButton.ToString() + "</br>";
        //    log += "</br>";
        //    log += "Left Button : " + LeftButton.ToString() + "</br>";
        //    return log;
        //}

        // References UI
        MinoisData data;
        private Transform EMOTES;
        private Animation EMOTES_ANIM;
        private Text EMOTES_TITLE;
        private Text EMOTES_PAGES;
        private Text EMOTES_UP;
        private Text EMOTES_RIGHT;
        private Text EMOTES_DOWN;
        private Text EMOTES_LEFT;

        // Booleans
        private bool isShow = false;
        bool showPress = false;
        bool upPress = false;
        bool rightPress = false;
        bool downPress = false;
        bool leftPress = false;
        bool nextPress = false;
        bool prevPress = false;

        const int STEP = 13;


        void GetReferences() {
            // References
            if (EMOTES == null) { EMOTES = GameObject.Find("Minois_Emotes").transform; }
            if (EMOTES_ANIM == null) { EMOTES_ANIM = EMOTES.GetComponent<Animation>(); }
            if (EMOTES_TITLE == null) { EMOTES_TITLE = EMOTES.Find("Title/Text").GetComponent<Text>(); }
            if (EMOTES_PAGES == null) { EMOTES_PAGES = EMOTES.Find("Title/Page").GetComponent<Text>(); }
            if (EMOTES_UP == null) { EMOTES_UP = EMOTES.Find("ButtonUp/Text").GetComponent<Text>(); }
            if (EMOTES_RIGHT == null) { EMOTES_RIGHT = EMOTES.Find("ButtonRight/Text").GetComponent<Text>(); }
            if (EMOTES_DOWN == null) { EMOTES_DOWN = EMOTES.Find("ButtonDown/Text").GetComponent<Text>(); }
            if (EMOTES_LEFT == null) { EMOTES_LEFT = EMOTES.Find("ButtonLeft/Text").GetComponent<Text>(); }

            // Data
            if (EMOTES != null) {
                if (EMOTES.gameObject.GetComponent<MinoisData>()) {
                    data = EMOTES.gameObject.GetComponent<MinoisData>();
                } else {
                    EMOTES.gameObject.AddComponent<MinoisData>();
                }
            }
        }

        public override void OnLateUpdate() {
            base.OnLateUpdate();
            InvokeFlow(null);

            /// GET REFERENCES
            GetReferences();

            /// MAIN LOGIC
            Buttons();

            /// DETECTION
            Detection();
        }

        void Detection() {
            // Emotes List have been updated !
            if (AllEmotes != allEmotes) {
                allEmotes = AllEmotes;
                SetUI_Emotes();
            }
        }


        // TOOLS -----------------------------------------------------------------------------------------------------------------------------------
        void Buttons() {
            // Emotes
            ButtonPressed(Button1, ref upPress, 0);
            ButtonPressed(Button2, ref rightPress, 3);
            ButtonPressed(Button3, ref downPress, 6);
            ButtonPressed(Button4, ref leftPress, 9);

            // Ui
            OpenCloseUI();
            NextPrevPage();
        }

        void ButtonPressed(bool button, ref bool press, int offset) {
            if (button && !press) {
                press = true;

                // Enable/Disable Look
                data.LookEnable = Convert.ToBoolean(AllEmotes[pageIndex * STEP + offset + 3]);

                // Check Animation
                if (AllEmotes[pageIndex * STEP + offset + 1] != null) {
                    animationClip = AllEmotes[pageIndex * STEP + offset + 1];
                    InvokeFlow("TriggerAnimation");
                } else {
                    return;
                }

                // Check Audio
                if (AllEmotes[pageIndex * STEP + offset + 2] != null) {
                    audioClip = AllEmotes[pageIndex * STEP + offset + 2];
                    InvokeFlow("TriggerSound");
                }
            } else if (!button && press) {
                press = false;
            }
        }

        void OpenCloseUI() {
            if (ShowButton && !showPress) {
                showPress = true;

                // Open / Close
                if (!isShow) {
                    isShow = true;
                    EMOTES_ANIM.Play("Open");
                } else {
                    isShow = false;
                    EMOTES_ANIM.Play("Close");
                }
            } else if (!ShowButton && showPress) {
                showPress = false;
            }
        }

        void NextPrevPage() {
            // Next
            if (NextButton && !nextPress) {
                nextPress = true;

                EMOTES_ANIM.Blend("Next", 1, 0.02f);
                SetIndex(1, AllEmotes.Length, STEP);
                SetUI_Emotes();
            } else if (!NextButton && nextPress) {
                nextPress = false;
            }

            // Previous
            if (PreviousButton && !prevPress) {
                prevPress = true;

                EMOTES_ANIM.Blend("Previous", 1, 0.02f);
                SetIndex(-1, AllEmotes.Length, STEP);
                SetUI_Emotes();
            } else if (!PreviousButton && prevPress) {
                prevPress = false;
            }
        }

        void SetIndex(int add, int length, int step) {
            // Add/Remove 1 to Index
            pageIndex += add;

            // Loop: From End to Start
            if (pageIndex > length / step - 1) {
                pageIndex = 0;
            }

            // Loop: From Start to End
            if (pageIndex < 0) {
                pageIndex = length / step - 1;
            }
        }

        string GetLastStringAndParse(string text) {
            string[] split = text.Split('/');
            return split[split.Length - 1].Replace(".warudo", "").Replace("_", " ").Replace(".anim", "").ToUpper();
        }

        void SetUI_Emotes() {
            // Get Title, Page & List
            string title = AllEmotes[pageIndex * STEP];
            string page = $"{pageIndex + 1} / {AllEmotes.Length / STEP}";

            // Get Animations Name
            List<string> emotes = new List<string>();
            for (int i = pageIndex * STEP + 1; i < pageIndex * STEP + STEP; i += 3) {
                if (AllEmotes[i] == null) {
                    emotes.Add("...");
                } else {
                    emotes.Add(AllEmotes[i]);
                }
            }

            // Set Text
            EMOTES_TITLE.text = title.ToUpper();
            EMOTES_PAGES.text = page;

            EMOTES_UP.text = GetLastStringAndParse(emotes[0]);
            EMOTES_RIGHT.text = GetLastStringAndParse(emotes[1]);
            EMOTES_DOWN.text = GetLastStringAndParse(emotes[2]);
            EMOTES_LEFT.text = GetLastStringAndParse(emotes[3]);
        }
    }
}