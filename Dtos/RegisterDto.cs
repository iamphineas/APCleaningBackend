namespace APCleaningBackend.Dtos
{
    public record RegisterDto(
        string FullName,
        string Email,
        string PhoneNumber,
        string Password
    );
}
