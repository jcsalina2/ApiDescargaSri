#!/bin/bash
set -e

Xvfb :99 -screen 0 1920x1080x24 -ac +extension GLX +render -noreset &
XVFB_PID=$!

export DISPLAY=:99

sleep 1

exec dotnet ApiDescargaSriV9.dll
