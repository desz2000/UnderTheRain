using System;
using System.Collections.Generic;

namespace UnderTheRain.Narrative
{
    /// <summary>
    /// Pure C# narrative state model tracking collected documents and story flags.
    /// </summary>
    public sealed class NarrativeProgressModel
    {
        private readonly HashSet<string> _collectedDocuments = new HashSet<string>();
        private readonly HashSet<string> _flags = new HashSet<string>();

        public int CollectedDocumentCount => _collectedDocuments.Count;

        public event Action<string> OnDocumentCollected;
        public event Action<string> OnFlagSet;

        public bool HasDocument(string documentId)
        {
            return !string.IsNullOrEmpty(documentId) && _collectedDocuments.Contains(documentId);
        }

        public bool TryCollectDocument(string documentId)
        {
            if (string.IsNullOrEmpty(documentId)) return false;
            if (!_collectedDocuments.Add(documentId)) return false;

            OnDocumentCollected?.Invoke(documentId);
            return true;
        }

        public bool HasFlag(string flag)
        {
            return !string.IsNullOrEmpty(flag) && _flags.Contains(flag);
        }

        public bool SetFlag(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return false;
            if (!_flags.Add(flag)) return false;

            OnFlagSet?.Invoke(flag);
            return true;
        }
    }
}
