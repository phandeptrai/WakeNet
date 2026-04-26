# WakeNet

## Yêu cầu
- .NET SDK (net8.0)
- Node.js + npm

## Cấu hình môi trường
Tạo file `.env` ở thư mục root (cùng cấp `WakeNet.sln`). Có thể copy từ `.env.example`.

Các biến đang dùng:
- `WAKENET_SQLITE_PATH`: đường dẫn file SQLite database.
- `WAKENET_CORS_ORIGINS`: danh sách origin FE được phép gọi API (phân tách bởi `,` hoặc `;`).

## Chạy Backend (WakeNetServer.Server)

- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`

Chạy:

```powershell
dotnet run --project .\WakeNetServer.Server\WakeNetServer.Server.csproj
```

DB SQLite sẽ được tự tạo khi server start (EnsureCreated).

## Chạy Frontend (wakenetserver.client)

Chạy dev server (port 3000) và proxy `/api` sang BE:

```powershell
cd .\wakenetserver.client
npm i
npm run dev
```

Mặc định FE chạy: `http://localhost:3000`

## Auth (Session)
BE cung cấp các API:
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`

FE có 2 trang:
- `/register`
- `/login`

