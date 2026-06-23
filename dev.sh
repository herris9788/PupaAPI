#!/bin/bash
docker compose -f docker-compose.dev.yml down 2>/dev/null
docker compose -f docker-compose.dev.yml up
