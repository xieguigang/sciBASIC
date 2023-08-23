'----------------------------------------------------------------------------------------
'	Copyright ©  - 2017 Tangible Software Solutions Inc.
'	This class can be used by anyone provided that the copyright notice remains intact.
'
'	This class includes methods to convert Java rectangular arrays (jagged arrays
'	with inner arrays of the same length).
'----------------------------------------------------------------------------------------
Friend Module RectangularArrays
    Friend Function ReturnRectangularDoubleArray(size1 As Integer, size2 As Integer, size3 As Integer, size4 As Integer) As Double()()()()
        Dim newArray = New Double(size1 - 1)()()() {}
        For array1 = 0 To size1 - 1
            newArray(array1) = New Double(size2 - 1)()() {}
            If size3 > -1 Then
                For array2 = 0 To size2 - 1
                    newArray(array1)(array2) = New Double(size3 - 1)() {}
                    If size4 > -1 Then
                        For array3 = 0 To size3 - 1
                            newArray(array1)(array2)(array3) = New Double(size4 - 1) {}
                        Next
                    End If
                Next
            End If
        Next

        Return newArray
    End Function

    Friend Function ReturnRectangularDoubleArray(size1 As Integer, size2 As Integer) As Double()()
        Dim newArray = New Double(size1 - 1)() {}
        For array1 = 0 To size1 - 1
            newArray(array1) = New Double(size2 - 1) {}
        Next

        Return newArray
    End Function

    Friend Function ReturnRectangularDoubleArray(size1 As Integer, size2 As Integer, size3 As Integer) As Double()()()
        Dim newArray = New Double(size1 - 1)()() {}
        For array1 = 0 To size1 - 1
            newArray(array1) = New Double(size2 - 1)() {}
            If size3 > -1 Then
                For array2 = 0 To size2 - 1
                    newArray(array1)(array2) = New Double(size3 - 1) {}
                Next
            End If
        Next

        Return newArray
    End Function
End Module
