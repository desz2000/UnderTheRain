using UnityEngine;

namespace UnderTheRain.Narrative
{
    /// <summary>
    /// Immutable narrative document or log entry collected from terminals or world clues.
    /// </summary>
    [CreateAssetMenu(fileName = "DocumentData", menuName = "UnderTheRain/Narrative/Document Data")]
    public sealed class DocumentDataSO : ScriptableObject
    {
        public string documentId = "doc_domus_sabotage";
        public string title = "CONFIDENTIAL: DOMUS CORP // FREE-WILL CONTAINMENT";
        public string author = "Internal Audit — Sector 4";

        [TextArea(4, 12)]
        public string bodyText =
            "Incident Report #2090-B: The autonomous free-will kernel was sabotaged prior to citywide deployment. " +
            "Cover-up ordered by Executive Board. Lead architect's termination unconfirmed — emergency lab access remained active.";
    }
}
