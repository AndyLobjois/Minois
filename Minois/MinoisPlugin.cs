using Warudo.Core.Attributes;
using Warudo.Core.Plugins;

namespace AndyHellgrim.Minois {
    [PluginType(
        Id = "AndyHellgrim.Minois",
        Name = "Minois",
        Description = "Minois allows you to control your avatar with a gamepad like a video game ! Walk, look around, sit, use emotes/animations with sounds and grab objects. And more thanks to the Warudo node system !",
        Author = "Andy Hellgrim",
        Version = "1.0.0",
        NodeTypes = new[] {
            typeof(MinoisData),
            typeof(MinoisMove),
            typeof(MinoisLook),
            typeof(MinoisEmotes),
            typeof(MinoisEmotesPage),
            typeof(MinoisEmote),
            typeof(MinoisGrab),
            typeof(MinoisPadMapping),
            typeof(MinoisInfo),
        },
        Icon = null)]

    public class MinoisPlugin : Plugin {
        protected override void OnCreate() {
            base.OnCreate();
            // On plugin load (app startup)
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            // On plugin unload (app quit)
        }
    }
}

