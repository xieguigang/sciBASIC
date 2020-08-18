
Public Enum wFormatTag
    ''' <summary>
    ''' PCM
    ''' </summary>
    WAVE_FORMAT_PCM = &H1
    ''' <summary>
    ''' IEEE float
    ''' </summary>
    WAVE_FORMAT_IEEE_FLOAT = &H3
    ''' <summary>
    ''' 8-bit ITU-T G.711 A-law
    ''' </summary>
    WAVE_FORMAT_ALAW = &H6
    ''' <summary>
    ''' 8-bit ITU-T G.711 µ-law
    ''' </summary>
    WAVE_FORMAT_MULAW = &H7
    ''' <summary>
    ''' Determined by SubFormat
    ''' </summary>
    WAVE_FORMAT_EXTENSIBLE = &HFFFE
End Enum

Public Enum Channels
    Mono = 1
    Stereo = 2
End Enum