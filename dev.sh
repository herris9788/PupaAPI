#!/bin/bash
docker compose -f docker-compose.dev.yml down 2>/dev/null
docker rm -f pupa-api-watch 2>/dev/null
docker compose -f docker-compose.dev.yml up
