using System.IO;
using NAudio.Wave; // Thư viện mới vừa cài đặt

namespace Logic_Revolver.Engine
{
    public static class AudioManager
    {
        private static WaveOutEvent _bgmDevice;
        private static WaveFileReader _bgmReader;
        private static bool _isBgmLooping = false;

        // Lưu bài nhạc nền hiện tại để có thể bật lại khi user bật Music ON
        private static byte[] _currentBgmData;

        // Trạng thái setting âm thanh
        public static bool MusicEnabled { get; private set; } = true;
        public static bool SfxEnabled { get; private set; } = true;

        // Volume từ 0 -> 100
        public static int MusicVolume { get; private set; } = 70;
        public static int SfxVolume { get; private set; } = 70;

        // 1. Hàm phát nhạc nền (Phát lặp lại liên tục)
        public static void PlayBGM(Stream audioStream)
        {
            if (audioStream == null) return;

            // Lưu lại dữ liệu bài nhạc hiện tại để có thể phát lại khi bật Music ON
            _currentBgmData = CopyStreamToBytes(audioStream);

            if (!MusicEnabled)
            {
                StopAll();
                return;
            }

            PlayBGMFromBytes(_currentBgmData);
        }

        private static void PlayBGMFromBytes(byte[] audioData)
        {
            if (audioData == null || audioData.Length == 0) return;

            StopAll(); // Dừng nhạc cũ trước khi phát bài mới

            _isBgmLooping = true;
            _bgmReader = new WaveFileReader(new MemoryStream(audioData));
            _bgmDevice = new WaveOutEvent();

            _bgmDevice.Init(_bgmReader);
            _bgmDevice.Volume = MusicEnabled ? (MusicVolume / 100f) : 0f;

            // Bắt sự kiện khi phát hết bài thì tự động quay về đầu (Loop)
            _bgmDevice.PlaybackStopped += (s, e) =>
            {
                if (_isBgmLooping && _bgmReader != null && _bgmDevice != null)
                {
                    _bgmReader.Position = 0;
                    _bgmDevice.Play();
                }
            };

            _bgmDevice.Play();
        }

        // 2. Hàm phát tiếng động SFX (Phát song song, không đè nhạc nền)
        public static void PlaySound(Stream audioStream)
        {
            if (audioStream == null) return;
            if (!SfxEnabled) return;

            byte[] sfxData = CopyStreamToBytes(audioStream);

            // Tạo một luồng phát (Device) hoàn toàn mới cho mỗi tiếng động
            WaveOutEvent sfxDevice = new WaveOutEvent();
            WaveFileReader sfxReader = new WaveFileReader(new MemoryStream(sfxData));

            sfxDevice.Init(sfxReader);
            sfxDevice.Volume = SfxVolume / 100f;
            sfxDevice.Play();

            // Tự động dọn dẹp bộ nhớ (Dispose) ngay khi tiếng động phát xong
            sfxDevice.PlaybackStopped += (s, e) =>
            {
                sfxDevice.Dispose();
                sfxReader.Dispose();
            };
        }

        // 3. Hàm dừng nhạc nền
        public static void StopAll()
        {
            _isBgmLooping = false;

            if (_bgmDevice != null)
            {
                _bgmDevice.Stop();
                _bgmDevice.Dispose();
                _bgmDevice = null;
            }
            if (_bgmReader != null)
            {
                _bgmReader.Dispose();
                _bgmReader = null;
            }
        }

        // 4. Chỉnh volume nhạc nền realtime
        public static void SetMusicVolume(int volume)
        {
            if (volume < 0) volume = 0;
            if (volume > 100) volume = 100;

            MusicVolume = volume;

            if (_bgmDevice != null)
            {
                _bgmDevice.Volume = MusicEnabled ? (MusicVolume / 100f) : 0f;
            }
        }

        // 5. Chỉnh volume hiệu ứng realtime
        public static void SetSfxVolume(int volume)
        {
            if (volume < 0) volume = 0;
            if (volume > 100) volume = 100;

            SfxVolume = volume;
        }

        // 6. Bật / tắt nhạc nền realtime
        public static void SetMusicEnabled(bool enabled)
        {
            MusicEnabled = enabled;

            if (!MusicEnabled)
            {
                StopAll();
            }
            else
            {
                if (_currentBgmData != null && _currentBgmData.Length > 0)
                {
                    PlayBGMFromBytes(_currentBgmData);
                }
            }
        }

        // 7. Bật / tắt hiệu ứng realtime
        public static void SetSfxEnabled(bool enabled)
        {
            SfxEnabled = enabled;
        }

        private static byte[] CopyStreamToBytes(Stream audioStream)
        {
            if (audioStream == null) return null;

            long oldPos = 0;
            if (audioStream.CanSeek)
            {
                oldPos = audioStream.Position;
                audioStream.Position = 0;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                audioStream.CopyTo(ms);

                if (audioStream.CanSeek)
                    audioStream.Position = oldPos;

                return ms.ToArray();
            }
        }
    }
}