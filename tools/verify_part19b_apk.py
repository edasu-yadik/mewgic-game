"""Inspect an ARM64 MeWGic QA APK without installing it on a device."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import struct
import subprocess
from zipfile import ZipFile


def elf_loads(data):
    if data[:6] != b"\x7fELF\x02\x01":
        raise ValueError("Expected a little-endian ELF64 library")
    if struct.unpack_from("<H", data, 18)[0] != 183:
        raise ValueError("Expected AArch64")
    offset = struct.unpack_from("<Q", data, 32)[0]
    size, count = struct.unpack_from("<HH", data, 54)
    result = []
    for index in range(count):
        kind, _, file_offset, address, _, _, _, align = struct.unpack_from(
            "<IIQQQQQQ", data, offset + size * index
        )
        if kind == 1:
            result.append(align >= 16384 and file_offset % 16384 == address % 16384)
    return bool(result) and all(result)


def command(tool, *args):
    return subprocess.run([str(tool), *map(str, args)], text=True, capture_output=True, check=False)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("apk", type=Path)
    parser.add_argument("build_tools", type=Path)
    parser.add_argument("--code", required=True, type=int)
    parser.add_argument("--report", type=Path)
    args = parser.parse_args()

    sdk = args.build_tools
    metadata = command(sdk / "aapt2.exe", "dump", "badging", args.apk)
    signing = command(sdk / "apksigner.bat", "verify", args.apk)
    alignment = command(sdk / "zipalign.exe", "-c", "-P", "16", "4", args.apk)
    attrs = dict(re.findall(r"\b(name|versionCode|versionName)='([^']*)'", (metadata.stdout.splitlines() or [""])[0]))
    minimum = re.search(r"^(?:minSdkVersion|sdkVersion):'(\d+)'", metadata.stdout, re.M)
    target = re.search(r"^targetSdkVersion:'(\d+)'", metadata.stdout, re.M)
    with ZipFile(args.apk) as archive:
        libraries = {name: elf_loads(archive.read(name)) for name in archive.namelist()
                     if name.startswith("lib/") and name.endswith(".so")}
    abi = sorted({name.split("/")[1] for name in libraries})
    report = {
        "file": args.apk.name,
        "bytes": args.apk.stat().st_size,
        "sha256": hashlib.sha256(args.apk.read_bytes()).hexdigest(),
        "package": attrs.get("name"),
        "versionName": attrs.get("versionName"),
        "versionCode": attrs.get("versionCode"),
        "minSdk": int(minimum[1]) if minimum else None,
        "targetSdk": int(target[1]) if target else None,
        "abi": abi,
        "debuggable": "application-debuggable" in metadata.stdout,
        "signatureVerified": signing.returncode == 0,
        "zipAlign16K": alignment.returncode == 0,
        "elfLoadAlign16K": libraries,
    }
    report["success"] = (
        metadata.returncode == 0 and report["package"] == "com.mewgic.game"
        and report["versionCode"] == str(args.code) and report["minSdk"] == 26
        and report["targetSdk"] == 36 and abi == ["arm64-v8a"] and report["debuggable"]
        and report["signatureVerified"] and report["zipAlign16K"] and all(libraries.values())
    )
    output = json.dumps(report, indent=2)
    if args.report:
        args.report.write_text(output + "\n", encoding="utf-8")
    print(output)
    return 0 if report["success"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
