'----------------------------------------------------------------------------------------
'	Copyright © 2006 - 2012 Tangible Software Solutions Inc.
'	This class can be used by anyone provided that the copyright notice remains intact.
'
'	This class provides the ability to simulate the behavior of the C/C++ functions for 
'	generating random numbers, using the .NET Framework System.Random class.
'	'rand' converts to the parameterless overload of NextNumber
'	'random' converts to the single-parameter overload of NextNumber
'	'randomize' converts to the parameterless overload of Seed
'	'srand' converts to the single-parameter overload of Seed
'----------------------------------------------------------------------------------------

Namespace Language.C

    ''' <summary>
    ''' 
    ''' </summary>
    Public Module RandomNumbers

        Dim r As Random

        Sub New()
            Call Seed()
        End Sub

        Public Function NextNumber() As Integer
            Return r.[Next]()
        End Function

        Public Function NextNumber(ceiling As Integer) As Integer
            Return r.[Next](ceiling)
        End Function

        Public Sub Seed()
            r = New Random()
        End Sub

        Public Sub Seed(seed__1 As Integer)
            r = New Random(seed__1)
        End Sub
    End Module
End Namespace