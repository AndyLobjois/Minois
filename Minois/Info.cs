using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;

namespace AndyHellgrim.Minois {
    [NodeType(Id = "AndyHellgrim.Minois.Info", Title = "Minois - Informations", Category = "Minois", Width = 1.5f)]
    public class MinoisInfo : Node {
        [Trigger] public void OpenRepository() {
            Application.OpenURL("https://github.com/AndyLobjois/Minois");
        }

        [Trigger] public void OpenTutorials() {
            Application.OpenURL("https://www.notion.so/andyhellgrim/Minois-Warudo-Plugin-1c0b736a3e05801481b5f28fc0a03d72?pvs=4");
        }
    }
}