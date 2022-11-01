using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ServicesManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private TaskbarIcon tray;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        tray = (TaskbarIcon)FindResource("NotifyIcon");
        tray.Icon = System.Drawing.SystemIcons.Exclamation;
        tray.TrayRightMouseDown += BeforeShowContextMenuEvent;
        SetMenu();
    }
    
    protected override void OnExit(ExitEventArgs e)
    {
        tray.Dispose();
        base.OnExit(e);
    }

    private List<string> GetServices()
    {
        string listOfServices = System.IO.File.ReadAllText(@"RequestedServices.txt");
        return listOfServices.Split(';').ToList();
    }

    private void SetMenu()
    {
        tray.ContextMenu = new ContextMenu();
        var requestedServices = GetServices();
        var selectedServices = ServiceController.GetServices().Where(x => requestedServices.Contains(x.ServiceName)).ToList();

        foreach (var selectedService in selectedServices)
        {
            var item = new MenuItem()
            {
                Header = $"{selectedService.ServiceName} | {selectedService.Status.ToString()}",
                Background = selectedService.Status == ServiceControllerStatus.Running
                    ? new SolidColorBrush(Colors.GreenYellow)
                    : new SolidColorBrush(Colors.Red),

            };
            item.Command = StartStopService(selectedService);
            tray.ContextMenu.Items.Add(item);
        }
    }

    private ICommand StartStopService(ServiceController service)
    {

        return new DelegateCommand()
        {
            CommandAction = () =>
            {
                if (service.Status == ServiceControllerStatus.Stopped ||
                    service.Status == ServiceControllerStatus.Running)
                {
                    if (service.Status == ServiceControllerStatus.Running) service.Stop();
                    else service.Start();
                    SetMenu();
                }
            }
        };
    }

    private void BeforeShowContextMenuEvent(object sender, RoutedEventArgs e)
    {
        SetMenu();
    }
}