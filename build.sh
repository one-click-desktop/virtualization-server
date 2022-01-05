#! /bin/bash

docker build runtime_container -t one-click-desktop/virtualization-server-runtime
docker build . -t one-click-desktop/virtualization-server