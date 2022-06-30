#!/bin/bash
echo "ITWebService Docker Deployment Script"
sudo  mkdir /var/ITWebService/DutyInfo
docker run -d --name ITWebService  -p 49156:80 fursion/itwebservice:1.2.1
sudo docker cp ITWebService:/var/ITWebService/ /var/ITWebService/DutyInfo
docker rm -f ITWebService
docker run -d --name ITWebService -v /var/ITWebService:/var/ITWebService/DutyInfo -p 49156:80 fursion/itwebservice:1.2.1
echo "ITWebService Docker Runing"
