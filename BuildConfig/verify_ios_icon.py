"""IPA kimliği, uygulama ikonları ve derlenmiş 1024px AppIcon doğrulaması."""
import argparse
import hashlib
import json
import os
import pathlib
import plistlib
import re
import subprocess
import tempfile
import zipfile


def verify(path, expected_build, assetutil="/usr/bin/assetutil"):
    path = pathlib.Path(path)
    with zipfile.ZipFile(path) as archive:
        assert archive.testzip() is None, "IPA ZIP CRC hatası"
        plists = [n for n in archive.namelist() if re.fullmatch(r"Payload/[^/]+\.app/Info.plist", n)]
        assert len(plists) == 1, "Ana uygulama Info.plist bulunamadı"
        prefix = plists[0].removesuffix("Info.plist")
        info = plistlib.loads(archive.read(plists[0]))
        assert info["CFBundleIdentifier"] == "com.edasuyadik.mewgic", "Bundle ID farklı"
        assert info["CFBundleShortVersionString"] == "0.8.0", "Sürüm farklı"
        assert str(info["CFBundleVersion"]) == expected_build, "Beklenen build numarası farklı"
        primary = info.get("CFBundleIcons", {}).get("CFBundlePrimaryIcon", {})
        assert primary.get("CFBundleIconName") == "AppIcon", "AppIcon Info.plist bağlantısı yok"
        files = primary.get("CFBundleIconFiles", [])
        assert files, "Uygulama ikon dosyaları yok"
        pngs = [n for n in archive.namelist() if n.startswith(prefix) and "/" not in n[len(prefix):] and n.endswith(".png")]
        for stem in files:
            assert any(pathlib.PurePosixPath(n).name.startswith(stem) for n in pngs), f"İkon PNG eksik: {stem}"
        car = archive.read(prefix + "Assets.car")
    with tempfile.TemporaryDirectory(prefix="mewgic-icon-") as temporary:
        car_path = pathlib.Path(temporary) / "Assets.car"
        car_path.write_bytes(car)
        process = subprocess.run([assetutil, "--info", str(car_path)], check=True, capture_output=True, text=True)
        renditions = json.loads(process.stdout)
        marketing = [r for r in renditions if r.get("PixelWidth") == 1024 and r.get("PixelHeight") == 1024
                     and "appicon" in (str(r.get("Name", "")) + str(r.get("RenditionName", ""))).lower()]
        assert marketing, "Assets.car içinde derlenmiş 1024x1024 AppIcon bulunamadı"
        assert any(r.get("Opaque") not in (False, "NO", "false", 0) for r in marketing), "1024 AppIcon opak değil"
    return {"success": True, "ipa": path.name, "bytes": path.stat().st_size,
            "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
            "bundleIdentifier": info["CFBundleIdentifier"], "version": info["CFBundleShortVersionString"],
            "build": str(info["CFBundleVersion"]), "iconFiles": files,
            "compiledAppIcon1024": marketing, "assetutilVerified": True}


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("ipa")
    parser.add_argument("--report", required=True)
    args = parser.parse_args()
    config = json.loads(pathlib.Path(__file__).with_name("iOS-TestFlight.json").read_text(encoding="utf-8"))
    expected = os.environ.get("MEWGIC_IOS_BUILD_NUMBER") or config.get("candidateBuildNumber") or os.environ.get("BUILD_NUMBER")
    if not expected or not expected.isdecimal() or int(expected) < 1:
        parser.error("Pozitif iOS build numarası gerekli")
    result = verify(args.ipa, str(int(expected)))
    pathlib.Path(args.report).write_text(json.dumps(result, indent=2, ensure_ascii=False), encoding="utf-8")
    print("MEWGIC_IPA_ICON_PASS " + json.dumps(result, ensure_ascii=False))
