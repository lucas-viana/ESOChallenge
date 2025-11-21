namespace WebAPI_ESOChallenge.Features.Admin.Dtos
{
    /// <summary>
    /// DTO para exibir usuário na listagem pública
    /// </summary>
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int TotalCosmetics { get; set; }
        public int VBucks { get; set; }
    }

    /// <summary>
    /// DTO para exibir detalhes do perfil de um usuário
    /// </summary>
    public class UserProfileDetailsDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int VBucks { get; set; }
        public List<UserCosmeticDto> Cosmetics { get; set; } = new();
    }

    /// <summary>
    /// DTO para cosmético do usuário
    /// </summary>
    public class UserCosmeticDto
    {
        public string CosmeticId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Price { get; set; }
        public DateTime PurchasedAt { get; set; }
        public string? BundleId { get; set; } // Identificador do bundle, se aplicável
    }
}
