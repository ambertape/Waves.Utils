﻿using System.IO;
using Newtonsoft.Json;

namespace Waves.Utils.Serialization
{
    /// <summary>
    /// JSON serialization.
    /// </summary>
    public static class Json 
    {
        /// <summary>
        ///        Writes object to JSON file.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="filePath">Path.</param>
        /// <param name="objectToWrite">Object.</param>
        public static void WriteToFile<T>(string filePath, T objectToWrite)
        {
            var json = JsonConvert.SerializeObject(
                objectToWrite, 
                Formatting.Indented);

            File.WriteAllText(filePath, json);
        }

        /// <summary>
        ///     Reads object from JSON file.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="filePath">Path.</param>
        /// <returns>Object.</returns>
        public static T ReadFile<T>(string filePath)
        {
            var json = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            });
        }
    }
}
