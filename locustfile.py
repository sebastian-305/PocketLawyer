from locust import User, task, between, events
from websocket import create_connection, WebSocketConnectionClosedException
import json, base64, os, time, ssl, random

HOST = "ws://localhost:5112"                 # <-- bei dir 5112
CONTRACT_TYPES = ["employment-analysis"]     # ggf. ["employment-analysis","rent-analysis"]
TEST_DIR = "data\\Samples"                            # Ordner mit Beispieldateien
ALLOW_SELF_SIGNED_TLS = False                # falls wss mit selbstsigniertem Zert

def pick_file():
    files = [f for f in os.listdir(TEST_DIR) if os.path.isfile(os.path.join(TEST_DIR, f))]
    if not files:
        return "ctr3.txt"                  # fallback
    return os.path.join(TEST_DIR, random.choice(files))

def is_validation(msg: dict) -> bool:
    # deine Validation hat z.B. isReadable/isContract
    return isinstance(msg, dict) and ("isReadable" in msg or "isContract" in msg)

def is_employment_analysis(msg: dict) -> bool:
    # passt zu deinem DTO: EmploymentContractAnalysisDto
    return (
        isinstance(msg, dict)
        and "keypoints" in msg
        and "fazit" in msg
        # optional weitere Felder:
        and all(k in msg for k in [
            "unzulaessige_klauseln", "nachteile_klauseln",
            "widerspruechliche_klauseln", "vorteilhafte_klauseln"
        ])
    )

class WebSocketUser(User):
    wait_time = between(1, 2)

    @task
    def analyze_contract(self):
        contract_type = random.choice(CONTRACT_TYPES)
        start = time.time()
        ws = None
        try:
            sslopt = {}
            if HOST.startswith("wss://") and ALLOW_SELF_SIGNED_TLS:
                sslopt = {"cert_reqs": ssl.CERT_NONE}

            ws = create_connection(f"{HOST}/{contract_type}", sslopt=sslopt)

            # Datei lesen & base64
            path = pick_file()
            with open(path, "rb") as f:
                b64 = base64.b64encode(f.read()).decode("utf-8")

            ws.send(json.dumps({"fileName": os.path.basename(path), "data": b64}))

            # 1) Validation empfangen
            raw1 = ws.recv()
            msg1 = json.loads(raw1)
            if not is_validation(msg1):
                raise AssertionError(f"Validation response missing/invalid: {raw1[:200]}")

            # 2) Analyse empfangen (bis Close oder bis Treffer)
            analysis_msg = None
            while True:
                try:
                    raw = ws.recv()
                except WebSocketConnectionClosedException:
                    break
                try:
                    msg = json.loads(raw)
                except Exception:
                    continue

                # Fehlerrahmen vom Server erkennen (falls vorhanden)
                if isinstance(msg, dict) and msg.get("error"):
                    raise AssertionError(f"Server error: {msg.get('error')}")

                # Arbeitsvertrag erkennen
                if contract_type == "employment-analysis" and is_employment_analysis(msg):
                    analysis_msg = msg
                    break
                # (Optional: Schema-Check für rent-analysis hier ergänzen)

            if analysis_msg is None:
                raise AssertionError("No analysis result matching expected schema")

            total_ms = int((time.time() - start) * 1000)
            events.request.fire(
                request_type="websocket",
                name=contract_type,
                response_time=total_ms,
                response_length=len(json.dumps(analysis_msg)),
                exception=None,
            )

        except Exception as e:
            total_ms = int((time.time() - start) * 1000)
            events.request.fire(
                request_type="websocket",
                name=contract_type if "contract_type" in locals() else "ws",
                response_time=total_ms,
                response_length=0,
                exception=e,
            )
        finally:
            try:
                if ws: ws.close()
            except: pass
