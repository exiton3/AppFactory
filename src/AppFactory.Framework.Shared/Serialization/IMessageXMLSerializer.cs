﻿namespace AppFactory.Framework.Shared.Serialization;

public interface IMessageXmlSerializer
{
    string Serialize(object objectToSerialize);
    T Deserialize<T>(string xml);
    T DeserializeInnerSoapObject<T>(string soapResponse,string path);
}