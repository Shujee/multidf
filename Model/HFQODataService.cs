using Common;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using System.Net.NetworkInformation;
using System;

namespace Model
{
  public class HFQODataService : IDataService
  {
    private RESTWrapper REST;
    private string _BearerToken = null;

    public HFQODataService()
    {
      REST = new RESTWrapper(Model.Properties.Settings.Default.ServerURL, 0);
    }

    public bool IsAlive()
    {
      Uri myUri = new Uri(Model.Properties.Settings.Default.ServerURL);
      string host = myUri.Host;

      using (var ping = new Ping())
      {
        try
        {
          var reply = ping.Send(host, 3 * 1000); // 3 seconds time out
          return reply.Status == IPStatus.Success;
        }
        catch
        {
          return false;
        }
      }
    }

    public async Task<User> Login(string email, string password)
    {
      try
      {
      var Response = await Task.Run(() => REST.ExecuteRest<User>("login", Method.POST,
                 new[]
                 {
                      new Parameter(){ Name = "email", Value = email, Type = ParameterType.GetOrPost },
                      new Parameter(){ Name = "password", Value = password, Type = ParameterType.GetOrPost },
                 },
                 null,
                 null,  //json response is wrapped in "token" node
                 false,
                 null));

        //Check if credentials are correct
        if (Response == null)
          return null;
        else
        {
          _BearerToken = Response.token.access_token;
          return Response;
        }
      }
      catch
      {
        return null;
      }
    }

    public async Task Logout()
    {
      var Response = await Task.Run(() =>
                REST.ExecuteRest<string>("logout", Method.POST,
                                        new[]
                                        {
                                            new Parameter(){ Name = "Authorization", Value = "Bearer " + _BearerToken, Type = ParameterType.HttpHeader},
                                            new Parameter(){ Name = "Accept", Value = "application/json", Type = ParameterType.HttpHeader},
                                        },
                                        null,
                                        null,
                                        false,
                                        null));

      _BearerToken = null;
    }

