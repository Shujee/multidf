using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HFQOModel
{
  public class HFQODataService : IDataService
  {
    private RESTWrapper REST;
    private string _BearerToken = null;

    public HFQODataService()
    {
      REST = new RESTWrapper(HFQOModel.Properties.Settings.Default.ServerURL, 0);
    }

    public async Task<bool> Login(string email, string password)
    {
      var Response = await Task.Run(() => REST.ExecuteRest<LoginToken>("login", Method.POST,
                 new[]
                 {
                      new RESTParameter(){ name = "email", value = email, type = ParameterType.GetOrPost },
                      new RESTParameter(){ name = "password", value = password, type = ParameterType.GetOrPost },
                 },
                 null,
                 "token",  //json response is wrapped in "token" node
                 false,
                 null));
      
      //Check if credentials are correct
      if (Response == null)
        return false;
      else
      {
        _BearerToken = Response.access_token;
        return true;
      }
    }

    public async Task Logout()
    {
      var Response = await Task.Run(() =>
                REST.ExecuteRest<string>("logout", Method.POST,
                                        new[]
                                        {
                                            new RESTParameter(){ name = "Authorization", value = "Bearer " + _BearerToken, type = ParameterType.HttpHeader},
                                            new RESTParameter(){ name = "Accept", value = "application/json", type = ParameterType.HttpHeader},
                                        },
                                        null,
                                        null,
                                        false,
                                        null));

      _BearerToken = null;
    }

    public bool UploadExam(string xpsPath, string xmlPath, string exam_name, int qa_count)
    {
      var Response = REST.ExecuteRest<string>("exam", Method.POST,
                              new[]
                              {
                                  new RESTParameter(){ name = "Authorization", value = "Bearer " + _BearerToken, type = ParameterType.HttpHeader},
                                  new RESTParameter(){ name = "Accept", value = "application/json", type = ParameterType.HttpHeader},
                                  new RESTParameter(){ name = "name", value = exam_name, type = ParameterType.GetOrPost },
                                  new RESTParameter(){ name = "qa_count", value = qa_count.ToString(), type = ParameterType.GetOrPost },
                              },
                              null,
                              null,
                              false,
                              new FileParameter[] {
                                new FileParameter() { Name = "xps_content", FileName = xpsPath, ContentType="application/octet-stream" },
                                new FileParameter() { Name = "xml_content", FileName = xmlPath, ContentType="application/octet-stream" },
                              });

      //Check if credentials are correct
      return Response != null;
    }

    public bool UpdateExamFiles(string xpsPath, string xmlPath, int exam_id, int qa_count)
    {
      var Response = REST.ExecuteRest<string>("exam/{exam}/update_files", Method.POST,
                              new[]
                              {
                                  new RESTParameter(){ name = "Authorization", value = "Bearer " + _BearerToken, type = ParameterType.HttpHeader},
                                  new RESTParameter(){ name = "Accept", value = "application/json", type = ParameterType.HttpHeader},
                                  new RESTParameter(){ name = "exam", value = exam_id.ToString(), type = ParameterType.UrlSegment},
                                  new RESTParameter(){ name = "qa_count", value = qa_count.ToString(), type = ParameterType.GetOrPost },
                              },
                              null,
                              null,
                              false,
                              new FileParameter[] {
                                new FileParameter() { Name = "xps_content", FileName = xpsPath, ContentType="application/octet-stream" },
                                new FileParameter() { Name = "xml_content", FileName = xmlPath, ContentType="application/octet-stream" },
                              });

      //Check if credentials are correct
      return Response != null;
    }

    public async Task<Dictionary<string, string>> GetExamsDL()
    {
      return await Task.Run(() => REST.ExecuteRest<Dictionary<string, string>>("user/myexams/dl", Method.GET,
                              new[]
                              {
                                  new RESTParameter(){ name = "Authorization", value = "Bearer " + _BearerToken, type = ParameterType.HttpHeader},
                                  new RESTParameter(){ name = "Accept", value = "application/json", type = ParameterType.HttpHeader},
                              },
                              null,
                              null,
                              true,
                              null));
    }

    public async Task<Dictionary<string, string>> GetExamsUL()
    {
      return await Task.Run(() => REST.ExecuteRest<Dictionary<string, string>>("user/myexams/ul", Method.GET,
                              new[]
                              {
                                  new RESTParameter(){ name = "Authorization", value = "Bearer " + _BearerToken, type = ParameterType.HttpHeader},
                                  new RESTParameter(){ name = "Accept", value = "application/json", type = ParameterType.HttpHeader},
                              },
                              null,
                              null,
                              true,
                              null));
    }

    public MasterFile DownloadExam(int access_id, string machine_name)
    {
      return REST.ExecuteRest<MasterFile>("access/{access}/download", Method.GET,
                              new[]
                              {
                                  new RESTParameter(){ name = "Authorization", value = "Bearer " + _BearerToken, type = ParameterType.HttpHeader},
                                  new RESTParameter(){ name = "Accept", value = "application/json", type = ParameterType.HttpHeader},
                                  new RESTParameter(){ name = "machine_name", value = machine_name, type = ParameterType.GetOrPost },
                                  new RESTParameter(){ name = "access", value = access_id.ToString(), type = ParameterType.UrlSegment},
                              },
                              null,
                              null,
                              false,
                              null);
    }

    public async Task<bool> UploadResult(int exam_id, string machine_name, IEnumerable<HFQResultRow> result)
    {
      return await Task.Run(() => REST.ExecuteRest<string>("/exam/{exam}/upload_result", Method.POST,
                              new[]
                              {
                                  new RESTParameter(){ name = "Authorization", value = "Bearer " + _BearerToken, type = ParameterType.HttpHeader},
                                  new RESTParameter(){ name = "Accept", value = "application/json", type = ParameterType.HttpHeader},
                                  new RESTParameter(){ name = "exam", value = exam_id.ToString(), type = ParameterType.UrlSegment},
                                  new RESTParameter(){ name = "machine_name", value = machine_name, type = ParameterType.GetOrPost },
                                  new RESTParameter(){ name = "result", value = SimpleJson.SerializeObject(result), type = ParameterType.GetOrPost },
                              },
                              null,
                              null,
                              false,
                              null)).ContinueWith(t => t.IsCompleted && !t.IsFaulted);
    }
  }
}