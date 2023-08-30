#!/bin/bash
CURRENTDATE=`date +"%s"`
libcamera-still -t 15000 -o /home/sebastien/Pictures/timeLapse/${CURRENTDATE}%d.jpg --timelapse 500