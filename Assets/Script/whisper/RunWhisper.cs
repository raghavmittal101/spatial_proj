using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using UnityEngine.Networking;
using System;

public class RunWhisper : MonoBehaviour
{


    // Link your audioclip here. Format must be 16Hz mono non-compressed.
    public AudioClip audioClip;

    public SpeechRecognitionController speechRecognitionController;

    int numSamples;
    float[] data;
    string outputString = "";

    // Maximum size of audioClip (30s at 16kHz)
    const int maxSamples = 30 * 16000;

    public string serverUrl; // URL of the Python server
    //private string endpoint = "transcribe";

    //private void Start()
    //{
    //    serverUrl = serverUrl + endpoint;
    //}
    public void Transcribe()
    {
        try
        {
            // Reset output string (transcript)
            outputString = "";
            SaveAndSendAudioClip();
        }
        catch (Exception e)
        {
            throw e;
        }

    }

    void LoadAudio()
    {
        Debug.Log("!!!!Loading audio...");
        if (audioClip.frequency != 16000)
        {
            Debug.Log($"The audio clip should have frequency 16kHz. It has frequency {audioClip.frequency / 1000f}kHz");
            return;
        }

        numSamples = audioClip.samples;

        if (numSamples > maxSamples)
        {
            Debug.Log($"The AudioClip is too long. It must be less than 30 seconds. This clip is {numSamples / audioClip.frequency} seconds.");
            return;
        }

        data = new float[numSamples];
        audioClip.GetData(data, 0);
    }


    private string SaveRecordingAsWav()
    {
        try
        {
            string filePath = Path.Combine(Application.persistentDataPath, "recordedAudio.wav");
            if (File.Exists(filePath)) File.Delete(filePath);
            LoadAudio();

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {

                byte[] wavData = ConvertAudioClipToWav(audioClip);
                fileStream.Write(wavData, 0, wavData.Length);
            }

            Debug.Log("Audio saved to: " + filePath);
            return filePath;
        }
        catch(Exception e)
        {
            throw e;
        }
    }


    /// <summary>
    /// Converts an AudioClip to WAV byte data.
    /// </summary>
    /// <param name="audioClip">The audio clip to convert.</param>
    /// <returns>Byte array representing the WAV file.</returns>
    private byte[] ConvertAudioClipToWav(AudioClip audioClip)
    {
        var samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        // Write WAV header
        int sampleCount = samples.Length;
        int frequency = audioClip.frequency;
        int channels = audioClip.channels;

        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + sampleCount * 2);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)channels);
        writer.Write(frequency);
        writer.Write(frequency * channels * 2);
        writer.Write((short)(channels * 2));
        writer.Write((short)16);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        writer.Write(sampleCount * 2);

        // Write audio data
        foreach (var sample in samples)
        {
            short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
            writer.Write(intSample);
        }

        writer.Flush();
        return stream.ToArray();
    }

    class Transcription
    {
        public string transcription;
    }
    Transcription transcription;

    /// <summary>
    /// Sends the saved WAV file to a Python server and retrieves the transcription.
    /// </summary>
    /// <param name="filePath">The path to the WAV file to upload.</param>
    public IEnumerator SendAudioForTranscription(string filePath)
    {


        if (!File.Exists(filePath))
        {
            Debug.LogError("Audio file not found: " + filePath);
            throw new FileNotFoundException("Audio file not found", filePath);
        }
        byte[] audioData = File.ReadAllBytes(filePath);
        string fileName = Path.GetFileName(filePath);
        Dictionary<string, string> headers = new Dictionary<string, string>();
        List<IMultipartFormSection> form = new List<IMultipartFormSection>
        {
           new MultipartFormFileSection("audio", audioData, fileName, "audio/wav")
        };

        byte[] boundary = UnityWebRequest.GenerateBoundary();
        byte[] formSections = UnityWebRequest.SerializeFormSections(form, boundary);
        byte[] terminate = Encoding.UTF8.GetBytes(String.Concat("-", Encoding.UTF8.GetString(boundary), "-"));
        byte[] body = new byte[formSections.Length + terminate.Length];
        Buffer.BlockCopy(formSections, 0, body, 0, formSections.Length);
        Buffer.BlockCopy(terminate, 0, body, formSections.Length, terminate.Length);
        string contentType = String.Concat("multipart/form-data; boundary=", Encoding.UTF8.GetString(boundary));
        UnityWebRequest wr = new UnityWebRequest(serverUrl, "POST");
        UploadHandler uploader = new UploadHandlerRaw(body);
        uploader.contentType = contentType;
        wr.uploadHandler = uploader;
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        wr.downloadHandler = dH;
        yield return wr.SendWebRequest();
        if (wr.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(wr.downloadHandler.text);
            Debug.Log("Transcription successful: " + wr.downloadHandler.text);
            transcription = JsonUtility.FromJson<Transcription>(wr.downloadHandler.text);
            speechRecognitionController.onResponse.Invoke(transcription.transcription);
        }
        else
        {

            Debug.LogError(wr.downloadHandler.text);
            //throw new SystemException(wr.error);
            speechRecognitionController.onError.Invoke("Something went wrong. Check you network connection.");
        }
    }

    public void SaveAndSendAudioClip()
    {
        try
        {
            string path = SaveRecordingAsWav();
            StartCoroutine(SendAudioForTranscription(path));
        }
        catch(Exception e)
        {
            throw e;
        }
        
    }


    private string GetFormPayloadAsString(WWWForm form)
    {
        StringBuilder payload = new StringBuilder();

        // Add headers
        foreach (KeyValuePair<string, string> header in form.headers)
        {
            payload.AppendLine($"{header.Key}: {header.Value}");
        }
        payload.AppendLine();

        // Add form data
        byte[] rawData = form.data;
        string rawDataString = Encoding.UTF8.GetString(rawData);
        payload.AppendLine(rawDataString);

        return payload.ToString();
    }
}