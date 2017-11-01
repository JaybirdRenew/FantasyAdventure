using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace TextAdventure
{
	/// <summary>
	/// Static class used to save and load binary files.
	/// </summary>
	internal static class BinarySerializer
	{
		/// <summary>
		/// Serializes and writes a binary object to file.
		/// </summary>
		/// <typeparam name="T">The type of object being serialized.</typeparam>
		/// <param name="filePath">Path at which the object T should be saved after serialization.</param>
		/// <param name="objectToWrite">Object to be serialized.</param>
		/// <param name="append">Whether or not the data being written should be added to the end of an existing file, or should overwrite the existing file.</param>
		internal static void WriteToFile<T>(string filePath, T objectToWrite, bool append = false)
		{
			Stream stream = File.Open(filePath, FileMode.Create);
			try
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, objectToWrite);
			}
			catch (Exception error)
			{
				WriteLine($"Error writing binary object to file: {error.Message}");
                throw;
			}
			finally
			{
				stream.Close();
			}
		}

		/// <summary>
		/// Deserializes a file object.
		/// </summary>
		/// <typeparam name="T">The type of the object being deserialized.</typeparam>
		/// <param name="filePath">The file path of the object to be deserialized.</param>
		/// <returns>A deserialized object of type T.</returns>
		internal static T ReadFromFile<T>(string filePath)
		{
			Stream stream = File.Open(filePath, FileMode.Open);
			try
			{
				BinaryFormatter formatter = new BinaryFormatter();
				stream.Position = 0;
				return (T) formatter.Deserialize(stream);
			}
			catch (Exception error)
			{
				WriteLine($"Error reading binary object from file: {error.Message}");
				return default(T);
			}
			finally
			{
				stream.Close();
			}
		}
	}
}
