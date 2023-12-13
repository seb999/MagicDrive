#!/bin/bash

# Define the path to your Python application script
app_path="/home/sebastien/Git/MagicDrivePyService/_magicDriveSocketIoServer.py"
if [ ! -f "$app_path" ]; then
  echo "Error: socket-io server script not found!"
  exit 1
fi
echo "Starting Socket-io server..."
/usr/bin/python3 "$app_path" &


app_path2="/home/sebastien/Git/MagicDrive/bin/Release/net6.0/MagicDrive.dll"
if [ ! -f "$app_path2" ]; then
  echo "Error: Application MagicDrive executable not found!"
  exit 1
fi
echo "Starting MagicDrive .NET Core application..."
#$HOME/.dotnet/dotnet "$app_path2"
/home/sebastien/.dotnet/dotnet "$app_path2" &

app_path3="/home/sebastien/Git/MagicDrivePyService/_magicDriveStreaming.py"
if [ ! -f "$app_path3" ]; then
  echo "Error: Streaming service script not found!"
  exit 1
fi
echo "Starting Streaming/Tensorflow camera service..."
sleep 20
/usr/bin/python3 "$app_path3"

