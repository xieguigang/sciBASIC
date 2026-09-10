# Data_science/Mathematica/SignalProcessing/wav/wav/waveFile-netcore5.vbproj

- RootNamespace : Microsoft.VisualBasic.Data.Wave
- AssemblyName  : Microsoft.VisualBasic.Data.Wave
- TargetFramework: net10.0
- Source files  : 11
- Existing Title: WAV Audio File Reader, Writer and Voiceprint Extractor
- Existing Desc : Parses and writes RIFF/WAVE audio files with FMT and data sub-chunk access, and extracts mel-filterbank voiceprint vectors with delta coefficients, CMVN and L2 normalisation. Part of sciBASIC#.
- Existing Tags : scibasic;wav;audio;voiceprint;mfcc;riff

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Data.Wave)

## Public types
- Class DataSubChunk (SubChunk\Data.vb)
- Class LazyDataChunk (SubChunk\Data.vb)
- Module ParserDispatch (SubChunk\Data.vb) - Central dispatch for choosing the correct parser based on audio format and bits per sample.
- Enum wFormatTag (SubChunk\Enums.vb)
- Enum Channels (SubChunk\Enums.vb) - Legacy enumeration for common channel counts. For multi-channel support, use Integer values directly on <see cref="FMTSubChunk.channels"/>.
- Enum ChannelPositions (SubChunk\Enums.vb)
- Module WavFormatGuids (SubChunk\Enums.vb) - Well-known GUIDs for WAVE_FORMAT_EXTENSIBLE SubFormat field.
- Class FMTSubChunk (SubChunk\FMT.vb) - The "fmt " subchunk describes the sound data's format
- Structure Sample (SubChunk\Sample.vb)
- Module SignalProcessing (VoicePrint\SignalProcessing.vb) - 信号处理工具模块，提供静态方法供声纹提取流水线调用。
- Class MelFilterbank (VoicePrint\SignalProcessing.vb) - Mel 尺度三角滤波器组，用于将功率谱转换为 Mel 频率能量。 Mel 尺度模拟人耳对频率的感知：低频分辨率高，高频分辨率低。 公式：mel = 2595 * log10(1 + hz/700)
- Module VoicePrint (VoicePrint\VoicePrint.vb)
- Class VoicePrintOptions (VoicePrint\VoicePrintExtractor.vb) - 声纹提取参数配置。
- Class VoicePrintResult (VoicePrint\VoicePrintExtractor.vb) - 声纹提取结果（包含向量和元数据）。
- Class VoicePrintExtractor (VoicePrint\VoicePrintExtractor.vb) - 声纹向量提取器。 纯 DSP 实现，不依赖任何文件 I/O。 输入为归一化的音频样本数组，输出为固定维度的声纹向量。
- Class WavVoicePrintReader (VoicePrint\WavVoicePrintReader.vb) - 基于 WAV 文件的声纹提取辅助类。 提供一次性调用接口，自动处理文件打开、声道选取、时间窗口裁剪。
- Class WaveFile (WaveFile.vb) - The wav file model Waveform Audio File Format (WAVE, or WAV due to its filename extension; pronounced /wæv/ or /weɪv/) is an audio file format standard, developed by IBM and Microsoft, for storing an audio bitstream on
- Module WaveWriter (WaveWriter.vb) - Static utility class for writing WAV audio files from sample data. Supports PCM (8/16/24/32-bit), IEEE Float (32/64-bit), and G.711 A-law/μ-law encoding output.

