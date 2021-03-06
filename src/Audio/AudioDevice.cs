#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	internal static class AudioDevice
	{
		#region RendererDetail List

		public static ReadOnlyCollection<RendererDetail> Renderers;

		#endregion

		#region Internal AL Device

		// FIXME: readonly? -flibit
		public static IALDevice ALDevice;

		#endregion

		#region SoundEffect Management Variables

		// FIXME: readonly? -flibit

		// Used to store SoundEffectInstances generated internally.
		public static List<SoundEffectInstance> InstancePool;

		// Used to store all DynamicSoundEffectInstances, to check buffer counts.
		public static List<DynamicSoundEffectInstance> DynamicInstancePool;

		#endregion

		#region Microphone Management Variables

		// FIXME: readonly? -flibit

		// Used to store Microphones that are currently recording
		public static List<Microphone> ActiveMics;

		#endregion

		#region Public Static Initialize Method

		public static void Initialize()
		{
			// We should only have one of these!
			if (ALDevice != null)
			{
				FNALoggerEXT.LogWarn("ALDevice already exists, overwriting!");
			}

			bool disableSound = Environment.GetEnvironmentVariable(
				"FNA_AUDIO_DISABLE_SOUND"
			) == "1";

			if (disableSound)
			{
				ALDevice = new NullALDevice();
			}
			else
			{
				ALDevice = FNAPlatform.CreateALDevice();
			}

			// Populate device list
			if (ALDevice != null)
			{
				Renderers = ALDevice.GetDevices();
				Microphone.All = ALDevice.GetCaptureDevices();

				InstancePool = new List<SoundEffectInstance>();
				DynamicInstancePool = new List<DynamicSoundEffectInstance>();
				ActiveMics = new List<Microphone>();
				AppDomain.CurrentDomain.ProcessExit += Dispose;
			}
			else
			{
				Renderers = new ReadOnlyCollection<RendererDetail>(new List<RendererDetail>());
				Microphone.All = new ReadOnlyCollection<Microphone>(new List<Microphone>());
			}
		}

		#endregion

		#region Private Static Dispose Method

		private static void Dispose(object sender, EventArgs e)
		{
			InstancePool.Clear();
			DynamicInstancePool.Clear();
			ALDevice.Dispose();
		}

		#endregion

		#region Public Static Update Methods

		public static void Update()
		{
			ALDevice.Update();

			for (int i = 0; i < InstancePool.Count; i += 1)
			{
				if (InstancePool[i].State == SoundState.Stopped)
				{
					InstancePool[i].Dispose();
					InstancePool.RemoveAt(i);
					i -= 1;
				}
			}

			for (int i = 0; i < DynamicInstancePool.Count; i += 1)
			{
				DynamicSoundEffectInstance sfi = DynamicInstancePool[i];
				sfi.Update();
				if (sfi.State == SoundState.Stopped)
				{
					i -= 1;
				}
			}

			foreach (Microphone mic in ActiveMics)
			{
				mic.CheckBuffer();
			}
		}

		#endregion

		#region Public Static Buffer Methods

		public static IALBuffer GenBuffer()
		{
			if (ALDevice == null)
			{
				throw new NoAudioHardwareException();
			}
			return ALDevice.GenBuffer();
		}

		public static IALBuffer GenBuffer(
			byte[] data,
			uint sampleRate,
			uint channels,
			uint loopStart,
			uint loopEnd,
			bool isADPCM,
			uint formatParameter
		) {
			if (ALDevice == null)
			{
				throw new NoAudioHardwareException();
			}
			return ALDevice.GenBuffer(
				data,
				sampleRate,
				channels,
				loopStart,
				loopEnd,
				isADPCM,
				formatParameter
			);
		}

		#endregion

		#region Public Static Reverb Methods

		public static IALReverb GenReverb(DSPParameter[] parameters)
		{
			if (ALDevice == null)
			{
				throw new NoAudioHardwareException();
			}
			return ALDevice.GenReverb(parameters);
		}

		#endregion
	}
}
