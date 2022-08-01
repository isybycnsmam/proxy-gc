#!/bin/bash

echo "Stopping proxy-gc service"
sudo systemctl stop proxy-gc

echo "Removing publish folder if exists"
if [ -d "../publish" ]
then
    rm -r ../publish
fi

echo "Publishing"
cd ../source/ProxyService
dotnet publish -c Release -r linux-arm64 -o ../../publish --no-self-contained

echo "Starting proxy-gc service"
sudo systemctl start proxy-gc
