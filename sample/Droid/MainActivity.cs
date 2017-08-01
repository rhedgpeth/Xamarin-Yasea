using Android.App;
using Android.Widget;
using Android.OS;

using Com.Github.Faucamp.Simplertmp;
using Com.Seu.Magicfilter.Utils;
using Net.Ossrs.Yasea;

using Android.Content;
using System;
using Android.Hardware;
using Android.Hardware.Camera2;
using Java.IO;
using Java.Net;
using Java.Lang;

namespace Sample.Droid
{
    [Activity(Label = "LiveStreamTester", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity, RtmpHandler.IRtmpListener, SrsRecordHandler.ISrsRecordListener, SrsEncodeHandler.ISrsEncodeListener
    {
        const string TAG = "Yasea";

        Button btnPublish;
        Button btnSwitchCamera;
        Button btnRecord;
        Button btnSwitchEncoder;

	    // TODO: Replace with your RTMP URL
		const string rtmpUrl = "rtmp://xxx.xxx.x.xxx:1935/live/<SteamID>";

        // TODO: Set up local storage/recording
        string recPath = "";

        SrsPublisher mPublisher;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window.AddFlags(Android.Views.WindowManagerFlags.KeepScreenOn);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

            // Response screen rotation event
            RequestedOrientation = Android.Content.PM.ScreenOrientation.FullSensor;

            btnPublish = (Button)FindViewById(Resource.Id.publish);
            btnSwitchCamera = (Button)FindViewById(Resource.Id.swCam);
            btnRecord = (Button)FindViewById(Resource.Id.record);
            btnSwitchEncoder = (Button)FindViewById(Resource.Id.swEnc);

			mPublisher = new SrsPublisher((SrsCameraView)FindViewById(Resource.Id.glsurfaceview_camera));
			mPublisher.SetEncodeHandler(new SrsEncodeHandler(this));
			mPublisher.SetRtmpHandler(new RtmpHandler(this));
			mPublisher.SetRecordHandler(new SrsRecordHandler(this));
			mPublisher.SetPreviewResolution(640, 360);
			mPublisher.SetOutputResolution(360, 640);
			mPublisher.SetVideoHDMode();
			mPublisher.StartCamera();

            btnPublish.Click += OnPublishClick;
            btnSwitchCamera.Click += OnCameraSwitch;
            btnRecord.Click += OnRecordClick;
        }

        protected void OnPublishClick(object sender, EventArgs e)
        {
            if (btnPublish.Text == "Publish")
            {
                mPublisher.StartPublish(rtmpUrl);
                mPublisher.StartCamera();

                btnPublish.Text = "Stop";
            }
            else if (btnPublish.Text == "Stop")
            {
                mPublisher.StopPublish();
                //mPublisher.StopRecord();

                btnPublish.Text = "Publish";
                //btnRecord.Text = "Record";
            }
        }

        protected void OnCameraSwitch(object sender, EventArgs e)
        {
			var manager = (CameraManager)GetSystemService(CameraService);
            mPublisher.SwitchCameraFace((mPublisher.CamraId + 1) % manager.GetCameraIdList().Length);
        }

        protected void OnRecordClick(object sender, EventArgs e)
        {
            if (btnRecord.Text == "Record")
            {
                if (mPublisher.StartRecord(recPath))
                {
                    btnRecord.Text = "Pause";
                }
            }
            else if (btnRecord.Text == "Pause")
            {
                mPublisher.PauseRecord();
                btnRecord.Text = "Resume";
            }
            else if (btnRecord.Text == "Resume")
            {
                mPublisher.ResumeRecord();
                btnRecord.Text = "Pause";
            }
        }

        protected void OnEncoderSwitch(object sender, EventArgs e)
        {
            // TODO: Possibly hook this up later
        }

        protected override void OnResume()
        {
            base.OnResume();

            btnPublish.Enabled = true;
            //mPublisher.ResumeRecord();
        }

        protected override void OnPause()
        {
            base.OnPause();

            //mPublisher.PauseRecord();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

			btnPublish.Click -= OnPublishClick;
			btnSwitchCamera.Click -= OnCameraSwitch;
			btnRecord.Click -= OnRecordClick;

            mPublisher.StopPublish();
            //mPublisher.StopRecord();
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            mPublisher.StopEncode();

            //mPublisher.StopRecord();
            //btnRecord.Text = "Record";

            mPublisher.SetScreenOrientation((int)newConfig.Orientation);

            if (btnPublish.Text == "Stop")
            {
                mPublisher.StartEncode();
            }

            mPublisher.StartCamera();
        }

		void ShowShortToast(string message)
		{
			Toast.MakeText(this, message, ToastLength.Short).Show();
		}

        void HandleException(Java.Lang.Exception e)
        {
            try
            {
                ShowShortToast(e.Message);

				mPublisher.StopPublish();
				mPublisher.StopRecord();

                btnPublish.Text = "Publish";
                btnRecord.Text = "Record";
                btnSwitchEncoder.Enabled = true;
            }
            catch
            { }
        }

        public void OnRtmpConnecting(string message) => ShowShortToast(message);

        public void OnRtmpConnected(string message) => ShowShortToast(message);

        public void OnRtmpVideoStreaming() { }

        public void OnRtmpAudioStreaming() { }

        public void OnRtmpStopped() => ShowShortToast("Stopped");

        public void OnRtmpDisconnected() => ShowShortToast("Disconnected");

        public void OnRtmpVideoFpsChanged(double fps)
        {
            System.Console.WriteLine($"Output Fps: {fps}");
        }

        public void OnRtmpVideoBitrateChanged(double bitrate)
        {
            int rate = (int)bitrate;

            if ((rate / 1000) > 0)
                System.Console.WriteLine($"Audio bitrate: {bitrate / 1000} kbps");
            else
                System.Console.WriteLine($"Audio bitrate: {rate} bps");
        }

		public void OnRtmpAudioBitrateChanged(double bitrate)
		{
			int rate = (int)bitrate;

			if ((rate / 1000) > 0)
				System.Console.WriteLine($"Video bitrate: {bitrate / 1000} kbps");
			else
				System.Console.WriteLine($"Video bitrate: {rate} bps");
		}

        public void OnRtmpSocketException(SocketException e) => HandleException(e);

        public void OnRtmpIOException(IOException e) => HandleException(e);

		public void OnRtmpIllegalArgumentException(IllegalArgumentException e) => HandleException(e);

		public void OnRtmpIllegalStateException(IllegalStateException e) => HandleException(e);

        public void OnRecordPause() => ShowShortToast("Record paused");

	    public void OnRecordResume() => ShowShortToast("Record resumed");

	    public void OnRecordStarted(string msg) => ShowShortToast("Recording file: " + msg);

        public void OnRecordFinished(string msg) => ShowShortToast("MP4 file saved: " + msg);

	    public void OnRecordIOException(IOException e) => HandleException(e);

	    public void OnRecordIllegalArgumentException(IllegalArgumentException e) => HandleException(e);

        // Implementation of SrsEncodeHandler

        public void OnNetworkWeak() => ShowShortToast("Network weak");

        public void OnNetworkResume() => ShowShortToast("Network resume");

	    public void OnEncodeIllegalArgumentException(IllegalArgumentException e) => HandleException(e);
    }
}

