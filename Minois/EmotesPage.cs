using System.Collections.Generic;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Graphs;

namespace AndyHellgrim.Minois {
    [NodeType(Id = "AndyHellgrim.Minois.EmotesPage", Title = "Minois - Emotes Page", Category = "Minois", Width = 1.5f)]
    public class MinoisEmotesPage : Node {
        [DataInput, Label("+")] public object Link;
        [DataInput, HideLabel] public string Title = "Title";

        [DataInput, Label("Button 1 (Up)")] public Emote Button1;
        [DataInput, Label("Button 2 (Right)")] public Emote Button2;
        [DataInput, Label("Button 3 (Down)")] public Emote Button3;
        [DataInput, Label("Button 4 (Left)")] public Emote Button4;

        [DataOutput]
        public string[] Output() {
            List<string> o = new List<string>();

            if (Link != null) {
                foreach (var item in Link as string[]) {
                    o.Add(item);
                }
            }

            o.Add(Title);

            o.Add(Button1.Animation);
            o.Add(Button1.Sound);
            o.Add(Button1.LookEnable.ToString());

            o.Add(Button2.Animation);
            o.Add(Button2.Sound);
            o.Add(Button2.LookEnable.ToString());

            o.Add(Button3.Animation);
            o.Add(Button3.Sound);
            o.Add(Button3.LookEnable.ToString());

            o.Add(Button4.Animation);
            o.Add(Button4.Sound);
            o.Add(Button4.LookEnable.ToString());

            return o.ToArray();
        }
    }

    public class Emote : StructuredData {
        [DataInput, PreviewGallery, AutoCompleteResource("CharacterAnimation"), HideLabel] public string Animation = null;
        [DataInput, PreviewGallery, AutoCompleteResource("Sound"), HideLabel] public string Sound = null;
        [DataInput] public bool LookEnable = true;
    }
}

