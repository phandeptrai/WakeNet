#!/usr/bin/env bash
set -euo pipefail

# Metadata keys expected:
# - wakenet_image
# - wakenet_container_port
# - wakenet_public_port

IMAGE="$(curl -fsS -H 'Metadata-Flavor: Google' \
  'http://metadata.google.internal/computeMetadata/v1/instance/attributes/wakenet_image')"
CONTAINER_PORT="$(curl -fsS -H 'Metadata-Flavor: Google' \
  'http://metadata.google.internal/computeMetadata/v1/instance/attributes/wakenet_container_port')"
PUBLIC_PORT="$(curl -fsS -H 'Metadata-Flavor: Google' \
  'http://metadata.google.internal/computeMetadata/v1/instance/attributes/wakenet_public_port')"

apt-get update -y
apt-get install -y ca-certificates curl gnupg

install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor --batch --yes -o /etc/apt/keyrings/docker.gpg
chmod a+r /etc/apt/keyrings/docker.gpg

echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" \
  > /etc/apt/sources.list.d/docker.list

apt-get update -y
apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

systemctl enable docker
systemctl start docker

docker rm -f wakenet-server || true

# Authenticate to Artifact Registry using the VM's service account access token.
# This avoids requiring gcloud on the instance.
AR_HOST="$(echo "$IMAGE" | cut -d/ -f1)"
TOKEN_JSON="$(curl -fsS -H 'Metadata-Flavor: Google' \
  'http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/token')"
ACCESS_TOKEN="$(python3 -c 'import json,sys; print(json.load(sys.stdin)["access_token"])' <<<"$TOKEN_JSON")"
docker login -u oauth2accesstoken -p "$ACCESS_TOKEN" "https://${AR_HOST}"

docker pull "$IMAGE"

docker run -d --restart unless-stopped --name wakenet-server \
  -p "${PUBLIC_PORT}:${CONTAINER_PORT}" \
  -e ASPNETCORE_URLS="http://0.0.0.0:${CONTAINER_PORT}" \
  "$IMAGE"

