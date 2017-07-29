using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Using a Modified MSA Model for moduleII.
namespace Moduleni
{
    public class PhoneModel
    {
        [JsonProperty(PropertyName = "Id")]
        public String ID { get; set; }

        [JsonProperty(PropertyName = "Phone")]
        public bool IsPhone { get; set; }

        [JsonProperty(PropertyName = "Phones_Probability")]
        public double PProbability { get; set; }

        [JsonProperty(PropertyName = "Brand")]
        public string Brand { get; set; }

        [JsonProperty(PropertyName = "Chance_of_Brand")]
        public double BProbability { get; set; }


    }
}
