#!/bin/bash

echo "Stopping proxy-gc service"
sudo systemctl stop proxy-gc

echo "Removing publish folder if exists"
if [ -d "../src/publish" ]
then
    rm -r ../src/publish
fi

echo "Publishing"
dotnet publish -c Release -r linux-arm64 -o ../src/publish --no-self-contained

echo "Starting proxy-gc service"
sudo systemctl start proxy-gc