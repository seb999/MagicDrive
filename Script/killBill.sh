#!/bin/bash

# Replace 'MagicDrive.dll' with a unique part of your application's command
app_command="MagicDrive.dll"

# Check if the application is running
if pgrep -f "$app_command" > /dev/null
then
    pkill -f "$app_command"
    echo "Killed the process matching command '$app_command'."
else
    echo "No process matching command '$app_command' found."
fi

app_command="_magicDriveServer.py"

# Check if the application is running
if pgrep -f "$app_command" > /dev/null
then
    pkill -f "$app_command"
    echo "Killed the process matching command '$app_command'."
else
    echo "No process matching command '$app_command' found."
fi

app_command="_magicDriveStreaming.py"

# Check if the application is running
if pgrep -f "$app_command" > /dev/null
then
    pkill -f "$app_command"
    echo "Killed the process matching command '$app_command'."
else
    echo "No process matching command '$app_command' found."
fi


