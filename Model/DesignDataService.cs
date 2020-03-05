using Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
  public class DesignDataService : IDataService
  {
    private string _BearerToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImp0aSI6IjE0MGQyZTY2YzhiNDU1M2U0ODcyZmVmNjk0MmUyMjQ3NDRiMDg4NWViZDE0MGY1ODFkMDg1MGFmNmQzZTk5ZTMzZGYyNTFjNjVlYmQyYTgzIn0.eyJhdWQiOiIyIiwianRpIjoiMTQwZDJlNjZjOGI0NTUzZTQ4NzJmZWY2OTQyZTIyNDc0NGIwODg1ZWJkMTQwZjU4MWQwODUwYWY2ZDNlOTllMzNkZjI1MWM2NWViZDJhODMiLCJpYXQiOjE1NzYxNzQ1MTIsIm5iZiI6MTU3NjE3NDUxMiwiZXhwIjoxNjA3Nzk2OTExLCJzdWIiOiIxIiwic2NvcGVzIjpbXX0.rHlmdoT4COu8q0i_QB_f5nRFVprWs2BWF7GdHS1yqlfO0zsaLZ0oIAfRbxl2ZtMo40taSxlNU8nrfXM6hc2oaf6ylaUiEAzCTrRexuOzO1vHaORFCsLrmWxMd4TKIrOWONP1Ns73f4PzP1cgB60aLFZKzXiFDHjUqNRd5BjcZVtaFf_PYmmQOS8k2TEdLIqcdaRaY7ZbFXaREMuexFp9ZB6sMz-K5ggYgwR-JKZThEWb2hvtuBBwsqaUjpNmsmRJvHSzVKpn3cQxpR_BDY6j6rF5reqh_jAbKIuVUJKYQ-Nu4eJGS84SGeS8PcmNfZYSSprPhqZUFj0307EVcFljWh2NmV2rzS7pDwaJkn7eKjMhsUo2PXpo0ZkNPIeLEJiWx1g4Cfe_ZXSmmy6XmPJQcJT3KuTU26KG2Bv285f6e52FdBzH76lb990VK2MHeS55bYcMgO1cKwY2Vpy_pHV8F0UTAQci4zxZCb71VN3azsPooJRDSR0IED1K2umYhEWUjWajJlJa2H_iEfqmASjD-AXJmK4jam2z8TKepSPE0SfT6I_58nKevxD8Tve7okjnd2utdh95xXXUx_nqeQ4R8CFjpD6PxL1wqBRZyPaDh0aoXQX7bztp0I-HLv6WQCGaTVHHQwM2xLfp6URiT-3VdB8UboJ_orLUXN3oaa_oyBg";

    public MasterFile DownloadExam(int access_id, string machine_name)
    {
      throw new System.NotImplementedException();
    }

    public Task<bool> ExamNumberExists(string number)
    {
      return Task.FromResult(false);
    }

    public string GetClientCity()
    {
      return "Karachi";
    }

    public string GetClientCountry()
    {
      return "Pakistan";
    }

    public Task<AccessibleMasterFile[]> GetExamsDL()
    {
      throw new System.NotImplementedException();
    }

    public Task<MasterFile[]> GetExamsUL()
    {
      throw new System.NotImplementedException();
    }

    public bool IsAlive()
    {
      return true;
    }

    public Task<User> Login(string email, string password)
    {
      return Task.FromResult<User>(new User() { name = "Test User", type = UserType.Admin });
    }

    public Task Logout()
    {
      throw new System.NotImplementedException();
    }

    public bool UpdateExamFiles(string xpsPath, string xmlPath, int exam_id, int qa_count, string qa_json, string remarks, string origfilename)
    {
      throw new System.NotImplementedException();
    }

    public bool UploadExam(string xpsPath, string xmlPath, string exam_number, string exam_name, string description, int qa_count, string qa_json, string origfilename)
    {
      throw new System.NotImplementedException();
    }

    public Task<bool> UploadResult(int exam_id, string machine_name, IEnumerable<HFQResultRow> result)
    {
      throw new System.NotImplementedException();
    }

    public Task<bool> UploadResult(int exam_id, string machine_name, string city, string country, IEnumerable<HFQResultRow> result)
    {
      throw new System.NotImplementedException();
    }

    public Task<bool> UploadSnapshot(int download_id, DateTime timestamp, string filename)
    {
      throw new NotImplementedException();
    }
  }
}