using System.Collections.Generic;

namespace WebAPI_ESOChallenge.Features.Cosmetics.Dtos
{
    public class CosmeticsListResponse
    {
        public ItemsDto? Items { get; set; }
    }

    public class ItemsDto
    {
        public List<CosmeticDto>? Br { get; set; }
    }
}