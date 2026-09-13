using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using UWGame.ClientSide;
using UWGame.SimSide.Maps;

namespace UWGame.Client.Audio;

public class AudioManager
{
	private struct MusicFadeEffect
	{
		public float SourceVolume;

		public float TargetVolume;

		private TimeSpan time;

		private TimeSpan duration;

		public bool PauseWhenFinished;

		public MusicFadeEffect(float sourceVolume, float targetVolume, TimeSpan duration, bool pauseWhenFinished)
		{
			SourceVolume = sourceVolume;
			TargetVolume = targetVolume;
			time = TimeSpan.Zero;
			this.duration = duration;
			PauseWhenFinished = pauseWhenFinished;
		}

		public bool Update(TimeSpan deltaTime)
		{
			time += deltaTime;
			if (time >= duration)
			{
				time = duration;
				return true;
			}
			return false;
		}

		public float GetVolume()
		{
			return MathHelper.Lerp(SourceVolume, TargetVolume, (float)time.Ticks / (float)duration.Ticks);
		}
	}

	private Song currentSong;

	private PlayedSound[] playingSounds = new PlayedSound[32];

	private Dictionary<SoundData, PlayedSound> groupedSounds = new Dictionary<SoundData, PlayedSound>();

	private bool isMusicPaused;

	private bool isFadingMusic;

	private MusicFadeEffect fadeEffect;

	private MediaState mediaPlayerState;

	private float mediaPlayerVolume;

	private const int MaxSounds = 32;

	private bool canPlayMusic;

	private const float centerRadiusForMaxVolume = 500f;

	private const float centerRadiusForMinVolume = 850f;

	private const float minVolumeFactor = 0.2f;

	public int MusicFadeInMilliseconds = 800;

	private float CurrentMusicVolume
	{
		get
		{
			return mediaPlayerVolume;
		}
		set
		{
			try
			{
				MediaPlayer.Volume = value;
				mediaPlayerVolume = value;
			}
			catch (NullReferenceException)
			{
			}
		}
	}

	public float MusicVolume { get; set; }

	public float SoundVolume
	{
		get
		{
			return SoundEffect.MasterVolume;
		}
		set
		{
			SoundEffect.MasterVolume = value;
		}
	}

	public bool IsSongPaused
	{
		get
		{
			if (currentSong != null)
			{
				return isMusicPaused;
			}
			return false;
		}
	}

	public AudioManager(Options options, bool canPlayMusic)
	{
		this.canPlayMusic = canPlayMusic;
		mediaPlayerState = MediaPlayer.State;
		MediaPlayer.MediaStateChanged += MediaPlayer_MediaStateChanged;
		Init(options);
	}

	public void Destroy()
	{
		MediaPlayer.MediaStateChanged -= MediaPlayer_MediaStateChanged;
	}

	private void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
	{
		mediaPlayerState = MediaPlayer.State;
	}

	public void Init(Options options, bool useFading = true)
	{
		if (options != null)
		{
			if (options.MusicEnabled)
			{
				MusicVolume = options.MusicVolume;
			}
			else
			{
				MusicVolume = 0f;
			}
			if (!useFading)
			{
				CurrentMusicVolume = MusicVolume;
			}
			if (options.SoundEnabled)
			{
				SoundVolume = options.SoundFXVolume;
			}
			else
			{
				SoundVolume = 0f;
			}
		}
	}

	public void PlaySong(Song song, bool loop = false)
	{
		if (!canPlayMusic)
		{
			throw new Exception("Can't play music in this player");
		}
		if ((currentSong != null && currentSong.IsDisposed) || currentSong != song)
		{
			if (currentSong != null)
			{
				MediaPlayer.Stop();
			}
			currentSong = song;
			isMusicPaused = false;
			MediaPlayer.IsRepeating = loop;
			MediaPlayer.Play(currentSong);
		}
	}

	public void PauseSong()
	{
		if (canPlayMusic && currentSong != null && !isMusicPaused)
		{
			MediaPlayer.Pause();
			isMusicPaused = true;
		}
	}

	public void ResumeSong()
	{
		if (canPlayMusic && currentSong != null && isMusicPaused)
		{
			MediaPlayer.Resume();
			isMusicPaused = false;
		}
	}

	public void StopSong()
	{
		if (canPlayMusic && currentSong != null && mediaPlayerState != MediaState.Stopped)
		{
			MediaPlayer.Stop();
			isMusicPaused = false;
			currentSong = null;
		}
	}

	public void FadeSong(float targetVolume, TimeSpan duration, bool pauseAtEnd)
	{
		if (canPlayMusic)
		{
			if (duration <= TimeSpan.Zero)
			{
				throw new ArgumentException("Duration must be a positive value");
			}
			fadeEffect = new MusicFadeEffect(CurrentMusicVolume, targetVolume, duration, pauseAtEnd);
			isFadingMusic = true;
		}
	}

