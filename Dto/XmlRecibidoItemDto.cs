using Newtonsoft.Json;

namespace ApiDescargaSriV9.Dto
{
    public class XmlRecibidoItemDto
    {
        [JsonProperty("claveAcceso")]
        public string ClaveAcceso { get; set; } = "";

        [JsonProperty("xml")]
        public string Xml { get; set; } = "";
    }
}
