Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.SyntaxAPI.Vectors
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LinearAlgebra

    ''' <summary>
    ''' Numerics vector converts for numeric types like:
    ''' <see cref="Integer"/>, <see cref="Long"/>, <see cref="ULong"/>, <see cref="Byte"/>, <see cref="Single"/>
    ''' </summary>
    Public Module NumericsVector

        <Extension> Public Function AsVector(Of T As {Structure, IComparable, IComparable(Of T), IEquatable(Of T), IConvertible, IFormattable})(source As IEnumerable(Of T)) As Vector
            Return New Vector(source.Select(Function(x) CDbl(CObj(x))))
        End Function

        <Extension> Public Function AsInteger(vector As Vector) As Integer()
            Return vector.Select(Function(x) CInt(x)).ToArray
        End Function

        <Extension> Public Function AsLong(vector As Vector) As Long()
            Return vector.Select(Function(x) CLng(x)).ToArray
        End Function

        <Extension> Public Function AsSingle(vector As Vector) As Single()
            Return vector.Select(Function(x) CSng(x)).ToArray
        End Function

        <Extension> Public Function AsUInteger(vector As Vector) As UInteger()
            Return vector.Select(Function(x) CUInt(x)).ToArray
        End Function

        <Extension> Public Function AsULong(vector As Vector) As ULong()
            Return vector.Select(Function(x) CULng(x)).ToArray
        End Function

        <Extension> Public Function AsUShort(vector As Vector) As UShort()
            Return vector.Select(Function(x) CUShort(x)).ToArray
        End Function

        <Extension> Public Function AsBytes(vector As Vector) As Byte()
            Return vector.Select(Function(x) CByte(x)).ToArray
        End Function

        <Extension> Public Function AsSByte(vector As Vector) As SByte()
            Return vector.Select(Function(x) CSByte(x)).ToArray
        End Function
    End Module
End Namespace