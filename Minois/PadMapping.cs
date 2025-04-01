using System.Collections.Generic;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Graphs;

namespace AndyHellgrim.Minois {
    [NodeType(Id = "AndyHellgrim.Minois.InputFix", Title = "Minois - Pad Mapping", Category = "Minois", Width = 1f)]
    public class MinoisPadMapping : Node {

        public enum PadTypes {PS4Controller, Xbox360Controller, Custom}
        [DataInput, Label("Pad Type")] public PadTypes pad;

        List<Map> maps = new List<Map>();

        protected override void OnCreate() {
            maps.Add(AddMap(3, 2, 1, 0, 4, 5, 6, 7, 0, 1, 2, 5, 10, 11, 9, 8)); // PS4 Controller
            maps.Add(AddMap(3, 1, 0, 2, 4, 5, 32, 32, 0, 1, 3, 4, 8, 9, 7, 6)); // Xbox 360 Controller
        }

        [DataOutput] public int ButtonTop() { if (pad.ToString() == "Custom") { return CustomMap.ButtonTop; } return maps[pad.GetHashCode()].ButtonTop; }
        [DataOutput] public int ButtonRight() { if (pad.ToString() == "Custom") {return CustomMap.ButtonRight; } return maps[pad.GetHashCode()].ButtonRight; }
        [DataOutput] public int ButtonBottom() { if (pad.ToString() == "Custom") {return CustomMap.ButtonBottom; } return maps[pad.GetHashCode()].ButtonBottom; }
        [DataOutput] public int ButtonLeft() { if (pad.ToString() == "Custom") {return CustomMap.ButtonLeft; } return maps[pad.GetHashCode()].ButtonLeft; }

        [DataOutput] public int ShoulderLeft() { if (pad.ToString() == "Custom") {return CustomMap.ShoulderLeft; } return maps[pad.GetHashCode()].ShoulderLeft; }
        [DataOutput] public int ShoulderRight() { if (pad.ToString() == "Custom") {return CustomMap.ShoulderRight; } return maps[pad.GetHashCode()].ShoulderRight; }
        [DataOutput] public int TriggerLeft() { if (pad.ToString() == "Custom") {return CustomMap.TriggerLeft; } return maps[pad.GetHashCode()].TriggerLeft; }
        [DataOutput] public int TriggerRight() { if (pad.ToString() == "Custom") {return CustomMap.TriggerRight; } return maps[pad.GetHashCode()].TriggerRight; }

        [DataOutput] public int JoystickLeftX() { if (pad.ToString() == "Custom") {return CustomMap.JoyLeftX; } return maps[pad.GetHashCode()].JoyLeftX; }
        [DataOutput] public int JoystickLeftY() { if (pad.ToString() == "Custom") {return CustomMap.JoyLeftY; } return maps[pad.GetHashCode()].JoyLeftY; }
        [DataOutput] public int JoystickRightX() { if (pad.ToString() == "Custom") {return CustomMap.JoyRightX; } return maps[pad.GetHashCode()].JoyRightX; }
        [DataOutput] public int JoystickRightY() { if (pad.ToString() == "Custom") {return CustomMap.JoyRightY; } return maps[pad.GetHashCode()].JoyRightY; }

        [DataOutput] public int JoystickLeftPress() { if (pad.ToString() == "Custom") {return CustomMap.JoyLeftPress; } return maps[pad.GetHashCode()].JoyLeftPress; }
        [DataOutput] public int JoystickRightPress() { if (pad.ToString() == "Custom") {return CustomMap.JoyRightPress; } return maps[pad.GetHashCode()].JoyRightPress; }

        [DataOutput] public int Start() { if (pad.ToString() == "Custom") {return CustomMap.Start; } return maps[pad.GetHashCode()].Start; }
        [DataOutput] public int Select() { if (pad.ToString() == "Custom") {return CustomMap.Select; } return maps[pad.GetHashCode()].Select; }

        [DataInput] public Map CustomMap;

        Map AddMap(int buttonTop, int buttonRight, int buttonBottom, int buttonLeft, int shoulderLeft, int shoulderRight, int triggerLeft, int triggerRight, int joyLeftX, int joyLeftY, int joyRightX, int joyRightY, int joyLeftPress, int joyRightPress, int start, int select) {
            Map map = new Map();

            map.ButtonTop = buttonTop;
            map.ButtonRight = buttonRight;
            map.ButtonBottom = buttonBottom;
            map.ButtonLeft = buttonLeft;
            map.ShoulderLeft = shoulderLeft;
            map.ShoulderRight = shoulderRight;
            map.TriggerLeft = triggerLeft;
            map.TriggerRight = triggerRight;
            map.JoyLeftX = joyLeftX;
            map.JoyLeftY = joyLeftY;
            map.JoyRightX = joyRightX;
            map.JoyRightY = joyRightY;
            map.JoyLeftPress = joyLeftPress;
            map.JoyRightPress = joyRightPress;
            map.Start = start;
            map.Select = select;

            return map;
        }

        [Trigger] public void Submit() {
            string title = "Submit Custom Mapping";
            Map cm = CustomMap;
            string message = "Submit your mapping if you think your controller should be in the default list !\n\n"
                            + "• Open a ticket in https://github.com/AndyLobjois/Minois/issues\n\n"
                            + $"• Write your Controller Name and copy/paste your custom mapping :\n({cm.ButtonTop}, {cm.ButtonRight}, {cm.ButtonBottom}, {cm.ButtonLeft}, {cm.ShoulderLeft}, {cm.ShoulderRight}, {cm.TriggerLeft}, {cm.TriggerRight}, {cm.JoyLeftX}, {cm.JoyLeftY}, {cm.JoyRightX}, {cm.JoyRightY}, {cm.JoyLeftPress}, {cm.JoyRightPress}, {cm.Start}, {cm.Select})";

            Context.Service.PromptMessage(title, message);
        }
    }

    public class Map : StructuredData, ICollapsibleStructuredData {
        [DataInput] public int ButtonTop;
        [DataInput] public int ButtonRight;
        [DataInput] public int ButtonBottom;
        [DataInput] public int ButtonLeft;

        [DataInput] public int ShoulderLeft;
        [DataInput] public int ShoulderRight;
        [DataInput] public int TriggerLeft;
        [DataInput] public int TriggerRight;

        [DataInput] public int JoyLeftX;
        [DataInput] public int JoyLeftY;
        [DataInput] public int JoyRightX;
        [DataInput] public int JoyRightY;

        [DataInput] public int JoyLeftPress;
        [DataInput] public int JoyRightPress;

        [DataInput] public int Start;
        [DataInput] public int Select;

        public string GetHeader() => "Custom Mapping";
    }
}