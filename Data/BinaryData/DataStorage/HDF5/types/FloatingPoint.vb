Namespace HDF5.type

    Public Class FloatingPoint : Inherits DataType

        Public Property byteOrder As ByteOrder
        Public Property lowPadding As Boolean
        Public Property highPadding As Boolean
        Public Property internalPadding As Boolean
        Public Property mantissaNormalization As Integer
        Public Property signLocation As Integer
        Public Property bitOffset As Short
        Public Property bitPrecision As Short
        Public Property exponentLocation As SByte
        Public Property exponentSize As SByte
        Public Property mantissaLocation As SByte
        Public Property mantissaSize As SByte
        Public Property exponentBias As Integer

        Public Overrides ReadOnly Property TypeInfo As System.Type
            Get
                Select Case bitPrecision
                    Case 16, 32
                        Return GetType(Single)
                    Case 64
                        Return GetType(Double)
                    Case Else
                        Throw New NotSupportedException("Unsupported signed fixed point data type")
                End Select
            End Get
        End Property

    End Class
End Namespace