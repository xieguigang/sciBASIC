'---------------------------------------------------------------------------------------------------------
'	Copyright ©  - 2017 Tangible Software Solutions Inc.
'	This class can be used by anyone provided that the copyright notice remains intact.
'
'	This class is used to replace calls to the static java.lang.Math.random method.
'---------------------------------------------------------------------------------------------------------
Friend Module GlobalRandom
    Private randomInstance As Random = Nothing

    Friend ReadOnly Property NextDouble As Double
        Get
            If randomInstance Is Nothing Then randomInstance = New Random()

            Return randomInstance.NextDouble()
        End Get
    End Property
End Module
