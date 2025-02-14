﻿using System;
using System.Text.Json.Serialization;

namespace StormGlass
{
    public class WindDirection
    {
        [JsonPropertyName("icon")]
        public float Icon { get; set; }

        [JsonPropertyName("noaa")]
        public float Noaa { get; set; }

        [JsonPropertyName("sg")]
        public float Sg { get; set; }
    }
}