    public bool UploadExam(string xpsPath, string xmlPath, string exam_number, string exam_name, string description, int qa_count, string qa_json, string origfilename)
    {
      var Response = REST.ExecuteRest<MasterFile>("exam", Method.POST,
                              new[]
                              {
                                  new Parameter(){ Name = "Authorization", Value = "Bearer " + _BearerToken, Type = ParameterType.HttpHeader},
                                  new Parameter(){ Name = "Accept", Value = "application/json", Type = ParameterType.HttpHeader},
                                  new Parameter(){ Name = "number", Value = exam_number, Type = ParameterType.GetOrPost },
                                  new Parameter(){ Name = "name", Value = exam_name, Type = ParameterType.GetOrPost },
                                  new Parameter(){ Name = "description", Value = description, Type = ParameterType.GetOrPost },
                                  new Parameter(){ Name = "qa_count", Value = qa_count.ToString(), Type = ParameterType.GetOrPost },
                                  new Parameter(){ Name = "qas", Value = qa_json, Type = ParameterType.GetOrPost },
                                  new Parameter(){ Name = "origfilename", Value = origfilename, Type = ParameterType.GetOrPost },
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

    public bool UpdateExamFiles(string xpsPath, string xmlPath, int exam_id, int qa_count, string qa_json, string remarks, string origfilename)
    {
      var Response = REST.ExecuteRest<bool>("exam/{exam}/update_files", Method.POST,
                              new[]
                              {
                                  new Parameter(){ Name = "Authorization", Value = "Bearer " + _BearerToken, Type = ParameterType.HttpHeader},
                                  new Parameter(){ Name = "Accept", Value = "application/json", Type = ParameterType.HttpHeader},

                                  new Parameter(){ Name = "exam", Value = exam_id.ToString(), Type = ParameterType.UrlSegment},

                                  new Parameter(){ Name = "qa_count", Value = qa_count.ToString(), Type = ParameterType.GetOrPost },
                                  new Parameter(){ Name = "qas", Value = qa_json, Type = ParameterType.GetOrPost },
                                  new Parameter(){ Name = "remarks", Value = remarks, Type = ParameterType.GetOrPost },
                                  new Parameter(){ Name = "origfilename", Value = origfilename, Type = ParameterType.GetOrPost },
                              },
                              null,
                              null,
                              false,
                              new FileParameter[] {
                                new FileParameter() { Name = "xps_content", FileName = xpsPath, ContentType="application/octet-stream" },
                                new FileParameter() { Name = "xml_content", FileName = xmlPath, ContentType="application/octet-stream" },
                              });

      //Check if credentials are correct
      return Response;
    }

    public async Task<AccessibleMasterFile[]> GetExamsDL()
    {
      return await Task.Run(() => REST.ExecuteRest<AccessibleMasterFile[]>("user/myexams/dl", Method.GET,
                              new[]
                              {
                                  new Parameter(){ Name = "Authorization", Value = "Bearer " + _BearerToken, Type = ParameterType.HttpHeader},
                                  new Parameter(){ Name = "Accept", Value = "application/json", Type = ParameterType.HttpHeader},
                              },
                              null,
                              "data",
                              true,
                              null));
    }

    public async Task<MasterFile[]> GetExamsUL()
    {
      return await Task.Run(() => REST.ExecuteRest<MasterFile[]>("user/myexams/ul", Method.GET,
                              new[]
                              {
                                  new Parameter(){ Name = "Authorization", Value = "Bearer " + _BearerToken, Type = ParameterType.HttpHeader},
                                  new Parameter(){ Name = "Accept", Value = "application/json", Type = ParameterType.HttpHeader},
                              },
                              null,
                              null,
                              true,
                              null));
    }

    public MasterFile DownloadExam(int access_id, string machine_name)
    {
      return REST.ExecuteRest<MasterFile>("access/{access}/download", Method.POST,
                              new[]
                              {
                                  new Parameter(){ Name = "Authorization", Value = "Bearer " + _BearerToken, Type = ParameterType.HttpHeader},
                                  new Parameter(){ Name = "Accept", Value = "application/json", Type = ParameterType.HttpHeader},
                                  new Parameter(){ Name = "machine_name", Value = machine_name, Type = ParameterType.GetOrPost },
                                  new Parameter(){ Name = "access", Value = access_id.ToString(), Type = ParameterType.UrlSegment},
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
                                  new Parameter(){ Name = "Authorization", Value = "Bearer " + _BearerToken, Type = ParameterType.HttpHeader},
                                  new Parameter(){ Name = "Accept", Value = "application/json", Type = ParameterType.HttpHeader},
                                  new Parameter(){ Name = "exam", Value = exam_id.ToString(), Type = ParameterType.UrlSegment},
                                  new Parameter(){ Name = "machine_name", Value = machine_name, Type = ParameterType.GetOrPost },
                                  new Parameter(){ Name = "result", Value = SimpleJson.SerializeObject(result), Type = ParameterType.GetOrPost },
                              },
                              null,
                              null,
                              false,
                              null)).ContinueWith(t =>
                              {
                                if (t.IsFaulted)
                                  throw t.Exception.InnerException;
                                else
                                  return t.IsCompleted && !t.IsFaulted;
                              });
    }

    public async Task<bool> UploadSnapshot(int download_id, DateTime timestamp, string filename)
    {
      return await Task.Run(() => REST.ExecuteRest<string>("/download/{download}/snapshot", Method.POST,
                              new[]
                              {
                                  new Parameter(){ Name = "Authorization", Value = "Bearer " + _BearerToken, Type = ParameterType.HttpHeader},
                                  new Parameter(){ Name = "Accept", Value = "application/json", Type = ParameterType.HttpHeader},
                                  new Parameter(){ Name = "download", Value = download_id.ToString(), Type = ParameterType.UrlSegment},
                                  new Parameter(){ Name = "timestamp", Value = timestamp.ToString("yyyy-MM-dd HH:mm:ss"), Type = ParameterType.GetOrPost },
                              },
                              null,
                              null,
                              false,
                              new FileParameter[] {
                                new FileParameter() { Name = "image_file", FileName = filename, ContentType="image/jpeg" },
                              })).ContinueWith(t =>
                              {
                                if (!t.IsCompleted || t.IsFaulted)
                                  throw t.Exception.InnerException;
                                else
                                  return t.IsCompleted && !t.IsFaulted;
                              });
    }

    public async Task<bool> ExamNumberExists(string number)
    {
      return await Task.Run(() => REST.ExecuteRest<bool>("exam_number_exists/{number}", Method.GET,
                                   new[]
                                   {
                                  new Parameter(){ Name = "Authorization", Value = "Bearer " + _BearerToken, Type = ParameterType.HttpHeader},
                                  new Parameter(){ Name = "Accept", Value = "application/json", Type = ParameterType.HttpHeader},
                                  new Parameter(){ Name = "number", Value = number, Type = ParameterType.UrlSegment},
                                   },
                                   null,
                                   null,
                                   false,
                                   null));
    }
  }
}