using Newtonsoft.Json;

namespace Core.NeopleOpenApi.Model
{
    public class Status
    {
        [JsonProperty("name")]
        public string SlotId { get; set; } = default!;

        [JsonProperty("value")]
        public string SlotName { get; set; } = default!;
    }

    public class Enchant
    {
        [JsonProperty("status")]
        public Status[]? SlotId { get; set; } = default!;

        [JsonProperty("upgrade")]
        public int Upgrade { get; set; } = default!;
    }

    public class Slot
    {
        [JsonProperty("slotId")]
        public string SlotId { get; set; } = default!;

        [JsonProperty("slotName")]
        public string SlotName { get; set; } = default!;
    }

    public class CardInfo
    {
        [JsonProperty("slots")]
        public Slot[]? Slots { get; set; }

        [JsonProperty("enchant")]
        public Enchant[]? Enchants { get; set; }
    }

    public class CardDetail
    {
        [JsonProperty("itemId")]
        public string ItemId { get; set; } = default!;

        [JsonProperty("itemName")]
        public string ItemName { get; set; } = default!;

        [JsonProperty("itemRarity")]
        public string ItemRarity { get; set; } = default!;

        // [JsonProperty("itemType")]
        // public string ItemType { get; set; } = default!;

        // [JsonProperty("itemTypeDetail")]
        // public string ItemTypeDetail { get; set; } = default!;

        // [JsonProperty("itemAvailableLevel")]
        // public int ItemAvailableLevel { get; set; }

        // [JsonProperty("itemObtainInfo")]
        // public string ItemObtainInfo { get; set; } = default!;

        // [JsonProperty("itemExplain")]
        // public string ItemExplain { get; set; } = default!;

        // [JsonProperty("itemFlavorText")]
        // public string ItemFlavorText { get; set; } = default!;

        // [JsonProperty("setItemId")]
        // public string? SetItemId { get; set; }

        // [JsonProperty("setItemName")]
        // public string? SetItemName { get; set; }

        [JsonProperty("cardInfo")]
        public CardInfo CardInfo { get; set; } = default!;
    }
}
