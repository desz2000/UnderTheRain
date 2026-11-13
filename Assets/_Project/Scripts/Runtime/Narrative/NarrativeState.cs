using System;
using UnityEngine;

namespace UnderTheRain.Narrative
{
    /// <summary>
    /// Scene-level narrative state wrapper around <see cref="NarrativeProgressModel"/>.
    /// </summary>
    public sealed class NarrativeState : MonoBehaviour
    {
        public const string FlagDomusEvidenceFound = "domus_evidence_found";
        public const string FlagIcarusConfronted = "icarus_confronted";
        public const string FlagSewerExitUnlocked = "sewer_exit_unlocked";

        private static NarrativeProgressModel _sharedModel;

        private NarrativeProgressModel _model;

        public int CollectedDocumentCount => EnsureModel().CollectedDocumentCount;

        public event Action<DocumentDataSO> OnDocumentCollected;
        public event Action<string> OnFlagSet;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            _sharedModel = null;
        }

        private NarrativeProgressModel EnsureModel()
        {
            if (_model == null)
            {
                _sharedModel ??= new NarrativeProgressModel();
                _model = _sharedModel;
            }
            return _model;
        }

        private void Awake()
        {
            EnsureModel().OnFlagSet += HandleModelFlagSet;
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.OnFlagSet -= HandleModelFlagSet;
            }
        }

        public bool HasDocument(DocumentDataSO document)
        {
            return document != null && EnsureModel().HasDocument(document.documentId);
        }

        public bool CollectDocument(DocumentDataSO document)
        {
            if (document == null) return false;
            if (!EnsureModel().TryCollectDocument(document.documentId)) return false;

            OnDocumentCollected?.Invoke(document);
            return true;
        }

        public bool HasFlag(string flag) => EnsureModel().HasFlag(flag);

        public bool SetFlag(string flag) => EnsureModel().SetFlag(flag);

        private void HandleModelFlagSet(string flag)
        {
            OnFlagSet?.Invoke(flag);
        }
    }
}
