#!/bin/bash
CURRENTDATE=`date +"%s"`
CURRENTTIME=`date +%H:%M:%S`
libcamera-jpeg -o /home/sebastien/Pictures/left/${CURRENTDATE}.jpg -t 500

