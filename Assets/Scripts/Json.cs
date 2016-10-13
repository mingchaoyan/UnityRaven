using System;
using System.Collections.Generic;
using FullSerializer;
using UnityEngine;

namespace FullSerializer {
	public partial class fsConverterRegistrar {
		public static fsData_DirectConverter Register_fsData_DirectConverter;
	}
}

public class fsData_DirectConverter : fsDirectConverter
{
	public override Type ModelType { get { return typeof(fsData); } }

	public override object CreateInstance(fsData data, Type storageType)
	{
		return new fsData();
	}

	public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
	{
		serialized = (fsData)instance;
		return fsResult.Success;
	}

	public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
	{
		instance = data ?? fsData.Null;
		return fsResult.Success;
	}
}

public static class Json
{
	private static readonly fsSerializer _serializer = new fsSerializer();
	private static readonly Type _fsDataType = typeof(fsData);
	private static readonly Type _dictType = typeof(Dictionary<string, fsData>);
	private static readonly Type _listType = typeof(List<fsData>);

	public static string Generate(fsData instance)
	{
		return fsJsonPrinter.CompressedJson(instance);
	}

	public static string Generate(Type storageType, object instance)
	{
		if (storageType == _fsDataType) {
			return Generate(instance as fsData);
		} else if (storageType == _dictType) {
			return Generate(new fsData(instance as Dictionary<string, fsData>));
		} else if (storageType == _listType) {
			return Generate(new fsData(instance as List<fsData>));
		}

		fsData data;
		if (LogResult(_serializer.TrySerialize(storageType, instance, out data)).Succeeded) {
			return Generate(data);
		}

		return null;
	}

	public static string Generate<T>(T instance)
	{
		if (typeof(T) == _fsDataType) {
			return Generate(instance as fsData);
		} else if (typeof(T) == _dictType) {
			return Generate(new fsData(instance as Dictionary<string, fsData>));
		} else if (typeof(T) == _listType) {
			return Generate(new fsData(instance as List<fsData>));
		}

		fsData data; 
		if (LogResult(_serializer.TrySerialize(instance, out data)).Succeeded) {
			return Generate(data);
		}

		return null;
	}

	public static fsData Parse(string input)
	{
		fsData data;
		if (LogResult(fsJsonParser.Parse(input, out data)).Succeeded) {
			return data;
		}

		return fsData.Null;
	}

	public static object Parse(Type type, string input)
	{
		return Parse(type, Parse(input));
	}

	public static object Parse(Type type, fsData input)
	{
		if (input != null) {
			if (type == _fsDataType) {
				return input;
			}

			object deserialized = null;
			if (LogResult(_serializer.TryDeserialize(input, type, ref deserialized)).Succeeded) {
				return deserialized;
			}
		}

		return null;
	}

	public static T Parse<T>(string input)
	{
		return (T)Parse(typeof(T), input);
	}

	public static T Parse<T>(fsData input)
	{
		return (T)Parse(typeof(T), input);
	}

	public static T Parse<T>(string input, T instance)
	{
		object deserialized = Parse(typeof(T), input);
		if (deserialized != null) {
			return (T)deserialized;
		}

		return instance;
	}

	private static fsResult LogResult(fsResult result)
	{
		if (result.Failed) {
			Debug.LogError(result.FormattedMessages);
		} else if (result.HasWarnings) {
			Debug.LogWarning(result.FormattedMessages);
		}

		return result;
	}

}
