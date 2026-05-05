namespace StreamingPlatform.Bridge
{
    // ============================================================
    // BRIDGE PATTERN
    // ============================================================
    // Problema: Platforma StreamZone trebuie să redea mai multe tipuri
    // de conținut media pe mai multe tipuri de dispozitive:
    //
    //   Tipuri media:  Video (film/serial) | Audio (podcast) | Live Stream
    //   Dispozitive:   Mobile | Desktop | Smart TV | Tabletă
    //
    // Abordare GREȘITĂ: 3 tipuri × 4 dispozitive = 12 clase:
    //   MobileVideoPlayer, MobileAudioPlayer, MobileLivePlayer,
    //   DesktopVideoPlayer, DesktopAudioPlayer, DesktopLivePlayer...
    //
    //   Adaugi dispozitiv nou → +3 clase. Adaugi tip media nou → +4 clase.
    //
    // Soluția — Bridge:
    //   Separă ABSTRACȚIA (tipul de media) de IMPLEMENTAREA (dispozitivul).
    //   Conectează-le printr-o referință (podul/bridge-ul).
    //
    //   Adaugi dispozitiv nou → 1 clasă (implementare).
    //   Adaugi tip media nou  → 1 clasă (abstracție).
    //   3 + 4 = 7 clase în loc de 12. Cu scalare: avantajul crește exponențial.
    //
    // Structura:
    //   ABSTRACȚIE:   MediaPlayerBase → VideoMediaPlayer, AudioMediaPlayer, LiveStreamPlayer
    //   IMPLEMENTARE: IDeviceRenderer → MobileRenderer, DesktopRenderer, SmartTVRenderer, TabletRenderer
    //   BRIDGE:       _device (referința din MediaPlayerBase către IDeviceRenderer)
    // ============================================================

    /// <summary>
    /// IMPLEMENTOR interface — definește ce poate face orice dispozitiv.
    /// Abstracția nu cunoaște clasele concrete — doar această interfață.
    /// </summary>
    public interface IDeviceRenderer
    {
        void RenderVideo(string title, string quality, bool hasSubtitles);
        void RenderAudio(string title, string audioFormat);
        void RenderLiveStream(string channelName, int viewerCount, bool isHD);
        string GetDeviceName();
        string GetCapabilities();
    }
}
