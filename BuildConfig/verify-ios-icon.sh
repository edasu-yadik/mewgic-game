#!/bin/bash
set -euo pipefail
project_dir="${PROJECT_DIRECTORY:-$PWD}"
ipa_path="${UNITY_PLAYER_PATH:-}"
if [[ -z "$ipa_path" || ! -f "$ipa_path" || "$ipa_path" != *.ipa ]]; then
  echo "MEWGIC_IPA_ICON_FAIL: UNITY_PLAYER_PATH geçerli IPA değil."
  exit 1
fi
python3 "$project_dir/BuildConfig/verify_ios_icon.py" "$ipa_path" --report "${ipa_path%.ipa}-icon-verification.json"
