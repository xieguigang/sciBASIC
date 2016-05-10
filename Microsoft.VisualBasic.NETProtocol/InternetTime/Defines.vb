Namespace InternetTime

    ''' <summary>
    ''' Leap indicator field values
    ''' </summary>
    Public Enum _LeapIndicator
        NoWarning       '0 - No warning
        LastMinute61    '1 - Last minute has 61 seconds
        LastMinute59    '2 - Last minute has 59 seconds
        Alarm           '3 - Alarm condition (clock not synchronized)
    End Enum

    ''' <summary>
    ''' Mode field values
    ''' </summary>
    Public Enum _Mode
        SymmetricActive     '1 - Symmetric active
        SymmetricPassive    '2 - Symmetric pasive
        Client              '3 - Client
        Server              '4 - Server
        Broadcast           '5 - Broadcast
        Unknown             '0, 6, 7 - Reserved
    End Enum

    ''' <summary>
    ''' Stratum field values
    ''' </summary>
    Public Enum _Stratum
        Unspecified         '0 - unspecified or unavailable
        PrimaryReference    '1 - primary reference (e.g. radio-clock)
        SecondaryReference  '2-15 - secondary reference (via NTP or SNTP)
        Reserved            '16-255 - reserved
    End Enum
End Namespace