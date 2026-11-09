"""Start a disposable loopback API and run the Unity clan integration probe."""
import json
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from threading import Thread
from pathlib import Path
import socket
import subprocess
import tempfile
import time
from urllib.error import URLError
from urllib.request import urlopen

from test_http import REPO, SERVER

ENDPOINT = REPO / "Library" / "ClanQaEndpoint.txt"
COLD_ENDPOINT = REPO / "Library" / "ClanQaColdEndpoint.txt"
REPORT = REPO / "docs" / "ProductionReports" / "Clans" / "PLAY_MODE.json"


class GatewayFixture(BaseHTTPRequestHandler):
    def do_GET(self):
        if self.path == "/html/v1/me":
            status, mime, body = 200, "text/html", b"<p>Starting service</p>"
        else:
            status, mime, body = 503, "application/json", b'{"error":"retry"}'
        self.send_response(status)
        self.send_header("Content-Type",mime)
        self.send_header("Content-Length",str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def log_message(self, *args):
        pass


def main():
    if not SERVER.is_file():
        raise SystemExit("Önce dotnet build tools/ClanServer -c Release çalıştır.")
    if not ENDPOINT.parent.is_dir():
        raise SystemExit("Önce Unity projesini aç.")
    with socket.socket() as listener:
        listener.bind(("127.0.0.1", 0))
        port = listener.getsockname()[1]
    base = f"http://127.0.0.1:{port}"
    try:
        with ENDPOINT.open("x", encoding="utf-8") as endpoint:
            endpoint.write(base)
    except FileExistsError:
        raise SystemExit("Clan QA uç nokta dosyası zaten var. Önce önceki QA koşusunu kapat.")
    gateway = ThreadingHTTPServer(("127.0.0.1",0),GatewayFixture)
    thread = Thread(target=gateway.serve_forever,daemon=True)
    owns_cold_fixture = False
    try:
        with COLD_ENDPOINT.open("x",encoding="utf-8") as output:
            owns_cold_fixture = True
            output.write(f"http://127.0.0.1:{gateway.server_port}")
        thread.start()
        with tempfile.TemporaryDirectory(prefix="mewgic-unity-clans-") as directory:
            root = Path(directory)
            with (root / "service.log").open("wb") as log:
                server = subprocess.Popen(["dotnet", str(SERVER), "--urls", base, "--Database", str(root / "clans.db")],
                                          cwd=REPO, stdout=log, stderr=subprocess.STDOUT,
                                          creationflags=getattr(subprocess,"CREATE_NO_WINDOW",0))
                try:
                    for _ in range(70):
                        try:
                            with urlopen(base + "/health", timeout=2) as health:
                                if health.status == 200:
                                    break
                        except (URLError, TimeoutError):
                            time.sleep(.15)
                    else:
                        raise RuntimeError((root / "service.log").read_text(encoding="utf-8"))
                    previous = REPORT.stat().st_mtime_ns if REPORT.exists() else 0
                    subprocess.run(["unity","command","editor","invoke_menu","Mewgic/QA/Run Clans","--json"],
                                   cwd=REPO, check=True, timeout=45)
                    for _ in range(300):
                        if REPORT.exists() and REPORT.stat().st_mtime_ns > previous:
                            try:
                                result = json.loads(REPORT.read_text(encoding="utf-8"))
                            except json.JSONDecodeError:
                                time.sleep(.5)
                                continue
                            print(f"{result['count']} Play Mode kontrolü. {REPORT}")
                            if not result["success"]:
                                raise SystemExit(result["failure"] or result["errors"])
                            return
                        time.sleep(.5)
                    raise TimeoutError("Unity klan raporu 150 saniye içinde yazılmadı.")
                finally:
                    if server.poll() is None:
                        server.terminate()
                        server.wait(timeout=10)
    finally:
        if thread.is_alive():
            gateway.shutdown()
            thread.join()
        gateway.server_close()
        if owns_cold_fixture:
            COLD_ENDPOINT.unlink(missing_ok=True)
        ENDPOINT.unlink(missing_ok=True)


if __name__ == "__main__":
    main()
