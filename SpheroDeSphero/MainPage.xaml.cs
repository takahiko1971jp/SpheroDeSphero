using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using RobotKit;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace SpheroDeSphero
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Sphero m_controller = null;  // コントローラー用Sphero
        Sphero m_target = null;  // 操縦するSphero

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void tsConnectController_Toggled(object sender, RoutedEventArgs e)
        {
            if(tsConnectController.IsOn)
            {
                tblController.Text = "接続中...";
                RobotProvider provider = RobotProvider.GetSharedProvider();
                provider.DiscoveredRobotEvent += OnControllerDiscovered;
                provider.NoRobotsEvent += OnNoControllerEvent;
                provider.ConnectedRobotEvent += OnControllerConnected;
                provider.FindRobots();
            }
            else
            {
                tblController.Text = "切断中...";

            }
        }

        private void OnControllerDiscovered(object sender, Robot robot)
        {
            if(robot.BluetoothName.Equals(txtController.Text))
            {
                RobotProvider provider = RobotProvider.GetSharedProvider();
                provider.ConnectRobot(robot);
                m_controller = (Sphero)robot;
            }
        }

        private void OnNoControllerEvent(object sender, EventArgs e)
        {
            tblController.Text = "Spheroがペアリングされませんでした。";
        }

        private void OnControllerConnected(object sender, Robot robot)
        {
            tsConnectController.IsOn = true;
            tblController.Text = "接続しました。";

            m_controller.SetRGBLED(0, 255, 0);
            m_controller.SensorControl.Hz = 10;
            m_controller.SensorControl.AccelerometerUpdatedEvent += OnAccelerometerUpdated;
        }

        private void OnAccelerometerUpdated(object sender, AccelerometerReading reading)
        {
            //AccelerometerX.Text = "" + reading.X;
            //AccelerometerY.Text = "" + reading.Y;
            //AccelerometerZ.Text = "" + reading.Z;
            float dx = (-100.0F * reading.X);
            float dy = (100.0F * reading.Y);
            double radian = Math.Atan2(dx, dy);
            int kakudo = (int)(radian * 180.0D / Math.PI);
            if (kakudo < 0) kakudo = 360 + kakudo;
            if (kakudo < 0) kakudo = 0;
            if (kakudo > 359) kakudo = 359;
            float kyori = (float)Math.Sqrt(Math.Pow(reading.X, 2) + Math.Pow(reading.Y, 2));
            if (m_target != null)
            {
                m_target.Roll(kakudo, kyori);
            }
        }

        private void btnController_Click(object sender, RoutedEventArgs e)
        {
            if (m_controller != null)
            {
                m_controller.SensorControl.StopAll();
                m_controller.Sleep();
                m_controller.SensorControl.AccelerometerUpdatedEvent -= OnAccelerometerUpdated;

                RobotProvider provider = RobotProvider.GetSharedProvider();
                provider.DiscoveredRobotEvent -= OnControllerDiscovered;
                provider.NoRobotsEvent -= OnNoControllerEvent;
                provider.ConnectedRobotEvent -= OnControllerConnected;
            }
        }

        private void tsConnectTarget_Toggled(object sender, RoutedEventArgs e)
        {
            if (tsConnectController.IsOn)
            {
                tblTarget.Text = "接続中...";
                RobotProvider provider = RobotProvider.GetSharedProvider();
                provider.DiscoveredRobotEvent += OnTargetDiscovered;
                provider.NoRobotsEvent += OnNoTargetEvent;
                provider.ConnectedRobotEvent += OnTargetConnected;
                provider.FindRobots();
            }
            else
            {
                tblTarget.Text = "切断中...";

            }
        }

        private void OnTargetDiscovered(object sender, Robot robot)
        {
            if (robot.BluetoothName.Equals(txtTarget.Text))
            {
                RobotProvider provider = RobotProvider.GetSharedProvider();
                provider.ConnectRobot(robot);
                m_target = (Sphero)robot;
            }
        }

        private void OnNoTargetEvent(object sender, EventArgs e)
        {
            tblTarget.Text = "Spheroがペアリングされませんでした。";
        }

        private void OnTargetConnected(object sender, Robot robot)
        {
            tsConnectTarget.IsOn = true;
            tblTarget.Text = "接続しました。";
            m_controller.SetRGBLED(255, 255, 255);
        }

        private void btnTarget_Click(object sender, RoutedEventArgs e)
        {
            if (m_target != null)
            {
                m_target.SensorControl.StopAll();
                m_target.Sleep();

                RobotProvider provider = RobotProvider.GetSharedProvider();
                provider.DiscoveredRobotEvent -= OnTargetDiscovered;
                provider.NoRobotsEvent -= OnNoTargetEvent;
                provider.ConnectedRobotEvent -= OnTargetConnected;
            }
        }

        DispatcherTimer timerController = null;
        int angleControllerDegrees = 0;

        void timerControllerTick(object sender, object e)
        {
            angleControllerDegrees += 15;
            angleControllerDegrees %= 360;
            if (m_controller != null)
            {
                m_controller.Roll(angleControllerDegrees, 0);
            }
        }

        private void imgCalibrateController_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (m_controller != null)
            {
                m_controller.SetHeading(0);
                m_controller.SetRGBLED(0, 0, 0);
                m_controller.SetBackLED(1.0f);
            }
            timerController = new DispatcherTimer();
            timerController.Interval = TimeSpan.FromMilliseconds(100d);
            timerController.Tick += timerControllerTick;
            timerController.Start();
        }

        private void imgCalibrateController_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            timerController.Stop();
            if (m_controller != null)
            {
                m_controller.SetHeading(0);
                m_controller.SetRGBLED(0, 255, 0);
                m_controller.SetBackLED(0.0f);
            }
        }

        DispatcherTimer timerTarget = null;
        int angleTargetDegrees = 0;

        void timerTargetTick(object sender, object e)
        {
            angleTargetDegrees += 15;
            angleTargetDegrees %= 360;
            if (m_target != null)
            {
                m_target.Roll(angleTargetDegrees, 0);
            }
        }

        private void imgCalibrateTarget_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (m_target != null)
            {
                m_target.SetHeading(0);
                m_target.SetRGBLED(0, 0, 0);
                m_target.SetBackLED(1.0f);
            }
            timerTarget = new DispatcherTimer();
            timerTarget.Interval = TimeSpan.FromMilliseconds(100d);
            timerTarget.Tick += timerTargetTick;
            timerTarget.Start();
        }

        private void imgCalibrateTarget_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            timerTarget.Stop();
            if (m_target != null)
            {
                m_target.SetHeading(0);
                m_target.SetRGBLED(255, 255, 255);
                m_target.SetBackLED(0.0f);
            }
        }
    }
}
