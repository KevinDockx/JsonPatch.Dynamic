﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marvin.JsonPatch.Dynamic.XUnitTest
{
    public class JsonPropertyDTO
    {
        [JsonProperty("AnotherName")]
        public string Name { get; set; }
    }
}
