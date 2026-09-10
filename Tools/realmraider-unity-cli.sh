#!/usr/bin/env bash

set -u

UNITY_EDITOR="/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity"
SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIRECTORY="$(cd "$SCRIPT_DIRECTORY/.." && pwd)"
RUN_STAMP="$(date -u +%Y%m%dT%H%M%SZ)"
OUTPUT_DIRECTORY="$PROJECT_DIRECTORY/Logs/CLI/$RUN_STAMP"
HUB_LICENSE_PIPE=""

usage() {
    echo "Usage: $0 export-android | export-ios | export-ios-simulator"
}

require_closed_editor() {
    pgrep -x Unity >/dev/null 2>&1
    local process_status=$?
    case "$process_status" in
        0)
            echo "Refusing to start: Unity Editor is already running. Close it and retry."
            exit 20
            ;;
        1)
            ;;
        *)
            echo "Refusing to start: unable to verify whether Unity Editor is running."
            exit 22
            ;;
    esac
}

require_editor() {
    if [[ ! -x "$UNITY_EDITOR" ]]; then
        echo "Unity 6000.6.0f1 was not found at: $UNITY_EDITOR"
        exit 21
    fi
}

require_hub() {
    pgrep -f '/Unity Hub.app/Contents/MacOS/Unity Hub' >/dev/null 2>&1
    local hub_status=$?
    case "$hub_status" in
        0)
            ;;
        1)
            echo "Refusing to start: Unity Hub must be open and signed in for the Personal licence."
            exit 23
            ;;
        *)
            echo "Refusing to start: unable to verify whether Unity Hub is running."
            exit 24
            ;;
    esac
}

resolve_hub_license_pipe() {
    local licensing_pid
    local licensing_process
    licensing_pid="$(pgrep -f '^/Applications/Unity Hub.app/Contents/Frameworks/UnityLicensingClient_V1.app/Contents/MacOS/Unity.Licensing.Client ' | head -n 1)"
    if [[ -z "$licensing_pid" ]]; then
        echo "Refusing to start: Unity Hub licensing process was not found. Confirm that Hub is signed in."
        exit 25
    fi
    licensing_process="$(ps -p "$licensing_pid" -o command=)"
    if [[ "$licensing_process" =~ --namedPipe[[:space:]]+([^[:space:]]+) ]]; then
        HUB_LICENSE_PIPE="${BASH_REMATCH[1]}"
        return
    fi
    echo "Refusing to start: Unity Hub licensing channel was not found. Confirm that Hub is signed in."
    exit 25
}

prepare_run() {
    require_closed_editor
    require_editor
    require_hub
    resolve_hub_license_pipe
    mkdir -p "$OUTPUT_DIRECTORY"
    cd "$PROJECT_DIRECTORY"
    git rev-parse HEAD > "$OUTPUT_DIRECTORY/git-head.txt"
    git status --short > "$OUTPUT_DIRECTORY/git-status.txt"
}

run_export() {
    local target="$1"
    local method="$2"
    local label="$3"
    local expected_output="$4"
    local completion_text="$5"
    local log_file="$OUTPUT_DIRECTORY/${label}.log"

    "$UNITY_EDITOR" \
        -batchmode \
        -licensingIpc "$HUB_LICENSE_PIPE" \
        -quit \
        -projectPath "$PROJECT_DIRECTORY" \
        -buildTarget "$target" \
        -executeMethod "$method" \
        -logFile "$log_file"
    local unity_exit=$?
    if [[ "$unity_exit" -ne 0 ]]; then
        echo "Unity export failed with exit code $unity_exit. Log: $log_file"
        return "$unity_exit"
    fi
    if [[ ! -d "$PROJECT_DIRECTORY/$expected_output" ]]; then
        echo "Unity returned success but the expected export is missing: $expected_output"
        return 40
    fi
    if ! grep -Fq "$completion_text" "$log_file"; then
        echo "Unity returned success but the completion marker is missing. Log: $log_file"
        return 41
    fi
    echo "Unity export completed. Log: $log_file"
}

if [[ $# -lt 1 ]]; then
    usage
    exit 2
fi

prepare_run

case "$1" in
    export-android)
        run_export Android RealmRaiders.Editor.PlatformBuild.ExportAndroidStudioProject android-export Builds/AndroidStudio "Realm Raiders Android export completed"
        ;;
    export-ios)
        run_export iOS RealmRaiders.Editor.PlatformBuild.ExportXcodeProject ios-export Builds/Xcode "Realm Raiders iOS export completed"
        ;;
    export-ios-simulator)
        run_export iOS RealmRaiders.Editor.PlatformBuild.ExportXcodeSimulatorProject ios-simulator-export Builds/XcodeSimulator "Realm Raiders iOS export completed"
        ;;
    *)
        usage
        exit 2
        ;;
esac
