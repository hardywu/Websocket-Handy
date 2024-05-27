using ReactiveUI;
using System;
using System.Reactive;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebsocketHandy.ViewModels;


public class Laputa : WebSocketBehavior
{
    public event EventHandler<string>? LogHdl;

    protected override void OnMessage(MessageEventArgs e)
    {
        LogHdl?.Invoke(this, e.Data);
    }

    protected override void OnOpen()
    {
        LogHdl?.Invoke(this, "new user");
    }

    protected override void OnClose(CloseEventArgs e)
    {
        LogHdl?.Invoke(this, "a user left");
    }

    public Laputa(EventHandler<string> logCb)
    {
        LogHdl += logCb;
    }
}

public class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
#pragma warning restore CA1822 // Mark members as static

    private string _Logs = "";
    public string Logs { get => _Logs; set => this.RaiseAndSetIfChanged(ref _Logs, value); }
    private string _Msg = "";
    public string Msg { get => _Msg; set => this.RaiseAndSetIfChanged(ref _Msg, value); }
    private string _Port = "34545";
    public string Port { get => _Port; set => this.RaiseAndSetIfChanged(ref _Port, value); }
    public bool Running { get => _wssv is not null; }

    public WebSocketServer? _wssv;

    public ReactiveCommand<Unit, Unit> SendMsg { get; }
    private void SendMsgTask()
    {
        Logs += Msg;
        _wssv?.WebSocketServices.Broadcast(Msg);
        Msg = "";
    }

    public void AddLog(object? sender, string msg)
    {
        Logs += $"{msg}\n";
    }

    public string StartBtnLab { get => Running ? "Stop" : "Start"; }
    public ReactiveCommand<Unit, Unit> StartSrv { get; }
    private void StartSrvTask()
    {
        if (_wssv is null)
        {
            _wssv = new WebSocketServer($"ws://localhost:{Port}");
            _wssv.AddWebSocketService<Laputa>("/", () => new Laputa(AddLog));
            _wssv.Start();
            Logs += $"start server at: ws://localhost:{Port}/\n";
        }
        else
        {
            _wssv.Stop();
            _wssv = null;
            Logs += $"Stop server\n";
        }
        this.RaisePropertyChanged(nameof(StartBtnLab));
        this.RaisePropertyChanged(nameof(Running));
    }

    public MainWindowViewModel()
    {
        SendMsg = ReactiveCommand.Create(SendMsgTask);
        StartSrv = ReactiveCommand.Create(StartSrvTask);
    }
}