	public SoundEffectInstance PlayWorldSound(SoundData sound, WorldLocation location, float fading = 1f, bool looping = false, Action<PlayedSound> soundEndedCallback = null)
	{
		GetSoundPanAndDistanceFactor(location.ToVector3(), out var pan, out var distanceVolumeFactor);
		return PlaySound(sound, distanceVolumeFactor, fading, pan, looping, soundEndedCallback);
	}

	public string PrintSoundsForDebug()
	{
		StringBuilder stringBuilder = new StringBuilder();
		PlayedSound[] array = playingSounds;
		foreach (PlayedSound playedSound in array)
		{
			if (playedSound != null)
			{
				stringBuilder.AppendLine(playedSound.ToString());
			}
			else
			{
				stringBuilder.AppendLine("null");
			}
		}
		return stringBuilder.ToString();
	}

	private int GetSoundIndex(SoundData sound, SoundEffectInstance instance)
	{
		return Array.FindIndex(playingSounds, (PlayedSound s) => s != null && s.SoundData == sound && s.SoundEffectInstance == instance);
	}

	public SoundEffectInstance PlaySound(SoundData sound, float distanceFactor, float fading = 1f, float pan = 0f, bool loop = false, Action<PlayedSound> soundEndedCallback = null)
	{
		int soundIndex;
		if (sound.PlayMaxOneInstance && groupedSounds.TryGetValue(sound, out var value))
		{
			if (soundEndedCallback != null)
			{
				value.SoundEndedEvent += soundEndedCallback;
			}
			if (value.SoundEffectInstance.State != SoundState.Playing)
			{
				value.SoundEffectInstance.Play();
			}
			soundIndex = GetSoundIndex(sound, value.SoundEffectInstance);
			if (soundIndex < 0)
			{
				soundIndex = GetAvailableSoundIndex();
				if (soundIndex == -1)
				{
					return null;
				}
				playingSounds[soundIndex] = value;
			}
			return value.SoundEffectInstance;
		}
		soundIndex = GetAvailableSoundIndex();
		if (soundIndex != -1)
		{
			float pitch = 0f;
			if (sound.RandomPitchChange != null)
			{
				pitch = (float)sound.RandomPitchChange.GetRandomValue(The.Client.ClientRandomGenerator);
			}
			SoundEffectInstance soundEffectInstance = sound.SoundEffect.CreateInstance();
			PlayedSound playedSound = new PlayedSound
			{
				SoundEffectInstance = soundEffectInstance,
				SoundData = sound
			};
			playedSound.SoundEndedEvent += soundEndedCallback;
			playingSounds[soundIndex] = playedSound;
			SetSoundVolume(soundEffectInstance, sound.Volume, fading, distanceFactor, pan);
			soundEffectInstance.Pitch = pitch;
			soundEffectInstance.IsLooped = loop;
			soundEffectInstance.Play();
			if (sound.PlayMaxOneInstance)
			{
				groupedSounds.Add(sound, playedSound);
			}
			return soundEffectInstance;
		}
		return null;
	}

	public void StopSound(SoundData sound, SoundEffectInstance soundInstance)
	{
		if (!sound.PlayMaxOneInstance)
		{
			int num = (num = GetSoundIndex(sound, soundInstance));
			if (num >= 0)
			{
				PlayedSound sound2 = playingSounds[num];
				StopSound(sound2);
			}
		}
	}

	private void StopSound(PlayedSound sound)
	{
		sound?.SoundEffectInstance.Stop(immediate: true);
		int soundIndex = GetSoundIndex(sound.SoundData, sound.SoundEffectInstance);
		if (soundIndex >= 0)
		{
			playingSounds[soundIndex] = null;
		}
	}

	public void StopAllSounds()
	{
		for (int i = 0; i < playingSounds.Length; i++)
		{
			PlayedSound playedSound = playingSounds[i];
			if (playedSound != null)
			{
				playedSound.SoundEffectInstance.Stop();
				playedSound.SoundEffectInstance.Dispose();
				playingSounds[i] = null;
			}
		}
	}

	public void SetSoundLocation(SoundEffectInstance instance, SoundData sounddata, float fading, Vector3 location)
	{
		GetSoundPanAndDistanceFactor(location, out var pan, out var distanceVolumeFactor);
		if (sounddata.PlayMaxOneInstance)
		{
			Common.AddToList(ref groupedSounds[sounddata].Sources, new Tuple<Vector3, float>(location, fading));
		}
		else
		{
			SetSoundVolumeAndPan(instance, sounddata, fading, distanceVolumeFactor, pan);
		}
	}

	public static void SetSoundVolumeAndPan(SoundEffectInstance instance, SoundData sounddata, float fading, float distanceFactor, float pan)
	{
		SetSoundVolume(instance, sounddata.Volume, fading, distanceFactor, pan);
	}

