using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Model
{
  public class ServerError
  {
    public string message { get; set; }
    public Dictionary<string, string[]> errors { get; set; }
  }

  public partial class RESTWrapper : RestClient, IDisposable
  {
    public RESTWrapper(string baseURL, int timeout) : base(baseURL)
    {
      ServicePointManager.Expect100Continue = true;
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

      ServicePointManager.DefaultConnectionLimit = 1000;

      OurWebClient = new System.Net.Http.HttpClient();

      if(timeout > 0)
        OurWebClient.Timeout = TimeSpan.FromMinutes(timeout);
    }

    /// <summary>
    /// Ping API server (using ICMP) to see if it is accessible. Times out in 5 seconds. BaseUri property must be set before
    /// calling this function.
    /// </summary>
    /// <returns></returns>
    public bool IsServiceAccessible()
    {
      if (string.IsNullOrEmpty(this.BaseUrl?.AbsolutePath))
        return false;
      else
      {
        using (var P = new System.Net.NetworkInformation.Ping())
        {
          var Reply = P.Send(this.BaseUrl.AbsolutePath, 5);
          return (Reply.Status == System.Net.NetworkInformation.IPStatus.Success);
        }
      }
    }

    /// <summary>
    /// Executes a REST call using the specified method, parameters and headers. This base method is used by the two other overloads.
    /// </summary>
    /// <typeparam name="T">Type of returned deserialized object</typeparam>
    /// <typeparam name="U">Json type of target node</typeparam>
    /// <param name="resource">Relative URL of the API resource</param>
    /// <param name="method">HTTP Method</param>
    /// <param name="parameters">parameters including query, header and segment parameters</param>
    /// <returns>IResponse object returned by the service.</returns>
    private IRestResponse ExecuteRest(string resource, Method method, Parameter[] parameters, object jsonBody, FileParameter[] files)
    {
      var request = new RestRequest(resource, method);

      if (parameters != null && parameters.Length > 0)
      {
        //Add parameters to the rest call
        foreach (var p in parameters)
          request.AddParameter(p.Name, p.Value, p.Type);
      }

      if (jsonBody != null)
        request.AddJsonBody(jsonBody);

      if (files != null && files.Length > 0)
      {
        request.AddHeader("Content-Type", "multipart/form-data");

        foreach (var f in files)
        {
          request.AddFile(f.Name, f.FileName, f.ContentType);
        }
      }
      
      request.Timeout = 10 * 60 * 1000;

      var Response = this.Execute(request);

      //Ensure that response code is 200 (OK)
      if (Response.StatusCode == HttpStatusCode.OK || Response.StatusCode == HttpStatusCode.Created)
      {
        return Response;
      }
      else
      {
        switch (Response.StatusCode)
        {
          case HttpStatusCode.Unauthorized:
            throw new Exception("Unauthorized Access");

          case HttpStatusCode.InternalServerError:
            throw new Exception("An error occurred on the server side. Please contact server administrator.");

          case (HttpStatusCode)422:
            var Ex = new Exception("The following errors occurred on the server:");
            foreach (var Err in CreateObjectFromJsonNode<Dictionary<string, string>>(Response, null, false))
            {
              Ex.Data.Add(Err.Key, Err.Value);
            }

            throw Ex;

          default:
            throw new Exception(Response.Content);
        }
      }
    }

    /// <summary>
    /// Executes a REST call using the specified method, parameters and headers. Used by many public functions of this class.
    /// </summary>
    /// <typeparam name="T">Type of returned deserialized object</typeparam>
    /// <typeparam name="U">Json type of target node</typeparam>
    /// <param name="resource">Relative URL of the API resource</param>
    /// <param name="method">HTTP Method</param>
    /// <param name="parameters">parameters including query, header and segment parameters</param>
    /// <returns>Deserialized .NET object created from response JSON.</returns>
    public T ExecuteRest<T>(string resource, Method method, Parameter[] parameters, object jsonBody, FileParameter[] files)
    {
      var Response = ExecuteRest(resource, method, parameters, jsonBody, files);

      if (Response == null)
        return default;
      else
        return this.Deserialize<T>(Response).Data; //If successful, use RestSharp to deserialize JSON into model object
    }

    /// <summary>
    /// Executes a REST call using the specified method, parameters and headers. Used by many public functions of this class. This overload
    /// performs recursive digging of response JSON to find the target node specified by the caller in responseNodeKey.
    /// </summary>
    /// <typeparam name="T">Type of returned deserialized object</typeparam>
    /// <typeparam name="U">Json type of target node</typeparam>
    /// <param name="resource">Relative URL of the API resource</param>
    /// <param name="method">HTTP Method</param>
    /// <param name="parameters">HTTP parameters in case of PUT or POST calls</param>
    /// <param name="headers">HTTP headers that need tobe sent with this request</param>
    /// <param name="segment">Segment values in case resource URL contains segment parameters</param>
    /// <returns>Deserialized .NET object created from response JSON.</returns>
    public T ExecuteRest<T>(string resource, Method method, Parameter[] parameters, object jsonBody,
                              string responseNodeKey, bool isArrayNode, FileParameter[] files)
    {
      var Response = ExecuteRest(resource, method, parameters, jsonBody, files);

      if (Response == null)
        return default;
      else
      {
        if (Response.ContentType == "application/json")
          return CreateObjectFromJsonNode<T>(Response, responseNodeKey, isArrayNode);
        else
          throw new Exception("Content-Type must be 'application/json' to perform deserialization.");
      }
    }

    private T CreateObjectFromJsonNode<T>(IRestResponse Response, string nodeKey, bool isArrayNode)
    {
      //If successful, use RestSharp to deserialize JSON into model object
      var Strategy = new IgnoreNullValuesJsonSerializerStrategy();

      if (nodeKey != null)
      {
        var GenericResponseObject = SimpleJson.DeserializeObject<JsonObject>(Response.Content, Strategy);
        var TargetNodeJson = FindTargetNodeRecursive(GenericResponseObject, nodeKey, isArrayNode).ToString();

        if (TargetNodeJson != null)
        {
          return SimpleJson.DeserializeObject<T>(TargetNodeJson, Strategy);
        }
        else
          return default;
      }
      else
      {
        if (string.IsNullOrEmpty(Response.Content))
          return default;
        else
          return SimpleJson.DeserializeObject<T>(Response.Content, Strategy);
      }
    }

    /// <summary>
    /// Traverses a JsonObject recursively and returns the first Value whose Key is equal to the specified key.
    /// Note: The JSON response returned by the API is often nested in multi-level wrapper nodes that are not necessary for our purpose.
    /// This function digs the response JSON and locates the node that actually contains the data that we're looking for.
    /// </summary>
    /// <param name="current"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private object FindTargetNodeRecursive(JsonObject current, string key, bool isArray)
    {
      var TargetResponseObject = current.Keys.FirstOrDefault(k => k == key);

      if (TargetResponseObject != null)
      {
        if (isArray && current[TargetResponseObject] is JsonArray)
          return (JsonArray)current[TargetResponseObject];
        else
          return (JsonObject)current[TargetResponseObject];
      }
      else
      {
        foreach (var Child in current.Keys)
        {
          var ChildTarget = FindTargetNodeRecursive((JsonObject)current[Child], key, isArray);
          if (ChildTarget != null) return ChildTarget;
        }
      }

      return null;
    }

    /// <summary>
    /// SimpleJson throws exception if incoming JSON contains NULL for an array type property. This strategy is used to
    /// manually handle that situation.
    /// </summary>
    public class IgnoreNullValuesJsonSerializerStrategy : PocoJsonSerializerStrategy
    {
      public override object DeserializeObject(object value, Type type)
      {
        if (value is JsonObject && ((JsonObject)value).Count == 0)
          return null;
        else
        {
          try
          {
            if (type.IsEnum)
            {
              if (value is string s)
                return Enum.Parse(type, s);
              else
                return Enum.ToObject(type, value);
            }
            else if (value is string)
              return value.ToString();
            else if (value is bool)
              return bool.Parse(value.ToString());
            else
              return base.DeserializeObject(value, type);
          }
          catch
          {
            System.Diagnostics.Debugger.Break();
            return null;
          }
        }
      }
    }

    public void Dispose()
    {
      OurWebClient.Dispose();
    }
  }
}