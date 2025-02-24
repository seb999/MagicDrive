## Documentation

## Sript folder
_startOld.sh       start all with startup.log
_start.sh          start all

## Layout

PIN layout:
GPIO LED standby = 23 
GPIO LED drive = 24
GPIO SWITCH start/stop = 18
GPIO A4988 DIR = 16;  // Direction pin
GPIO A4988 STEP = 20; // Step pin
GPIO A4988 ENABLE = 21; // Enable pin
GPIO A4988 Groud = ground
GPIO A4988 vcc = 5V


```bat
https://scisharp.github.io/tensorflow-net-docs/#/tutorials/ImageRecognition
```
## systemd commands
journalctl -u sdubos.magicdrive.service
sudo nano /lib/systemd/system/sdubos.magicdrive.service
sudo systemctl enable sdubos.magicdrive.service
sudo systemctl daemon-reload
sudo systemctl start sdubos.magicdrive.service
sudo systemctl stop sdubos.magicdrive.service
sudo systemctl disable sdubos.magicdrive.service

[Unit]
Description=MagicDrive Startup Service
After=multi-user.target

[Service]
User=root
ExecStart=/bin/bash /home/sebastien/Git/MagicDrive/Script/_startBBB.sh
Restart=no

[Install]
WantedBy=multi-user.target
