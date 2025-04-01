using Warudo.Core.Attributes;
using Warudo.Core.Graphs;

namespace AndyHellgrim.Minois {
    [NodeType(Id = "AndyHellgrim.Minois.EmoteMaker", Title = "Minois - Emote", Category = "Minois", Width = 1f)]
    public class MinoisEmote : Node {
        [DataInput, PreviewGallery, AutoCompleteResource("CharacterAnimation"), HideLabel] public string AnimationClip;
        [DataInput, PreviewGallery, AutoCompleteResource("Sound"), HideLabel] public string AudioClip;
        [DataInput] public bool LookEnable = true;

        [DataOutput]
        public Emote Output() {
            Emote _emote = new Emote();
            _emote.Animation = AnimationClip;
            _emote.Sound = AudioClip;
            _emote.LookEnable = LookEnable;

            return _emote;
        }
    }
}