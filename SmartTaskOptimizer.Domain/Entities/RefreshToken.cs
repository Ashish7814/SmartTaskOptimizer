namespace SmartTaskOptimizer.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        //<summary>
        //    SHA256 hssh of the actual refresh token.
        //    Never store the raw refresh token in the 
        //    database
        //</summary>
        public string TokenHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedBytokenHash { get; set; }
        public string? CreatedByIp { get; set; }
        public string? RevokedByIp { get; set; }
        //<summary>
        //    Used for Optimistic concurrency during 
        //     refresh-token rotation
        // </summary>
        public byte[] RowVersion { get; set;}
        public bool IsExpired =>
            DateTime.UtcNow >= ExpiresAt;

        public bool IsRevoked =>
            RevokedAt.HasValue;
        public bool IsActive =>
            !IsExpired && !IsRevoked;
    }
}
