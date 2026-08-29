using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace EngineeringPlayground.Core.Progress
{
    public enum PresentationMode
    {
        Explorer,
        Engineer
    }

    [Serializable]
    public sealed class ChallengeProgress
    {
        public bool Completed;
        public int Stars;
        public double BestScore;
        public string BestGrade = string.Empty;
    }

    [Serializable]
    public sealed class PlayerProgressData
    {
        public Dictionary<string, ChallengeProgress> Challenges = new();
        public HashSet<string> UnlockedConcepts = new();
        public PresentationMode PresentationMode = PresentationMode.Explorer;
    }

    public sealed class PlayerProgressStore
    {
        private const string PlayerPrefsKey = "engineering_playground_progress_v1";
        private PlayerProgressData _data;

        public PlayerProgressStore()
        {
            _data = Load();
        }

        public PresentationMode PresentationMode => _data.PresentationMode;
        public IReadOnlyCollection<string> UnlockedConcepts => _data.UnlockedConcepts;

        public ChallengeProgress GetChallenge(string challengeId)
        {
            if (!_data.Challenges.TryGetValue(challengeId, out var progress))
            {
                progress = new ChallengeProgress();
                _data.Challenges[challengeId] = progress;
            }
            return progress;
        }

        public int TotalStars()
        {
            var total = 0;
            foreach (var entry in _data.Challenges.Values)
                total += entry.Stars;
            return total;
        }

        public void RecordChallengeResult(string challengeId, double score, string grade, int stars, IEnumerable<string> conceptUnlocks)
        {
            var progress = GetChallenge(challengeId);
            progress.Completed = true;
            progress.Stars = Math.Max(progress.Stars, Math.Clamp(stars, 0, 3));
            if (score >= progress.BestScore)
            {
                progress.BestScore = score;
                progress.BestGrade = grade ?? string.Empty;
            }

            if (conceptUnlocks != null)
            {
                foreach (var concept in conceptUnlocks)
                {
                    if (!string.IsNullOrWhiteSpace(concept))
                        _data.UnlockedConcepts.Add(concept);
                }
            }
            Save();
        }

        public void SetPresentationMode(PresentationMode mode)
        {
            _data.PresentationMode = mode;
            Save();
        }

        public void ResetAll()
        {
            _data = new PlayerProgressData();
            Save();
        }

        private static PlayerProgressData Load()
        {
            var json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
                return new PlayerProgressData();

            try
            {
                return JsonConvert.DeserializeObject<PlayerProgressData>(json) ?? new PlayerProgressData();
            }
            catch
            {
                return new PlayerProgressData();
            }
        }

        private void Save()
        {
            PlayerPrefs.SetString(PlayerPrefsKey, JsonConvert.SerializeObject(_data));
            PlayerPrefs.Save();
        }
    }
}
