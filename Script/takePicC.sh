#!/bin/bash
CURRENTDATE=`date +"%s"`
CURRENTTIME=`date +%H:%M:%S`
libcamera-jpeg -o /home/sebastien/Pictures/center/${CURRENTDATE}.jpg -t 500

