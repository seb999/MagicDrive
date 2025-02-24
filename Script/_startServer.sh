#!/bin/bash
# Define the path to your virtual environment
venv_path="/home/sebastien/Git/MagicDrivePy/env"

# Check if the virtual environment exists
if [ ! -d "$venv_path" ]; then
  echo "Error: Virtual environment not found at $venv_path"
  exit 1
fi

# Activate the virtual environment
source "$venv_path/bin/activate"

# Define the path to your Python application script
app_path="/home/sebastien/Git/MagicDrivePy/_magicDriveServer.py"
if [ ! -f "$app_path" ]; then
  echo "Error: socket-io server script not found!"
  exit 1
fi
echo "Starting Socket-io server..."
/home/sebastien/Git/MagicDrivePy/env/bin/python "$app_path"