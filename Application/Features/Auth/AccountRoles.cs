namespace Application.Features.Auth;

public static class AccountRoles
{
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";
    public const string Admin = "Admin";

    public static bool IsValid(string? role)
        => role is Patient or Doctor or Admin;
}
