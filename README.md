# Xamarin-Yasea
RTMP live streaming client for Xamarin.Android

Yet Another Stream Encoder for Android
======================================

**Xamarin-Yasea** is an Xamarin Android streaming client. It encodes YUV and PCM data from
camera and microphone to H.264/AAC, encapsulates in FLV and transmits over RTMP.

Feature
-------

- [x] Android mini API 16.
- [x] H.264/AAC hard encoding.
- [x] H.264 soft encoding.
- [x] RTMP streaming with state callback handler.
- [x] Portrait and landscape dynamic orientation.
- [x] Front and back cameras hot switch.
- [x] Recording to MP4 while streaming.
- [x] Beautiful filters with GPUImage.
- [x] Acoustic echo cancellation and automatic gain control support.


#### C#
```c#
using Com.Github.Faucamp.Simplertmp;
using Com.Seu.Magicfilter.Utils;
using Net.Ossrs.Yasea;

const string rtmpUrl = "rtmp://xxx.xxx.x.xxx:1935/live/<SteamID>";

Button btnPublish;
SrsPublisher mPublisher;

protected override void OnCreate(Bundle savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    
    mPublisher = new SrsPublisher((SrsCameraView)FindViewById(Resource.Id.glsurfaceview_camera));
    mPublisher.SetEncodeHandler(new SrsEncodeHandler(this));
    mPublisher.SetRtmpHandler(new RtmpHandler(this));
    mPublisher.SetRecordHandler(new SrsRecordHandler(this));
    mPublisher.SetPreviewResolution(640, 360);
    mPublisher.SetOutputResolution(360, 640);
    mPublisher.SetVideoHDMode();
    mPublisher.StartCamera();
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
        btnPublish.Text = "Publish";
    }
}
```

## Credit/Props
 **Xamarin-Yasea is a Xamarin.Android binding library based on Yasea; https://github.com/begeekmyfriend/yasea
