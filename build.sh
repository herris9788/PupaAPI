#!/bin/bash
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
    SRC=$(pwd -W)
    MSYS_NO_PATHCONV=1 docker run --rm \
      -v "$SRC:/src" \
      -w //src/Pupa \
      mcr.microsoft.com/dotnet/sdk:10.0 \
      dotnet publish Pupa.csproj -c Release -o bin/Release/net10.0/publish
else
    docker run --rm \
      -v "$(pwd):/src" \
      -w /src/Pupa \
      mcr.microsoft.com/dotnet/sdk:10.0 \
      dotnet publish Pupa.csproj -c Release -o bin/Release/net10.0/publish
fi
