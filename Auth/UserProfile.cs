using System;

namespace StreamingPlatform.Auth
{
    /// <summary>
    /// Mapează tabelul Profiles din StreamZoneDB.
    /// Profil de vizionare în stil Netflix — un cont (Account) poate avea mai multe profile.
    /// Numele complet 'StreamingPlatform.Auth.UserProfile' îl deosebește de
    /// 'StreamingPlatform.Proxy.UserProfile' (care e demo pentru pattern-ul Proxy).
    /// </summary>
    public class UserProfile
    {
        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool IsKidsProfile { get; set; }
        public string PreferredLanguage { get; set; } = "ro";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Account? Account { get; set; }
    }
}
