using System.Text.Json.Serialization;

namespace CaoThaiToan_2380602281_KTGK_WebAPI.models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }

        [JsonIgnore]
        public ICollection<KidDevice> Devices { get; set; } = new List<KidDevice>();
    }
}
