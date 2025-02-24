#!/bin/bash

set -e  # Exit immediately if any command fails
set -u  # Treat unset variables as an error

# Uncomment the next line only if running in bash (not in sh)
set -o pipefail  # Catch errors in pipelines

# Base paths for the project
BASE_PATH_DOTNET="/home/sebastien/Git/MagicDrive"
BASE_PATH_PYTHON="/home/sebastien/Git/MagicDrivePy"
LOG_FILE="$BASE_PATH_DOTNET/startup.log"

# Ensure log directory exists
mkdir -p "$(dirname "$LOG_FILE")"
echo "============================================================" >> "$LOG_FILE"
echo "$(date): Starting services..." | tee -a "$LOG_FILE"

##########################################################
# Activate the virtual environment########################
##########################################################
if [ ! -d "$BASE_PATH_PYTHON/env" ]; then
  echo "Error: Virtual environment not found at $BASE_PATH_PYTHON/env" | tee -a "$LOG_FILE"
  exit 1
fi
source "$BASE_PATH_PYTHON/env/bin/activate"
echo "Python Virtual Environment ACTIVATED" | tee -a "$LOG_FILE"
sleep 1  # Delay

###############################################
# Start Socketio Server########################
###############################################
APP1="$BASE_PATH_PYTHON/_magicDriveServer.py"
if [ ! -f "$APP1" ]; then
  echo "Error: Socket.IO server script not found!" | tee -a "$LOG_FILE"
  exit 1
fi
echo "$(date): Starting Socket.IO server..." | tee -a "$LOG_FILE"
"$BASE_PATH_PYTHON/env/bin/python" "$APP1" &>> "$LOG_FILE" &
echo "Python Server STARTED" | tee -a "$LOG_FILE" &
sleep 5

# ###########################################
# # Start Dotnet main #######################
# ###########################################
APP3="$BASE_PATH_DOTNET/bin/Release/net6.0/MagicDrive.dll"
if [ ! -f "$APP3" ]; then
  echo "Error: MagicDrive application not found!" | tee -a "$LOG_FILE"
  exit 5
fi
/home/sebastien/.dotnet/dotnet "$APP3" &>> "$LOG_FILE" &
echo "Magic Drive dotnet STARTED" | tee -a "$LOG_FILE"

###############################################
# Start Streaming Video########################
###############################################
APP2="$BASE_PATH_PYTHON/_magicDriveStreaming.py"
if [ ! -f "$APP2" ]; then
  echo "Error: Streaming service script not found!" | tee -a "$LOG_FILE"
  exit 5
fi
echo "$(date): Starting camera service..." | tee -a "$LOG_FILE"
"$BASE_PATH_PYTHON/env/bin/python" "$APP2"
echo "Streaming video STARTED" | tee -a "$LOG_FILE"
sleep 1  # Delay


