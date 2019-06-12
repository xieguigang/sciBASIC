Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

Namespace ComponentModel

    ''' <summary>
    ''' {Properties} -> Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class IntegerEntity : Inherits EntityBase(Of Integer)

        <XmlAttribute>
        Public Property [Class] As Integer

        Public Overrides Function ToString() As String
            Return $"<{String.Join("; ", entityVector)}> --> {[Class]}"
        End Function

        Default Public Overloads ReadOnly Property ItemValue(Index As Integer) As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return entityVector(Index)
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(properties As Double()) As IntegerEntity
            Return New IntegerEntity With {
                .entityVector = (From x In properties Select CType(x, Integer)).ToArray
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(properties As Integer()) As IntegerEntity
            Return New IntegerEntity With {
                .entityVector = properties
            }
        End Operator
    End Class
End Namespace