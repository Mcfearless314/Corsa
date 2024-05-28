namespace Backend.infrastructure.dataModels;

/**
 * This data model is for the password-hashes
 */
public class PasswordHash
{
    public int UserId { get; set; }
    public required string Hash { get; set; }
    public required string Salt { get; set; }
    public required string Algorithm { get; set; }
}