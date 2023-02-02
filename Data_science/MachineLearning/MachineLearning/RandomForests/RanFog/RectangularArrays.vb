'----------------------------------------------------------------------------------------
'	Copyright ©  - 2017 Tangible Software Solutions Inc.
'	This class can be used by anyone provided that the copyright notice remains intact.
'
'	This class includes methods to convert Java rectangular arrays (jagged arrays
'	with inner arrays of the same length).
'----------------------------------------------------------------------------------------
Friend Module RectangularArrays
    Friend Function ReturnRectangularIntArray(ByVal size1 As Integer, ByVal size2 As Integer) As Integer()()
        Dim newArray = New Integer(size1 - 1)() {}
        For array1 = 0 To size1 - 1
            newArray(array1) = New Integer(size2 - 1) {}
        Next

        Return newArray
    End Function

    Friend Function ReturnRectangularDoubleArray(ByVal size1 As Integer, ByVal size2 As Integer) As Double()()
        Dim newArray = New Double(size1 - 1)() {}
        For array1 = 0 To size1 - 1
            newArray(array1) = New Double(size2 - 1) {}
        Next

        Return newArray
    End Function
End Module
