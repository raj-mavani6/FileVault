<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/MongoDB-7.0-47A248?style=for-the-badge&logo=mongodb&logoColor=white" />
  <img src="https://img.shields.io/badge/C%23-13-239120?style=for-the-badge&logo=csharp&logoColor=white" />
  <img src="https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white" />
  <img src="https://img.shields.io/badge/License-MIT-blue?style=for-the-badge" />
</p>

<h1 align="center">FileVault — Secure Cloud File Management</h1>

<p align="center">
  <strong>A modern, enterprise-grade cloud file storage platform built with ASP.NET Core 9, MongoDB & GridFS.</strong><br/>
  Premium "Sienna & Cream" dark-first UI · Chunked resumable uploads · Universal file preview · Secure sharing
</p>

---

## Features

### Authentication & Security
- Cookie-based authentication with **7-day sliding expiration**
- **BCrypt** password hashing with automatic salt generation
- Email confirmation tokens & password reset flow
- Anti-forgery tokens (CSRF protection via `X-CSRF-TOKEN`)
- IP-based **rate limiting** to prevent brute-force attacks
- Security headers middleware (`X-Frame-Options`, `X-Content-Type-Options`, CSP)
- Filename sanitization preventing **HTTP header injection** attacks
- Pluggable **virus scan** interface (extensible)
- Complete **audit trail** logging with IP tracking

### File Management
- **Chunked resumable uploads** — no file size limit, 5MB chunks, SHA-256 integrity verification
- Drag-and-drop upload with real-time progress tracking
- Browse, search, filter by type/extension/tag, and sort (name, date, size)
- **Folder system** with nested hierarchy & breadcrumb navigation
- Soft-delete → **Trash** → Restore or Permanent Delete
- File tagging, descriptions, and **version control**
- Pagination for large file collections

### Universal File Preview (30+ formats)
| Category | Supported Formats |
|---|---|
| **Images** | JPG, PNG, GIF, BMP, WebP, SVG, ICO |
| **Video** | MP4, WebM, OGG, MOV, AVI, MKV |
| **Audio** | MP3, WAV, OGG, FLAC, AAC, M4A |
| **Documents** | PDF (native), DOCX (Mammoth.js), DOC, XLS, XLSX, PPT, PPTX (Office Online Viewer) |
| **Archives** | ZIP, RAR, 7Z, TAR, GZ, BZ2 (metadata extraction via SharpCompress) |
| **Code & Text** | TXT, MD, JSON, XML, YAML, CSV, CS, JS, TS, PY, Java, CPP, SQL, PHP, Go, Rust, and more |

### Secure File Sharing
- Generate **tokenized share links** with one click
- Optional **password protection** (BCrypt hashed)
- Configurable **expiry** (days)
- **Download permission** toggle
- Access count tracking & last-accessed timestamp
- Link **revocation** support

### Premium UI/UX
- **"Sienna & Cream"** dark-first design system (`#0d0c0b` · `#4B262F` · `#F4EBE2`)
- Glassmorphism with `backdrop-filter: blur()`
- Smooth micro-animations (slide-up, fade-in, scroll-triggered)
- Dark/Light theme toggle persisted in `localStorage`
- Custom toast notification system with slide-in animations
- Fully responsive across all screen sizes
- Google Fonts (Inter) typography

---

## Architecture

```
┌───────────────────────────────────────────────────────┐
│                  MIDDLEWARE PIPELINE                  │
│  SecurityHeaders → ExceptionHandler → RateLimiter     │
├───────────────────────────────────────────────────────┤
│                  CONTROLLER LAYER                     │
│  ┌───────────────────┐  ┌───────────────────────────┐ │
│  │  MVC Controllers  │  │  RESTful API Controllers  │ │
│  │  (Razor Views)    │  │  /api/files, /api/shares  │ │
│  └────────┬──────────┘  └─────────────┬─────────────┘ │
├───────────┼───────────────────────────┼───────────────┤
│           │     SERVICE LAYER         │               │
│  Auth · File · Folder · Upload · Share · Audit        │
├───────────┼───────────────────────────┼───────────────┤
│           │     REPOSITORY LAYER      │               │
│  UserRepo · FileRepo · FolderRepo · ShareLinkRepo     │
├───────────┼───────────────────────────┼───────────────┤
│           │     DATA LAYER            │               │
│  ┌────────┴───────────────────────────┴─────────────┐ │
│  │         MongoDB 7 + GridFS (Binary Storage)      │ │
│  └──────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────┘
```

---

## Project Structure

