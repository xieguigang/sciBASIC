' Bzip2 library for .net
' By Jaime Olivares
' Location: http://github.com/jaime-olivares/bzip2
' Ported from the Java implementation by Matthew Francis: https://github.com/MateuszBartosiewicz/bzip2

Imports System

Namespace Bzip2
    ''' <summary>
    ''' A 256 entry Move To Front transform
    ''' </summary>
    Friend Class MoveToFront
#Region "Private fields"
        ''' <summary>The Move To Front list</summary> 
        Private ReadOnly mtf As Byte()
#End Region

#Region "Public methods"
        ''' <summary>Public constructor</summary>
        Public Sub New()
            mtf = New Byte(255) {}

            For i = 0 To 256 - 1
                mtf(i) = CByte(i)
            Next
        End Sub

        ''' <summary>Moves a value to the head of the MTF list (forward Move To Front transform)</summary>
        ''' <param name="value">The value to move</param>
        ''' <return>The position the value moved from</return>
        Public Function ValueToFront(value As Byte) As Integer
            Dim index = 0
            Dim temp = mtf(0)
            If value = temp Then Return index
            mtf(0) = value

            While temp <> value
                index += 1
                Dim temp2 = temp
                temp = mtf(index)
                mtf(index) = temp2
            End While

            Return index
        End Function

        ''' <summary>Gets the value from a given index and moves it to the front of the MTF list (inverse Move To Front transform)</summary>
        ''' <param name="index">The index to move</param>
        ''' <return>The value at the given index</return>
        Public Function IndexToFront(index As Integer) As Byte
            Dim value = mtf(index)
            Array.ConstrainedCopy(mtf, 0, mtf, 1, index)
            mtf(0) = value
            Return value
        End Function
#End Region
    End Class
End Namespace