## Notable public members
- Public Property data As Sample()
- Public Overrides Function LoadSamples(start As Integer, length As Integer, Optional scan0% = 0) As IEnumerable(Of Sample)
- Friend Shared Function ParseData(wav As BinaryDataReader, format As FMTSubChunk) As DataSubChunk
- Public Iterator Function GenericEnumerator() As IEnumerator(Of Sample) Implements Enumeration(Of Sample).GenericEnumerator
- Public MustOverride Iterator Function LoadSamples(start As Integer, length As Integer, Optional scan0% = 0) As IEnumerable(Of Sample)
- Public Sub Close()
- Public Overrides Iterator Function LoadSamples(start As Integer, length As Integer, Optional scan0% = 0) As IEnumerable(Of Sample)
- Public Function MeasureChunkSize(length As Integer) As Long
- Public Function CalculateOffset(start As Integer, Optional scan0% = 0) As Long
- Public Shared Function MoveToDataChunk(wav As BinaryDataReader) As BinaryDataReader
- Friend Function ResolveParser(audioFormat As wFormatTag, bitsPerSample As Integer) _
- Friend Function ResolveSingleSampleParser(audioFormat As wFormatTag, bitsPerSample As Integer) _
- Public Property audioFormat As wFormatTag
- Public Property channels As Integer
- Public Property SampleRate As Integer
- Public Property ByteRate As Integer
- Public Property BlockAlign As Integer
- Public Property BitsPerSample As Integer
- Public Property cbSize As Integer
- Public Property ValidBitsPerSample As Integer
- Public Property ChannelMask As Integer
- Public Property SubFormat As Guid
- Public ReadOnly Property effectiveAudioFormat As wFormatTag
- Public ReadOnly Property isPCM As Boolean
- Public ReadOnly Property sampleSizeBytes As Integer
- Friend Shared Function ParseChunk(wav As BinaryDataReader) As FMTSubChunk
- Public ReadOnly Property left As Single
- Public ReadOnly Property right As Single
- Public Overrides Function ToString() As String
- Friend Shared Iterator Function Parse8Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
- Friend Shared Iterator Function Parse16Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
- Friend Shared Iterator Function Parse24Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
- Friend Shared Iterator Function Parse32BitPCM(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
- Friend Shared Iterator Function Parse32Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
- Friend Shared Iterator Function Parse64Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
- Friend Shared Iterator Function ParseALaw(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
- Friend Shared Iterator Function ParseMuLaw(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
- Friend Shared Function Parse8BitSample(wav As BinaryDataReader, channels As Integer) As Sample
- Friend Shared Function Parse16BitSample(wav As BinaryDataReader, channels As Integer) As Sample
- Friend Shared Function Parse24BitSample(wav As BinaryDataReader, channels As Integer) As Sample
- Friend Shared Function Parse32BitSample(wav As BinaryDataReader, channels As Integer) As Sample
- Friend Shared Function Parse32BitPCMSample(wav As BinaryDataReader, channels As Integer) As Sample
- Friend Shared Function Parse64BitSample(wav As BinaryDataReader, channels As Integer) As Sample
- Friend Shared Function ParseALawSample(wav As BinaryDataReader, channels As Integer) As Sample
- Friend Shared Function ParseMuLawSample(wav As BinaryDataReader, channels As Integer) As Sample
- Public Property chunkID As String
- Public Property chunkSize As Integer
- Public Function ApplyPreEmphasis(samples As Double(), coeff As Double) As Double()
- Public Function FrameSignal(samples As Double(), frameSize As Integer, hopSize As Integer) As List(Of Double())
- Public Function CreateHammingWindow(size As Integer) As Double()
- Public Sub FFT(real As Double(), imag As Double())
- Public Function PowerSpectrum(real As Double(), imag As Double(), numBins As Integer) As Double()
- Public Function DCT(input As Double(), numCoeffs As Integer) As Double()
- Public Function ComputeDelta(features As Double()(), N As Integer) As Double()()
- Public Sub ComputeStatistics(data As Double(), ByRef mean As Double, ByRef std As Double,
- Public Function NextPowerOf2(n As Integer) As Integer
- Public Sub New(numFilters As Integer, fftSize As Integer, sampleRate As Integer,
- Public Function Apply(powerSpectrum As Double()) As Double()
- Public Shared Function HzToMel(hz As Double) As Double
- Public Shared Function MelToHz(mel As Double) As Double
- ... and 32 more

## Imports
- Microsoft.VisualBasic.Data.IO
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.Serialization.JSON
- std = System.Math
- System.Globalization
- System.IO
- System.Runtime.CompilerServices

## File tree
- SubChunk\Data.vb
- SubChunk\Enums.vb
- SubChunk\FMT.vb
- SubChunk\Sample.vb
- SubChunk\SubChunk.vb
- VoicePrint\SignalProcessing.vb
- VoicePrint\VoicePrint.vb
- VoicePrint\VoicePrintExtractor.vb
- VoicePrint\WavVoicePrintReader.vb
- WaveFile.vb
- WaveWriter.vb