	public static void SetSoundVolume(SoundEffectInstance instance, float sounddataVolume, float fading, float distanceFactor, float pan)
	{
		instance.Volume = sounddataVolume * fading * distanceFactor;
		instance.Pan = pan;
	}

	private static void GetSoundPanAndDistanceFactor(Vector3 location, out float pan, out float distanceVolumeFactor)
	{
		float num = Common.Clamp(location.X - The.MapUI.MapWindowWorldPosition.X, 0f, The.MapUI.mapWindowWidth);
		num /= (float)The.MapUI.mapWindowWidth;
		num -= 0.5f;
		num = 2f * num;
		pan = num;
		float num2 = Common.DistanceOctile(The.MapUI.MapWindowWorldPosition + new Vector2((float)The.MapUI.mapWindowWidth * 0.5f, (float)The.MapUI.mapWindowHeight * 0.5f), location.ToVector2());
		if (num2 > 500f)
		{
			float num3 = num2 - 500f;
			num3 /= 350f;
			num3 = Common.Clamp(num3, 0f, 1f);
			distanceVolumeFactor = MathHelper.Lerp(1f, 0.2f, num3);
		}
		else
		{
			distanceVolumeFactor = 1f;
		}
	}

	public void Update(GameTime gameTime)
	{
		UpdateGroupedSounds();
		UpdateEndedSounds();
		UpdateMusic(gameTime);
	}

	private void UpdateEndedSounds()
	{
		for (int i = 0; i < playingSounds.Length; i++)
		{
			PlayedSound playedSound = playingSounds[i];
			if (playedSound != null && playedSound.SoundEffectInstance.State == SoundState.Stopped)
			{
				playingSounds[i] = null;
				playedSound.SoundEnded();
			}
		}
	}

	private void UpdateMusic(GameTime gameTime)
	{
		if (currentSong != null && mediaPlayerState == MediaState.Stopped)
		{
			currentSong = null;
			isMusicPaused = false;
		}
		if (!isFadingMusic || isMusicPaused)
		{
			return;
		}
		if (currentSong != null && mediaPlayerState == MediaState.Playing)
		{
			if (fadeEffect.Update(gameTime.ElapsedGameTime))
			{
				isFadingMusic = false;
				if (fadeEffect.PauseWhenFinished)
				{
					PauseSong();
				}
			}
			CurrentMusicVolume = fadeEffect.GetVolume();
		}
		else
		{
			isFadingMusic = false;
		}
	}

	private void UpdateGroupedSounds()
	{
		List<PlayedSound> list = null;
		foreach (KeyValuePair<SoundData, PlayedSound> groupedSound in groupedSounds)
		{
			if (groupedSound.Value.Sources != null && groupedSound.Value.Sources.Count > 0)
			{
				Vector3 zero = Vector3.Zero;
				float num = 0f;
				foreach (Tuple<Vector3, float> source in groupedSound.Value.Sources)
				{
					zero += source.Item1 / groupedSound.Value.Sources.Count;
					num += source.Item2;
				}
				num = Common.ClampTop(num, 1f);
				GetSoundPanAndDistanceFactor(zero, out var pan, out var distanceVolumeFactor);
				PlayedSound playedSound = groupedSounds[groupedSound.Key];
				SetSoundVolumeAndPan(playedSound.SoundEffectInstance, playedSound.SoundData, num, distanceVolumeFactor, pan);
				groupedSound.Value.Sources.Clear();
			}
			else
			{
				Common.AddToList(ref list, groupedSound.Value);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (PlayedSound item in list)
		{
			SetSoundVolumeAndPan(item.SoundEffectInstance, item.SoundData, 0f, 0f, 0f);
		}
	}

	public void Resume()
	{
		for (int i = 0; i < playingSounds.Length; i++)
		{
			PlayedSound playedSound = playingSounds[i];
			if (playedSound != null && playedSound.SoundEffectInstance.State == SoundState.Paused)
			{
				playedSound.SoundEffectInstance.Resume();
			}
		}
		ResumeSong();
		FadeSong(MusicVolume, new TimeSpan(0, 0, 0, 0, MusicFadeInMilliseconds), pauseAtEnd: false);
	}

	public void Pause()
	{
		for (int i = 0; i < playingSounds.Length; i++)
		{
			PlayedSound playedSound = playingSounds[i];
			if (playedSound != null && playedSound.SoundEffectInstance.State == SoundState.Playing)
			{
				playedSound.SoundEffectInstance.Pause();
			}
		}
		FadeSong(0f, new TimeSpan(0, 0, 0, 0, MusicFadeInMilliseconds), pauseAtEnd: true);
	}

	private int GetAvailableSoundIndex()
	{
		for (int i = 0; i < playingSounds.Length; i++)
		{
			if (playingSounds[i] == null)
			{
				return i;
			}
		}
		return -1;
	}
}
