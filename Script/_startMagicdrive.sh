#!/bin/bash

# Define the path to your .NET Core application executable
app_path="/home/sebastien/Git/MagicDrive/bin/Release/net6.0/MagicDrive.dll"

# Check if the application executable exists
if [ ! -f "$app_path" ]; then
  echo "Error: Application MagicDrive executable not found!"
  exit 1
fi

# Start the .NET Core application
echo "Starting MagicDrive .NET Core application..."
#$HOME/.dotnet/dotnet "$app_path"
/home/sebastien/.dotnet/dotnet "$app_path"
