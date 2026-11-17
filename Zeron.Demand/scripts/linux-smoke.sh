#!/usr/bin/env bash
# Boot-only smoke for Zeron.Demand (linux-x64). Optional full checklist: Docs/deployment.md#linux-agent-smoke-test
set -euo pipefail

AGENT_DIR="${1:-.}"
TIMEOUT_SEC="${SMOKE_TIMEOUT_SEC:-20}"
DLL="${AGENT_DIR}/Zeron.Demand.dll"
HOST="${AGENT_DIR}/Zeron.Demand"
SMOKE_CONFIG="${AGENT_DIR}/Resource/App.Smoke.config"

if [[ -f "$DLL" ]]; then
  RUN_MODE="dotnet-dll"
elif [[ -f "$HOST" ]]; then
  RUN_MODE="apphost"
  chmod +x "$HOST"
else
  echo "error: neither Zeron.Demand.dll nor Zeron.Demand apphost found under ${AGENT_DIR}" >&2
  exit 1
fi

if [[ ! -f "$SMOKE_CONFIG" ]]; then
  echo "error: missing ${SMOKE_CONFIG}" >&2
  exit 1
fi

APP_CONFIG="${AGENT_DIR}/App.config"
if [[ -f "$APP_CONFIG" ]]; then
  cp "$APP_CONFIG" "${APP_CONFIG}.smoke.bak"
  restore_config() {
  if [[ -f "${APP_CONFIG}.smoke.bak" ]]; then
      mv "${APP_CONFIG}.smoke.bak" "$APP_CONFIG"
    fi
  }
  trap restore_config EXIT
fi

cp "$SMOKE_CONFIG" "$APP_CONFIG"
export ZERON_DEMAND_FOREGROUND=1
export DOTNET_ENVIRONMENT="${DOTNET_ENVIRONMENT:-Production}"

cd "$AGENT_DIR"
LOG="$(mktemp)"
set +e
if [[ "$RUN_MODE" == "dotnet-dll" ]]; then
  timeout "${TIMEOUT_SEC}s" dotnet "./Zeron.Demand.dll" --console >"$LOG" 2>&1
else
  timeout "${TIMEOUT_SEC}s" "./Zeron.Demand" --console >"$LOG" 2>&1
fi
TIMEOUT_CODE=$?
set -e

if [[ "$TIMEOUT_CODE" -ne 124 ]]; then
  echo "error: agent exited before timeout (code=${TIMEOUT_CODE}); log:" >&2
  cat "$LOG" >&2
  exit 1
fi

grep -q "foreground host started" "$LOG" || {
  echo "error: missing 'foreground host started' in log:" >&2
  cat "$LOG" >&2
  exit 1
}

grep -q "ScriptHostBootstrapServer ready" "$LOG" || {
  echo "error: missing 'ScriptHostBootstrapServer ready' in log:" >&2
  cat "$LOG" >&2
  exit 1
}

echo "linux-smoke: OK (${TIMEOUT_SEC}s boot check)"