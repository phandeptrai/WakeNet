# GCP Deploy (WakeNet FE + BE in one container)

Thư mục `Gcp-deploy/` dùng để **build image (FE+BE)**, **push lên Artifact Registry**, tạo/cập nhật **Compute Engine VM**, rồi chạy container bằng `startup.sh`.

## Yêu cầu trước khi chạy

- Cài **Google Cloud SDK** (`gcloud`) và đăng nhập:

```bash
gcloud auth login
gcloud auth application-default login
```

- Cài **Docker Desktop** (hoặc Docker Engine) và đảm bảo chạy được:

```bash
docker version
```

- Tài khoản bạn dùng `gcloud` phải có quyền (ít nhất):
  - **Compute**: tạo VM, firewall rule, set metadata, reset VM
  - **Artifact Registry**: tạo repo (nếu chưa có), push image

## Cấu hình `deploy.config.json`

File: `Gcp-deploy/deploy.config.json`

Ví dụ:

```json
{
  "projectId": "wakenet-494615",
  "region": "asia-southeast1",
  "zone": "asia-southeast1-a",
  "vmName": "wakenet-vm",
  "machineType": "e2-standard-2",
  "bootDiskGb": 30,
  "networkTag": "wakenet-http",
  "firewallRuleName": "allow-wakenet-http-https",
  "artifactRegistryRegion": "asia-southeast1",
  "artifactRepoName": "wakenet",
  "imageName": "wakenet-server",
  "imageTag": "latest",
  "containerPort": 5000,
  "publicPort": 80
}
```

Giải thích nhanh:
- **projectId**: GCP Project ID.
- **region / zone**: vùng + zone VM. Script có cơ chế thử `a/b/c/d` khi zone bị thiếu tài nguyên.
- **vmName**: tên VM.
- **machineType**: cấu hình máy (ví dụ `e2-standard-2` ~ 8GB RAM).
- **bootDiskGb**: dung lượng disk OS.
- **networkTag**: tag gắn lên VM để firewall rule apply đúng VM.
- **firewallRuleName**: tên firewall rule mở cổng 80/443.
- **artifactRegistryRegion**: region của Artifact Registry (thường cùng region).
- **artifactRepoName**: tên repo Docker trong Artifact Registry.
- **imageName / imageTag**: tên + tag image.
- **containerPort**: port bên trong container (ASP.NET Core) — hiện đang là `5000`.
- **publicPort**: port public trên VM — hiện đang là `80`.

## Cách chạy deploy

Tại root repo (`WakeNetServer/`), chạy:

```powershell
.\Gcp-deploy\deploy.ps1
```

Script sẽ:
- enable API cần thiết
- tạo Artifact Registry repo nếu chưa có
- build/push Docker image (`Gcp-deploy/Dockerfile`)
- tạo firewall rule mở `80,443`
- tạo VM (nếu chưa có) hoặc update metadata + reset VM (nếu đã có)
- in ra external IP

## Sau khi deploy

Vì FE và BE được serve chung 1 app/container:
- **Frontend**: `http://<EXTERNAL_IP>/`
- **Backend API**: `http://<EXTERNAL_IP>/api/...`

## Cách VM chạy container (startup script)

VM dùng metadata để chạy `startup.sh`:
- `wakenet_image`
- `wakenet_container_port`
- `wakenet_public_port`

`startup.sh` sẽ:
- cài Docker
- **docker login vào Artifact Registry bằng access token của Service Account (metadata)**
- pull image và `docker run -p publicPort:containerPort`

## Troubleshooting nhanh

### 1) Mở IP thấy “refused to connect”
Thường do container chưa chạy hoặc chưa listen port 80. SSH vào VM:

```bash
gcloud compute ssh wakenet-vm --zone <zone>
sudo docker ps -a
sudo ss -ltnp | grep ":80\b" || true
sudo journalctl -u google-startup-scripts.service --no-pager -n 120
```

### 2) Lỗi `ZONE_RESOURCE_POOL_EXHAUSTED`
Zone đó đang hết tài nguyên. Script sẽ thử zone khác trong cùng region. Nếu vẫn fail, đổi:
- `zone`/`region` trong `deploy.config.json`, hoặc
- giảm `machineType` (ví dụ `e2-medium`) rồi deploy lại.

### 3) Lỗi pull image “Unauthenticated request”
Đây là lỗi quyền service account của VM. Bản `startup.sh` hiện tại đã `docker login` bằng token,
nhưng service account của VM vẫn cần quyền pull từ Artifact Registry (roles phù hợp).

### 4) FE build fail ở bước `npm ci`
`Dockerfile` đang dùng `npm install` để tránh lỗi lockfile không sync. Nếu muốn build ổn định hơn:
- commit đúng `package-lock.json` và chuyển lại dùng `npm ci`.

## Ghi chú

- Deploy hiện tại **không cấu hình HTTPS** (mặc định HTTP port 80). Nếu muốn HTTPS, cần reverse proxy/nginx + cert.
- DB SQLite trong container/VM sẽ phụ thuộc bạn mount volume hay không (nếu chưa mount thì data sẽ mất khi container bị thay).

# GCP Deploy (Compute Engine VM)

Mục tiêu: deploy `WakeNetServer.Server` lên 1 VM Compute Engine loại **e2-standard-2 (2 vCPU, 8GB RAM)**.

## Yêu cầu trên máy chạy deploy
- cài `gcloud` + đăng nhập: `gcloud auth login`
- cài `docker`
- có quyền tạo Compute Engine + Artifact Registry

## Files
- `deploy.config.json`: cấu hình deploy (project/region/zone/name...)
- `deploy.ps1`: script PowerShell tự động tạo VM + build/push image + deploy container
- `startup.sh`: script chạy trên VM (cài docker, run container)
- `Dockerfile`: build image chứa cả FE + BE (FE build ra `wwwroot`)

## Chạy
1) Copy `deploy.config.json` và chỉnh các giá trị.
2) Chạy:

```powershell
pwsh .\Gcp-deploy\deploy.ps1 -Config .\Gcp-deploy\deploy.config.json
```

Sau deploy:
- FE: `http://<VM_EXTERNAL_IP>/`
- BE API: `http://<VM_EXTERNAL_IP>/api/...`

