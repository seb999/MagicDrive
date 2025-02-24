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
app_path="/home/sebastien/Git/MagicDrivePy/_magicDriveStreaming.py"

# Check if the application script exists
if [ ! -f "$app_path" ]; then
  echo "Error: Streaming service script not found!"
  exit 1
fi

# Start the Python application
echo "Starting Streaming/Tensorflow camera service..."
/home/sebastien/Git/MagicDrivePy/env/bin/python "$app_path"
