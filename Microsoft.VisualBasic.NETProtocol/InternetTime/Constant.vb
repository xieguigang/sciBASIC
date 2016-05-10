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
    Public Enum Stratum
        Unspecified         '0 - unspecified or unavailable
        PrimaryReference    '1 - primary reference (e.g. radio-clock)
        SecondaryReference  '2-15 - secondary reference (via NTP or SNTP)
        Reserved            '16-255 - reserved
    End Enum
End Namespace