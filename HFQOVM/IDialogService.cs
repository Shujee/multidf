using System.Threading.Tasks;

namespace HFQOVM
{
  public interface IDialogService : Common.IDialogService
  {
    Task<Common.AccessibleMasterFile> ShowExamsListDialog();
  }
}