using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Vintagestory.API.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DayNightCycles;

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public class Config {
    [YamlMember(Order = 0)]
    public SpeedConfig Speed = new();

    public class SpeedConfig {
        [YamlMember(Order = 0, Description = "Number of IRL minutes should days last.\nVanilla default is 24 minutes.")]
        public float Day = 24;

        [YamlMember(Order = 1, Description = "Number of IRL minutes should nights last.\nVanilla default is 24 minutes.")]
        public float Night = 24;
    }

    public static Config Reload(string filename) {
        string path = Path.Combine(GamePaths.ModConfig, filename);
        return Write(Read(path), path);
    }

    private static Config Read(string path) {
        try {
            return new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(NullNamingConvention.Instance)
                .Build().Deserialize<Config>(File.ReadAllText(path));
        }
        catch (Exception) {
            return new Config();
        }
    }

    private static Config Write(Config config, string path) {
        File.WriteAllText(path, new SerializerBuilder()
                .WithQuotingNecessaryStrings()
                .WithNamingConvention(NullNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build().Serialize(config)
            , Encoding.UTF8);
        return config;
    }
}