```
FileVault/
├── FileVault.sln                         # Solution file
├── docker-compose.yml                    # MongoDB container
├── .env.example                          # Environment variables template
│
├── src/FileVault.Web/
│   ├── Program.cs                        # Entry point & DI configuration
│   ├── Controllers/
│   │   ├── HomeController.cs             # Landing, About, Contact, Privacy, Terms
│   │   ├── AuthController.cs             # Login, Register, Logout, Password Reset
│   │   ├── DashboardController.cs        # User dashboard with storage stats
│   │   ├── FilesController.cs            # File listing, details, download, preview
│   │   ├── ProfileController.cs          # Profile management & avatar upload
│   │   ├── ShareController.cs            # Public share link access
│   │   └── Api/
│   │       ├── FilesApiController.cs     # RESTful file CRUD
│   │       ├── FoldersApiController.cs   # RESTful folder CRUD
│   │       ├── UploadApiController.cs    # Chunked upload engine
│   │       └── ShareApiController.cs     # Share link API
│   ├── Services/                         # 8 services (Auth, File, Upload, Share, etc.)
│   ├── Data/
│   │   ├── MongoDbContext.cs             # MongoDB client & collections
│   │   ├── GridFs/                       # GridFS binary storage service
│   │   ├── Repositories/                 # 6 repositories with interfaces
│   │   └── Seed/                         # Database seeder
│   ├── Models/
│   │   ├── Domain/                       # 6 entities (AppUser, FileItem, Folder, etc.)
│   │   ├── ViewModels/                   # Page-specific view models
│   │   └── Settings/                     # Configuration POCOs
│   ├── Middleware/                        # Security headers & exception handling
│   ├── Helpers/                          # File type detection & sanitization
│   ├── Views/                            # Razor views (Home, Auth, Files, etc.)
│   └── wwwroot/
│       ├── css/site.css                  # Design system (450+ lines)
│       └── js/                           # Client-side modules
│
└── tests/FileVault.Tests/               # xUnit test project
```

---

## Tech Stack

| Layer | Technology | Version |
|---|---|---|
| Backend | ASP.NET Core | .NET 9.0 |
| Language | C# | 13 |
| Database | MongoDB | 7.x |
| File Storage | MongoDB GridFS | 2.30.0 |
| Password Hashing | BCrypt.Net-Next | 4.0.3 |
| Logging | Serilog (Console + File) | 9.0.0 |
| Rate Limiting | AspNetCoreRateLimit | 5.0.0 |
| Archive Handling | SharpCompress | 0.38.0 |
| Frontend | Bootstrap 5 + Vanilla JS | 5.3 |
| Icons | Bootstrap Icons | Latest |
| Typography | Google Fonts (Inter) | — |
| DOCX Preview | Mammoth.js | 1.6.0 |
| Office Preview | Microsoft Office Online Viewer | — |

---

## Quick Start

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [MongoDB 7+](https://www.mongodb.com/try/download/community) or [Docker](https://www.docker.com/)

### 1. Start MongoDB

**Option A — Docker (Recommended):**
```bash
docker-compose up -d
```

**Option B — Local MongoDB:**
Install MongoDB Community Server and ensure it's running on port `27017`.

### 2. Run the Application
```bash
cd FileVault
dotnet restore
cd src/FileVault.Web
dotnet run
```

The app starts at **https://localhost:5001**

### 3. Default Account
The database seeder creates a test account on first run:

| Field | Value |
|---|---|
| Email | `admin@filevault.local` |
| Password | `Admin@123` |

---

## API Reference

### Files — `/api/files`
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/files` | List files (pagination, search, filter, sort) |
| `GET` | `/api/files/{id}` | Get file metadata |
| `GET` | `/api/files/{id}/download` | Download file stream |
| `GET` | `/api/files/{id}/preview` | Inline file preview |
| `PUT` | `/api/files/{id}` | Update metadata (name, tags, description) |
| `DELETE` | `/api/files/{id}` | Soft delete (move to trash) |
| `DELETE` | `/api/files/{id}?permanent=true` | Permanent delete |
| `POST` | `/api/files/{id}/restore` | Restore from trash |
| `POST` | `/api/files/empty-trash` | Empty all trash |

### Upload — `/api/upload`
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/upload/initiate` | Start chunked upload session |
| `POST` | `/api/upload/{id}/chunk` | Upload a single chunk |
| `POST` | `/api/upload/{id}/complete` | Finalize & assemble |
| `GET` | `/api/upload/{id}/status` | Check upload progress |

### Folders — `/api/folders`
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/folders` | Create folder |
| `PUT` | `/api/folders/{id}` | Rename folder |
| `DELETE` | `/api/folders/{id}` | Soft delete folder |
| `POST` | `/api/folders/{id}/restore` | Restore folder |

### Shares — `/api/shares`
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/shares` | Create share link |
| `GET` | `/api/shares` | List user's share links |
| `DELETE` | `/api/shares/{id}` | Revoke share link |

### Public Share
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/s/{token}` | View shared file page |
| `POST` | `/s/{token}` | Authenticate with password |
| `GET` | `/s/{token}/download` | Download shared file |

---

## Database Collections

| Collection | Purpose |
|---|---|
| `users` | User accounts with roles, storage tracking, avatar references |
| `files` | File metadata with owner, folder, tags, versions, soft-delete |
| `folders` | Hierarchical folder structure with path tracking |
| `uploadSessions` | Chunked upload session state (chunks, status, expiry) |
| `shareLinks` | Share tokens with password, expiry, access tracking |
| `auditLogs` | Activity audit trail with user, action, IP, metadata |
| `fileVaultFiles.files` | GridFS file metadata |
| `fileVaultFiles.chunks` | GridFS binary chunks (255KB each) |

---

## Reverse Proxy Configuration

### Nginx
```nginx
client_max_body_size 0;         # No upload limit
proxy_request_buffering off;    # Stream directly
```

### IIS (web.config)
```xml
<system.webServer>
  <security>
    <requestFiltering>
      <requestLimits maxAllowedContentLength="4294967295" />
    </requestFiltering>
  </security>
</system.webServer>
```

---

## Related

- **[FileVaultAdmin](../FileVaultAdmin)** — Dedicated Admin Control Panel for platform management

---

## 📜 License

MIT — See [LICENSE](LICENSE) for details.
