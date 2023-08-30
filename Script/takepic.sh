#!/bin/bash
CURRENTDATE=`date +"%s"`
CURRENTTIME=`date +%H:%M:%S`
libcamera-jpeg -o Right-${CURRENTDATE}.jpg -t 800

