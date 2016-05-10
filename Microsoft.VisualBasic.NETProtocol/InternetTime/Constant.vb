Namespace InternetTime

    ''' <summary>
    ''' Leap indicator field values
    ''' </summary>
    Public Enum LeapIndicator
        ''' <summary>
        ''' 0 - No warning
        ''' </summary>
        NoWarning       '0 - No warning
        ''' <summary>
        ''' 1 - Last minute has 61 seconds
        ''' </summary>
        LastMinute61    '1 - Last minute has 61 seconds
        ''' <summary>
        ''' 2 - Last minute has 59 seconds
        ''' </summary>
        LastMinute59    '2 - Last minute has 59 seconds
        ''' <summary>
        ''' 3 - Alarm condition (clock not synchronized)
        ''' </summary>
        Alarm           '3 - Alarm condition (clock not synchronized)
    End Enum

    ''' <summary>
    ''' Mode field values
    ''' </summary>
    Public Enum Mode
        ''' <summary>
        ''' 1 - Symmetric active
        ''' </summary>
        SymmetricActive     '1 - Symmetric active
        ''' <summary>
        ''' 2 - Symmetric pasive
        ''' </summary>
        SymmetricPassive    '2 - Symmetric pasive
        ''' <summary>
        ''' 3 - Client
        ''' </summary>
        Client              '3 - Client
        ''' <summary>
        ''' 4 - Server
        ''' </summary>
        Server              '4 - Server
        ''' <summary>
        ''' 5 - Broadcast
        ''' </summary>
        Broadcast           '5 - Broadcast
        ''' <summary>
        ''' 0, 6, 7 - Reserved
        ''' </summary>
        Unknown             '0, 6, 7 - Reserved
    End Enum

    ''' <summary>
    ''' Stratum field values
    ''' </summary>
    Public Enum Stratum
        ''' <summary>
        ''' 0 - unspecified or unavailable
        ''' </summary>
        Unspecified         '0 - unspecified or unavailable
        ''' <summary>
        ''' 1 - primary reference (e.g. radio-clock)
        ''' </summary>
        PrimaryReference    '1 - primary reference (e.g. radio-clock)
        ''' <summary>
        ''' 2-15 - secondary reference (via NTP or SNTP)
        ''' </summary>
        SecondaryReference  '2-15 - secondary reference (via NTP or SNTP)
        ''' <summary>
        ''' 16-255 - reserved
        ''' </summary>
        Reserved            '16-255 - reserved
    End Enum
End Namespace