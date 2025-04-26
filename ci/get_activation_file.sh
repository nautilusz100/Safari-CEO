#!/usr/bin/env bash

set -e

activation_file="Unity_v2022.3.24f1.alf"


if [[ -z "${UNITY_USERNAME}" ]] || [[ -z "${UNITY_PASSWORD}" ]]; then
  echo "UNITY_USERNAME or UNITY_PASSWORD environment variables are not set."
  exit 1
fi

echo "Generating Unity manual activation file..."

xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' \
  unity-editor \
    -batchmode \
    -nographics \
    -logFile /dev/stdout \
    -createManualActivationFile \
    -manualLicenseFile "${activation_file}" \
    -username "$UNITY_USERNAME" \
    -password "$UNITY_PASSWORD"

# Sanity check
if [[ -s "${activation_file}" ]]; then
  echo ""
  echo "### Success: ${activation_file} was generated!"
  echo ""
  echo "Now upload it to https://license.unity3d.com/manual to get your .ulf file."
else
  echo "Error: ${activation_file} is empty or not created."
  exit 1
fi
