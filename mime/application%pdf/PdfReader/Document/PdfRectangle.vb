Imports System
Imports stdNum = System.Math

Namespace PdfReader
    Public Class PdfRectangle
        Inherits PdfObject

        Private _LowerLeftX As Single, _LowerLeftY As Single, _UpperRightX As Single, _UpperRightY As Single

        Public Sub New(ByVal parent As PdfObject, ByVal array As ParseArray)
            MyBase.New(parent, array)
            ' Extract raw values
            Dim lx = ObjectToFloat(array.Objects(0))
            Dim ly = ObjectToFloat(array.Objects(1))
            Dim ux = ObjectToFloat(array.Objects(2))
            Dim uy = ObjectToFloat(array.Objects(3))

            ' Normalize so the lower-left and upper-right are actually those values
            LowerLeftX = stdNum.Min(lx, ux)
            LowerLeftY = stdNum.Min(ly, uy)
            UpperRightX = stdNum.Max(lx, ux)
            UpperRightY = stdNum.Max(ly, uy)
        End Sub

        Public Overrides Function ToString() As String
            Return $"({LowerLeftX},{LowerLeftY}) -> ({UpperRightX},{UpperRightY})"
        End Function

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public Property LowerLeftX As Single
            Get
                Return _LowerLeftX
            End Get
            Private Set(ByVal value As Single)
                _LowerLeftX = value
            End Set
        End Property

        Public Property LowerLeftY As Single
            Get
                Return _LowerLeftY
            End Get
            Private Set(ByVal value As Single)
                _LowerLeftY = value
            End Set
        End Property

        Public Property UpperRightX As Single
            Get
                Return _UpperRightX
            End Get
            Private Set(ByVal value As Single)
                _UpperRightX = value
            End Set
        End Property

        Public Property UpperRightY As Single
            Get
                Return _UpperRightY
            End Get
            Private Set(ByVal value As Single)
                _UpperRightY = value
            End Set
        End Property

        Public ReadOnly Property Width As Single
            Get
                Return UpperRightX - LowerLeftX
            End Get
        End Property

        Public ReadOnly Property Height As Single
            Get
                Return UpperRightY - LowerLeftY
            End Get
        End Property

        Private Function ObjectToFloat(ByVal obj As ParseObjectBase) As Single
            ' Might be an integer if the value has no fractional part
            If TypeOf obj Is ParseInteger Then
                Return TryCast(obj, ParseInteger).Value
            ElseIf TypeOf obj Is ParseReal Then
                Return TryCast(obj, ParseReal).Value
            Else
                Throw New ApplicationException($"Array does not contain numbers that can be converted to a rectangle.")
            End If
        End Function
    End Class
End Namespace
