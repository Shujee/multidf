using GalaSoft.MvvmLight.Ioc;
using HFQOVM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VMBase;

namespace HFQAppTest
{
  [TestClass]
  public class HFQAppTests
  {
    private TestContext testContextInstance;

    /// <summary>
    /// Gets or sets the test context which provides
    /// information about and functionality for the current test run.
    /// This property is automatically assigned by unit test engine.
    /// </summary>
    public TestContext TestContext
    {
      get { return testContextInstance; }
      set { testContextInstance = value; }
    }

    public HFQAppTests()
    {
      ViewModelLocator.Auth.Email = "	down@hfqserver.com";
      ViewModelLocator.Auth.Password = "12345678";

      SimpleIoc.Default.Unregister<IRegistrationService>();
      SimpleIoc.Default.Register<IRegistrationService, TestRegistrationService>();

      SimpleIoc.Default.Register<VMBase.ILogger>(() => new TestLogger(testContextInstance));

      SimpleIoc.Default.Register<TestDialogService>();
      SimpleIoc.Default.Register<Common.IDialogService>(() => SimpleIoc.Default.GetInstance<TestDialogService>());
      SimpleIoc.Default.Register<HFQOVM.IDialogService>(() => SimpleIoc.Default.GetInstance<TestDialogService>());

      SimpleIoc.Default.Register<IApplicationService, TestApplicationService>();
    }

    [TestMethod]
    public async Task TestMethod1()
    {
      await ViewModelLocator.Auth.LoginCommand.ExecuteAsync();

      //var SelectedExam = await ViewModelLocator.HFQ.RefreshExamsList();
      ViewModelLocator.HFQ.SelectedAccess = new Common.AccessibleMasterFile(); //ViewModelLocator.HFQ.MyExams[0];

      await ViewModelLocator.HFQ.OpenExamCommand.ExecuteAsync();

      ViewModelLocator.HFQ.UploadResultCommand.CanExecuteChanged += (sender, e) =>
      {
        if(!ViewModelLocator.HFQ.UploadResultCommand.CanExecute(null))
          Assert.Fail("UploadResultCommand has changed to false.");
      };

      //override default camera service
      SimpleIoc.Default.Unregister<ICameraService>();
      SimpleIoc.Default.Register<ICameraService, TestCameraService>();

      System.Random rnd = new System.Random();
      for (int i = 0; i < ViewModelLocator.HFQ.XMLDoc.QAs.Count; i++)
      {
        var n = rnd.Next(0, ViewModelLocator.HFQ.XMLDoc.QAs.Count);

        ViewModelLocator.HFQ.MarkCommand.Execute(ViewModelLocator.HFQ.XMLDoc.QAs[n]);
        ViewModelLocator.HFQ.GoToNextCommand.Execute(null);

        Task.Delay(500).Wait();

        //if (i % 3 == 0)
        //  (ViewModelLocator.CameraService as TestCameraService).IsCameraShutdown = true;
        //else
        //  (ViewModelLocator.CameraService as TestCameraService).IsCameraShutdown = false;
      }
    }
  }
}
