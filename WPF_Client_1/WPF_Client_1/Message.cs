using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WPF_Client_1
{
    public class Message
    {
        [JsonPropertyName("message")]
        public string MessageContent { get; set; }
        public string Sender { get; set; }
        public DateTime Timestamp { get; set; }
        public int Id { get; set; }
    }
}

