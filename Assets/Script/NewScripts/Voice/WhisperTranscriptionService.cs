using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;

/// <summary>
/// Implementation of ITranscriptionService using the Whisper API
/// </summary>
public class WhisperTranscriptionService : MonoBehaviour, ITranscriptionService
{
    public event Action<string> TranscriptionCompleted;
    public event Action<string> TranscriptionError;
    public event Action TranscriptionStarted;
    
    private const int MaxSamples = 30 * 16000; // 30 seconds at 16kHz
    private const int RequiredFrequency = 16000; // 16kHz required by Whisper
    private const string DefaultEndpoint = "transcribe";
    
    private string _serverUrl = "";
    private bool _isTranscribing = false;
    
    public bool IsTranscribing => _isTranscribing;
    
    /// <summary>
    /// Configure the service with server URL
    /// </summary>
    public void Configure(string serverUrl)
    {
        if (string.IsNullOrEmpty(serverUrl))
        {
            Debug.LogError("Server URL cannot be empty");
            return;
        }
        
        _serverUrl = serverUrl;
        if (!_serverUrl.EndsWith("/"))
        {
            _serverUrl += "/";
        }
        _serverUrl += DefaultEndpoint;
        
        Debug.Log($"Whisper transcription service configured with URL: {_serverUrl}");
    }
    
    /// <summary>
    /// Transcribe an audio clip by sending it to the Whisper server
    /// </summary>
    public void TranscribeAudio(AudioClip audioClip)
    {
        if (_isTranscribing)
        {
            TranscriptionError?.Invoke("Another transcription is already in progress");
            return;
        }
        
        if (string.IsNullOrEmpty(_serverUrl))
        {
            TranscriptionError?.Invoke("Transcription service not configured");
            return;
        }
        
        if (audioClip == null)
        {
            TranscriptionError?.Invoke("No audio clip provided");
            return;
        }
        
        // Check audio clip format
        if (audioClip.frequency != RequiredFrequency)
        {
            TranscriptionError?.Invoke($"Audio clip must be {RequiredFrequency}Hz. Current: {audioClip.frequency}Hz");
            return;
        }
        
        if (audioClip.samples > MaxSamples)
        {
            TranscriptionError?.Invoke($"Audio clip too long. Maximum: {MaxSamples / RequiredFrequency} seconds");
            return;
        }
        
        StartCoroutine(TranscribeAudioCoroutine(audioClip));
    }
    
    /// <summary>
    /// Coroutine to handle audio transcription process
    /// </summary>
    private IEnumerator TranscribeAudioCoroutine(AudioClip audioClip)
    {
        _isTranscribing = true;
        TranscriptionStarted?.Invoke();
        
            // Convert AudioClip to WAV format
            byte[] wavData = ConvertAudioClipToWav(audioClip);
            
            // Prepare the form data
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("audio", wavData, "audio.wav", "audio/wav")
            };
            
            // Create the request
            byte[] boundary = UnityWebRequest.GenerateBoundary();
            byte[] formSections = UnityWebRequest.SerializeFormSections(formData, boundary);
            byte[] terminate = Encoding.UTF8.GetBytes($"--{Encoding.UTF8.GetString(boundary)}--");
            
            byte[] body = new byte[formSections.Length + terminate.Length];
            Buffer.BlockCopy(formSections, 0, body, 0, formSections.Length);
            Buffer.BlockCopy(terminate, 0, body, formSections.Length, terminate.Length);
            
            // Set content type
            string contentType = $"multipart/form-data; boundary={Encoding.UTF8.GetString(boundary)}";
            
            // Create and send the web request
            UnityWebRequest request = new UnityWebRequest(_serverUrl, "POST");
            UploadHandlerRaw uploadHandler = new UploadHandlerRaw(body);
            uploadHandler.contentType = contentType;
            request.uploadHandler = uploadHandler;
            request.downloadHandler = new DownloadHandlerBuffer();
            
            yield return request.SendWebRequest();
        try { 

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Transcription successful: {request.downloadHandler.text}");
                
                // Parse JSON response
                TranscriptionResponse response = JsonUtility.FromJson<TranscriptionResponse>(request.downloadHandler.text);
                TranscriptionCompleted?.Invoke(response.transcription);
            }
            else
            {
                Debug.LogError($"Transcription failed: {request.error}");
                TranscriptionError?.Invoke($"Transcription failed: {request.error}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during transcription: {ex.Message}");
            TranscriptionError?.Invoke($"Error during transcription: {ex.Message}");
        }
        finally
        {
            _isTranscribing = false;
        }
    }
    
    /// <summary>
    /// Convert an AudioClip to WAV byte array
    /// </summary>
    private byte[] ConvertAudioClipToWav(AudioClip audioClip)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            // Get audio samples
            float[] samples = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(samples, 0);
            
            int sampleCount = samples.Length;
            int channels = audioClip.channels;
            int frequency = audioClip.frequency;
            
            // Write WAV header
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + sampleCount * 2);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1); // PCM format
            writer.Write((short)channels);
            writer.Write(frequency);
            writer.Write(frequency * channels * 2);
            writer.Write((short)(channels * 2));
            writer.Write((short)16); // 16-bit
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(sampleCount * 2);
            
            // Write audio data
            foreach (float sample in samples)
            {
                short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                writer.Write(intSample);
            }
            
            return stream.ToArray();
        }
    }
    
    /// <summary>
    /// Class for JSON deserialization of transcription response
    /// </summary>
    [Serializable]
    private class TranscriptionResponse
    {
        public string transcription;
    }
}