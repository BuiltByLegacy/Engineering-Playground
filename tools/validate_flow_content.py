#!/usr/bin/env python3
import json
import pathlib
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
CONTENT = ROOT / "content" / "flow"

EXPECTED_SHOWCASES = {
    "plumbing": "showcase_residential_plumbing",
    "exhaust": "showcase_automotive_exhaust",
    "hvac": "showcase_hvac_distribution",
    "manifold": "showcase_manifold_optimization",
}


def load_json(path: pathlib.Path):
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def fail(message: str):
    print(f"ERROR: {message}", file=sys.stderr)
    raise SystemExit(1)


def main():
    campaign_path = CONTENT / "campaign.json"
    showcases_path = CONTENT / "showcases.json"
    if not campaign_path.exists():
        fail("Missing content/flow/campaign.json")
    if not showcases_path.exists():
        fail("Missing content/flow/showcases.json")

    campaign = load_json(campaign_path)
    chapters = campaign.get("chapters", [])
    challenges = [challenge for chapter in chapters for challenge in chapter.get("challenges", [])]
    if len(challenges) != 30:
        fail(f"Expected 30 campaign challenges, found {len(challenges)}")

    campaign_ids = [item.get("challenge_id", "") for item in challenges]
    if len(set(campaign_ids)) != len(campaign_ids):
        fail("Campaign challenge IDs are not unique")
    if any(not challenge_id for challenge_id in campaign_ids):
        fail("Campaign contains an empty challenge_id")

    manifest = load_json(showcases_path)
    entries = manifest.get("showcases", [])
    if len(entries) != 4:
        fail(f"Expected four showcase entries, found {len(entries)}")

    entry_ids = [entry.get("id", "") for entry in entries]
    if set(entry_ids) != set(EXPECTED_SHOWCASES):
        fail(f"Unexpected showcase IDs: {entry_ids}")

    geometry_ids = []
    for entry in entries:
        showcase_id = entry["id"]
        relative_path = entry.get("path", "")
        if relative_path.startswith("res://") or ".." in relative_path:
            fail(f"Unsafe/non-Unity showcase path for {showcase_id}: {relative_path}")

        challenge_path = CONTENT / relative_path.removeprefix("flow/")
        if not challenge_path.exists():
            fail(f"Missing showcase challenge file for {showcase_id}: {challenge_path}")
        challenge = load_json(challenge_path)

        if challenge.get("playground_id") != "flow":
            fail(f"Showcase {showcase_id} is not a Flow challenge")

        geometry = challenge.get("starting_state", {}).get("geometry", "")
        expected_geometry = EXPECTED_SHOWCASES[showcase_id]
        if geometry != expected_geometry:
            fail(
                f"Showcase {showcase_id} geometry mismatch: expected {expected_geometry}, found {geometry}"
            )
        if geometry == "default_channel":
            fail(f"Showcase {showcase_id} still uses default_channel")
        geometry_ids.append(geometry)

    if len(set(geometry_ids)) != 4:
        fail("Showcase geometry IDs must be unique")

    project_version = (ROOT / "ProjectSettings" / "ProjectVersion.txt").read_text(encoding="utf-8")
    if "6000.3.18f1" not in project_version:
        fail("Unity editor pin changed from 6000.3.18f1; update CI intentionally")

    required_sources = [
        ROOT / "Assets/Scripts/Flow/Showcases/FlowShowcaseGeometryPresets.cs",
        ROOT / "Assets/Scripts/Flow/Showcases/ShowcasePackagingOverlay.cs",
        ROOT / "Assets/Tests/EditMode/FlowShowcaseTests.cs",
    ]
    for path in required_sources:
        if not path.exists():
            fail(f"Missing required showcase source: {path.relative_to(ROOT)}")

    print("Flow content smoke validation passed.")
    print("- 30 campaign challenges")
    print("- 4 unique showcase scenarios")
    print("- 4 unique scenario-specific geometry presets")
    print("- Unity 6000.3.18f1 pin confirmed")


if __name__ == "__main__":
    main()
