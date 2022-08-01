#!/bin/bash

export ASPNETCORE_ENVIRONMENT='Production'
cd ../source/ProxyService
dotnet ef database update