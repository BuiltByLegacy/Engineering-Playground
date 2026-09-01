using System.IO;
using UnityEngine;

namespace EngineeringPlayground.Core.Content
{
    public static class ContentRepository
    {
        public static string Resolve(string relativePath)
        {
            relativePath = ShowcaseCatalog.NormalizeContentPath(relativePath);
            if (Application.isEditor)
                return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "content", relativePath));

            return Path.Combine(Application.streamingAssetsPath, "content", relativePath);
        }

        public static string ReadText(string relativePath)
        {
            relativePath = ShowcaseCatalog.NormalizeContentPath(relativePath);

            if (!Application.isEditor)
            {
                var resourcePath = $"EngineeringContent/{Path.ChangeExtension(relativePath, null)}"
                    .Replace('\\', '/');
                var asset = Resources.Load<TextAsset>(resourcePath);
                if (asset == null)
                    throw new FileNotFoundException($"Engineering Playground content not found: {relativePath}");
                return asset.text;
            }

            var path = Resolve(relativePath);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Engineering Playground content not found: {relativePath}", path);
            return File.ReadAllText(path);
        }

        public static CampaignDefinition LoadFlowCampaign() =>
            CampaignCatalog.Parse(ReadText("flow/campaign.json"));

        public static ShowcaseManifest LoadFlowShowcases() =>
            ShowcaseCatalog.Parse(ReadText("flow/showcases.json"));

        public static ChallengeDefinition LoadChallenge(string relativePath) =>
            CampaignCatalog.ParseChallenge(ReadText(relativePath));
    }
}
