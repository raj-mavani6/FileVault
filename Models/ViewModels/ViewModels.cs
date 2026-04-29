using System.ComponentModel.DataAnnotations;

namespace FileVault.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = null!;
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = null!;
}

public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = null!;

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = null!;
}

public class DashboardViewModel
{
    public long TotalFiles { get; set; }
    public long TotalFolders { get; set; }
    public long StorageUsedBytes { get; set; }
    public string StorageUsedFormatted { get; set; } = "0 B";
    public long SharedCount { get; set; }
    public long TrashCount { get; set; }
    public List<FileItemViewModel> RecentFiles { get; set; } = new();
    public Dictionary<string, long> FileTypeStats { get; set; } = new();
}

public class FileItemViewModel
{
    public string Id { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string Extension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string SizeFormatted { get; set; } = "0 B";
    public string? FolderId { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string FileIcon { get; set; } = "bi-file-earmark";
    public bool IsPreviewable { get; set; }
    public int VersionNumber { get; set; }
}

public class FolderViewModel
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ParentFolderId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FileListViewModel
{
    public List<FileItemViewModel> Files { get; set; } = new();
    public List<FolderViewModel> Folders { get; set; } = new();
    public string? CurrentFolderId { get; set; }
    public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public long TotalFiles { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalFiles / PageSize);
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; } = true;
    public string? SearchTerm { get; set; }
    public string? FilterExtension { get; set; }
    public string? FilterTag { get; set; }
}

public class BreadcrumbItem
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class ShareCreateViewModel
{
    [Required]
    public string FileId { get; set; } = null!;
    public bool AllowDownload { get; set; } = true;
    public int ExpiryDays { get; set; } = 7;
    public string? Password { get; set; }
}

public class ShareAccessViewModel
{
    public string Token { get; set; } = null!;
    public bool RequiresPassword { get; set; }
    public string? FileName { get; set; }
    public string? FileSize { get; set; }
    public bool AllowDownload { get; set; }
    public bool IsExpired { get; set; }
    public bool IsRevoked { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Password { get; set; }
}

public class ProfileViewModel
{
    public string Id { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string StorageUsed { get; set; } = "0 B";
    public long TotalFiles { get; set; }
}

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = null!;

    [Required]
    [Compare("NewPassword")]
    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; } = null!;
}

public class ContactViewModel
{
    [Required(ErrorMessage = "Your name is required")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Subject is required")]
    public string Subject { get; set; } = null!;

    [Required(ErrorMessage = "Message is required")]
    [StringLength(2000)]
    public string Message { get; set; } = null!;
}
