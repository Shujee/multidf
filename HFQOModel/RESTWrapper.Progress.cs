using RestSharp;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HFQOModel
{
  /// <summary>
  /// RESTSharp does not report upload progress during POST calls. We're going to use the good old WebClient for this purpose.
  /// </summary>
  partial class RESTWrapper : IUpload
  {
    HttpClient OurWebClient;

    public event Action<UploadProgressEventArgs> ProgressChanged;
    public event Action<string> PostCompleted;

    public async Task<T> PostRequestWithProgress<T>(string resource, Parameter[] parameters, object jsonBody,
                              string responseNodeKey, bool isArrayNode)
    {
      // adding necessary Headers and parameters
      bool InvalidParamFlag = false;

      using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, this.BaseUrl + resource.TrimStart('/')))
      {
        foreach (var Param in parameters)
        {
          switch (Param.Type)
          {
            case ParameterType.HttpHeader:
              requestMessage.Headers.Add(Param.Name, Param.Value.ToString());
              break;
            default:
              InvalidParamFlag = true;
              break;
          }

          if (InvalidParamFlag)
            throw new Exception($"Parameters of type {Param.Type} are not supported.");
        }

        var BodyBuffer = Encoding.UTF8.GetBytes(SimpleJson.SerializeObject(jsonBody) ?? "");
        var BodyStream = new MemoryStream(BodyBuffer);
        this.Filesize = BodyBuffer.Length;
        requestMessage.Content = new ProgressableStreamContent(BodyStream, this);

        var Resp = await OurWebClient.SendAsync(requestMessage);
        var RespContent = await Resp.Content.ReadAsStringAsync();

        RestResponse response = new RestResponse();
        return this.CreateObjectFromJsonNode<T>(new RestResponse() { Content = RespContent }, responseNodeKey, isArrayNode);
      }
    }

    private void UploadCompletedCallback(object sender, UploadFileCompletedEventArgs e)
    {
      if (e.Error != null)
        PostCompleted?.Invoke(e.Error.ToString());
      else
        PostCompleted?.Invoke(Encoding.UTF8.GetString(e.Result));
    }

    public long Filesize { get; set; }

    public void ReportProgress(int progress)
    {
      
      ProgressChanged?.Invoke(new UploadProgressEventArgs(progress));
    }
  }
}
