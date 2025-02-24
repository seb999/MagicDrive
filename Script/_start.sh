#!/bin/bash

# Base paths for the project
BASE_PATH_DOTNET="/home/sebastien/Git/MagicDrive"
BASE_PATH_PYTHON="/home/sebastien/Git/MagicDrivePy"
LOG_FILE="$BASE_PATH_DOTNET/startup.log"


##########################################################
# Activate the virtual environment########################
##########################################################
if [ ! -d "$BASE_PATH_PYTHON/env" ]; then
  echo "Error: Virtual environment not found at $BASE_PATH_PYTHON/env" | tee -a "$LOG_FILE"
  exit 1
fi
source "$BASE_PATH_PYTHON/env/bin/activate"
echo "Python Virtual Environment ACTIVATED"
sleep 1

###############################################
# Start Socketio Server########################
###############################################
APP1="$BASE_PATH_PYTHON/_magicDriveServer.py"
if [ ! -f "$APP1" ]; then
  echo "Error: Socket.IO server script not found!"
  exit 1
fi
echo "$(date): Starting Socket.IO server..." 
"$BASE_PATH_PYTHON/env/bin/python" "$APP1" &
echo "Python Server STARTED" 
sleep 1

# ###########################################
# # Start Dotnet main #######################
# ###########################################
APP3="$BASE_PATH_DOTNET/bin/Release/net6.0/MagicDrive.dll"
if [ ! -f "$APP3" ]; then
  echo "Error: MagicDrive application not found!"
  exit 5
fi
/home/sebastien/.dotnet/dotnet "$APP3" &
echo "Magic Drive dotnet STARTED" 
sleep 3  # Delay

###############################################
# Start Streaming Video########################
###############################################
APP2="$BASE_PATH_PYTHON/_magicDriveStreaming.py"
if [ ! -f "$APP2" ]; then
  echo "Error: Streaming service script not found!"
  exit 5
fi
echo "$(date): Starting camera service..." 
"$BASE_PATH_PYTHON/env/bin/python" "$APP2"
echo "Streaming video STARTED"
sleep 3  # Delay