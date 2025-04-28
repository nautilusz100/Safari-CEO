#!/usr/bin/env bash

set -euo pipefail
set -x

echo "Starting Unity Build for target: $BUILD_TARGET"

# Set Unity binary path
UNITY_BINARY=${UNITY_EXECUTABLE:-/opt/unity/Editor/Unity}

# Check if Unity binary exists
if [ ! -x "$UNITY_BINARY" ]; then
  echo "ERROR: Unity executable not found at '$UNITY_BINARY'"
  exit 1
fi

# Prepare build folder
export BUILD_PATH="$UNITY_DIR/Builds/$BUILD_TARGET/"
mkdir -p "$BUILD_PATH"

# Run Unity build
echo "Running Unity in batchmode..."
xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' "$UNITY_BINARY" \
  -projectPath "$UNITY_DIR" \
  -quit \
  -batchmode \
  -nographics \
  -buildTarget "$BUILD_TARGET" \
  -customBuildTarget "$BUILD_TARGET" \
  -customBuildName "$BUILD_NAME" \
  -customBuildPath "$BUILD_PATH" \
  -executeMethod BuildCommand.PerformBuild \
  -logFile /dev/stdout

UNITY_EXIT_CODE=$?

# Handle Unity exit codes
case $UNITY_EXIT_CODE in
  0)
    echo "Unity Run succeeded, no failures occurred."
    ;;
  2)
    echo "Unity Run succeeded, but some tests failed."
    ;;
  3)
    echo "Unity Run failure (other failure)."
    ;;
  *)
    echo "Unexpected Unity exit code: $UNITY_EXIT_CODE"
    ;;
esac

# Verify build output
echo "Build output files in $BUILD_PATH:"
ls -la "$BUILD_PATH"

echo "Build completed successfully!"
