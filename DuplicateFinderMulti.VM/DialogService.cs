using GalaSoft.MvvmLight.Messaging;
using System;

//namespace GalaSoft.MvvmLight.Messaging
//{
//  public class AppMessenger<TMessage> : Messenger
//  {
//    public void Register<TMessage>(object recipient, Action<TMessage> action, bool keepTargetAlive = false)
//    {

//    }

//    public void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action, bool keepTargetAlive = false);
//    public void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action, bool keepTargetAlive = false);
//    public void Register<TMessage>(object recipient, object token, Action<TMessage> action, bool keepTargetAlive = false);
//  }
//}

namespace AscendasWordAddin.VM
{
  class DialogService : IDialogService
  {
    public void ShowMessage(string msg, bool isError)
    {
      Messenger.Default.Send(new AppMessage<bool>(isError ? AppMessageType.ShowError : AppMessageType.ShowMessage, msg));
    }

    public bool AskBooleanQuestion(string msg)
    {
      bool Result = false;
      Messenger.Default.Send(new AppMessage<bool>(AppMessageType.AskBooleanQuestion, (r) => Result = r, msg));
      return Result;
    }

    public string AskStringQuestion(string msg)
    {
      string Result = null;
      Messenger.Default.Send(new AppMessage<string>(AppMessageType.AskStringQuestion, (r) => Result = r, msg));
      return Result;
    }

    public bool ShowDialog(AppMessageType type)
    {
      bool Result = false;
      Action<bool> Callback = (o) => Result = o;
      Messenger.Default.Send(new AppMessage<bool>(type, callback: Callback));
      return Result;
    }

    public bool ShowDialog(AppMessageType type, object arg)
    {
      bool Result = false;
      Action<bool> Callback = (o) => Result = o;
      Messenger.Default.Send(new AppMessage<bool>(type, callback: Callback, arg: arg));
      
      return Result;
    }
  }
}