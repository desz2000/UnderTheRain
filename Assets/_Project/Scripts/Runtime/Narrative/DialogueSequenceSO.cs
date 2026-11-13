using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnderTheRain.Narrative
{
    [Serializable]
    public struct DialogueLine
    {
        public string speakerName;
        public Color speakerColor;

        [TextArea(2, 5)]
        public string text;
    }

    /// <summary>
    /// Ordered sequence of dialogue or terminal lines displayed by <see cref="DialogueRunner"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueSequence", menuName = "UnderTheRain/Narrative/Dialogue Sequence")]
    public sealed class DialogueSequenceSO : ScriptableObject
    {
        public List<DialogueLine> lines = new List<DialogueLine>();
    }
}
