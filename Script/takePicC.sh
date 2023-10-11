#!/bin/bash
CURRENTDATE=$(date +%Y-%m-%d)
CURRENTTIME=$(date +%H:%M:%S)
libcamera-still -t 4000 -o /home/sebastien/Pictures/center/${CURRENTDATE}_${CURRENTTIME}%d --timelapse 500