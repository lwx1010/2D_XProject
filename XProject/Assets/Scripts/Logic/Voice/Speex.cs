using NSpeex;
using System;
using System.Runtime.InteropServices;

public sealed class Speex
{
    private const int FrameSize = 160;

    private SpeexEncoder _encoder;
    private SpeexDecoder _decoder;


    /// <summary>
    /// 初始化。
    /// </summary>
    public Speex()
    {
        _encoder = new SpeexEncoder(BandMode.Narrow);
        _decoder = new SpeexDecoder(BandMode.Narrow);
    }

    /// <summary>
    /// 将采集到的音频数据进行编码。
    /// </summary>       
    public byte[] Encode(byte[] data)
    {
        if (data == null)
            return null;

        if ( data.Length % (FrameSize * 2) != 0)
        {
            throw new ArgumentException("Invalid Data Length.");
        }

        int nbBytes;
        short[] input = new short[FrameSize];
        byte[] buffer = new byte[200];
        byte[] output = new byte[0];
        for (int i = 0; i < data.Length / (FrameSize * 2); i++)
        {
            for (int j = 0; j < input.Length; j++)
            {
                input[j] = (short)(data[i * FrameSize * 2 + j * 2] + data[i * FrameSize * 2 + j * 2 + 1] * 0x100);
            }

            nbBytes = _encoder.Encode(input, 0, input.Length, buffer, 0, buffer.Length);
            Array.Resize<byte>(ref output, output.Length + nbBytes + sizeof(int));
            Array.Copy(buffer, 0, output, output.Length - nbBytes, nbBytes);

            for (int j = 0; j < sizeof(int); j++)
            {
                output[output.Length - nbBytes - sizeof(int) + j] = (byte)(nbBytes % 0x100);
                nbBytes /= 0x100;
            }
        }
        return output;
    }

    /// <summary>
    /// 将编码后的数据进行解码得到原始的音频数据。
    /// </summary>      
    public byte[] Decode(byte[] data)
    {
        int nbBytes, index = 0;
        byte[] input;
        short[] buffer = new short[FrameSize];
        byte[] output = new byte[0];
        while (index < data.Length)
        {
            nbBytes = 0;
            index += sizeof(int);
            for (int i = 1; i <= sizeof(int); i++)
                nbBytes = nbBytes * 0x100 + data[index - i];
            input = new byte[nbBytes];
            Array.Copy(data, index, input, 0, input.Length);
            index += input.Length;
            _decoder.Decode(input, 0, nbBytes, buffer, 0, false);
            Array.Resize<byte>(ref output, output.Length + FrameSize * 2);
            for (int i = 0; i < FrameSize; i++)
            {
                output[output.Length - FrameSize * 2 + i * 2] = (byte)(buffer[i] % 0x100);
                output[output.Length - FrameSize * 2 + i * 2 + 1] = (byte)(buffer[i] / 0x100);
            }
        }
        return output;
    }

}