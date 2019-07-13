#Region "Microsoft.VisualBasic::1a1b655e13d4faf88e1a0094c8519ba3, Data\BinaryData\DataStorage\HDF5\types\FixedPoint.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class FixedPoint
    ' 
    '         Properties: bitOffset, bitPrecision, byteOrder, highPadding, lowPadding
    '                     signed, typeInfo
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics

Namespace HDF5.type

    Public Class FixedPoint : Inherits DataType

        Public Property byteOrder As ByteOrder
        Public Property lowPadding As Boolean
        Public Property highPadding As Boolean
        Public Property signed As Boolean
        Public Property bitOffset As Short
        Public Property bitPrecision As Short

        Public Overrides ReadOnly Property typeInfo As System.Type
            Get
                If signed Then
                    Select Case bitPrecision
                        Case 8 : Return GetType(SByte)
                        Case 16 : Return GetType(Short)
                        Case 32 : Return GetType(Integer)
                        Case 64 : Return GetType(Long)
                        Case Else
                            Throw New NotSupportedException
                    End Select
                Else
                    Select Case bitPrecision
                        Case 8, 16
                            Return GetType(Integer)
                        Case 32
                            Return GetType(Long)
                        Case 64
                            Return GetType(BigInteger)
                        Case Else
                            Throw New NotSupportedException
                    End Select
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"({byteOrder.ToString} {Me.GetType.Name}) {typeInfo.FullName}"
        End Function
    End Class
End Namespace
