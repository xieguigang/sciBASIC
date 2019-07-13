#Region "Microsoft.VisualBasic::0f0a45ead835aa53842a98132971d378, Data_science\Mathematica\Math\Math\uncheckInteger.vb"

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

    ' Module UncheckIntegerExtensions
    ' 
    '     Function: unchecked, uncheckedInteger, uncheckedLong, uncheckedULong
    ' 
    ' Structure UncheckedInteger
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: ToString
    '     Operators: (+2 Overloads) -, (+2 Overloads) *, (+2 Overloads) /, (+2 Overloads) ^, (+2 Overloads) +
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' unchecked arithmetic
''' </summary>
Public Module UncheckIntegerExtensions

    Public Function unchecked(u&) As BigInteger
        Return New BigInteger(u)
    End Function

    <Extension>
    Public Function uncheckedULong(bytes As BigInteger) As ULong
        Dim data As Byte() = bytes.ToByteArray
        If data.Length < 8 Then
            data = data.Join({0, 0, 0, 0, 0, 0, 0, 0}).ToArray
        End If
        Return BitConverter.ToUInt64(data, Scan0)
    End Function

    <Extension>
    Public Function uncheckedLong(bytes As BigInteger) As Long
        Dim data As Byte() = bytes.ToByteArray
        If data.Length < 8 Then
            data = data.Join({0, 0, 0, 0, 0, 0, 0, 0}).ToArray
        End If
        Return BitConverter.ToInt64(data, Scan0)
    End Function

    <Extension>
    Public Function uncheckedInteger(bytes As BigInteger) As Integer
        Return BitConverter.ToInt32(bytes.ToByteArray, Scan0)
    End Function
End Module

Public Structure UncheckedInteger

    Dim Value As BigInteger

    Sub New(i%)
        Value = New BigInteger(i)
    End Sub

    Sub New(l&)
        Value = New BigInteger(l)
    End Sub

    Sub New(s As Short)
        Value = New BigInteger(s)
    End Sub

    Public Overrides Function ToString() As String
        Return Value.ToString
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator +(unchecked As UncheckedInteger, i%) As UncheckedInteger
        Return New UncheckedInteger((unchecked.Value + i).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator +(i%, unchecked As UncheckedInteger) As UncheckedInteger
        Return New UncheckedInteger((unchecked.Value + i).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator -(unchecked As UncheckedInteger, i%) As UncheckedInteger
        Return New UncheckedInteger((unchecked.Value - i).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator -(i%, unchecked As UncheckedInteger) As UncheckedInteger
        Return New UncheckedInteger((i - unchecked.Value).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator *(unchecked As UncheckedInteger, i%) As UncheckedInteger
        Return New UncheckedInteger((unchecked.Value * i).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator *(i%, unchecked As UncheckedInteger) As UncheckedInteger
        Return New UncheckedInteger((i * unchecked.Value).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator /(unchecked As UncheckedInteger, i%) As UncheckedInteger
        Return New UncheckedInteger((unchecked.Value / i).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator /(i%, unchecked As UncheckedInteger) As UncheckedInteger
        Return New UncheckedInteger((i / unchecked.Value).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator ^(unchecked As UncheckedInteger, i%) As UncheckedInteger
        Return New UncheckedInteger(BigInteger.Pow(unchecked.Value, i).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator ^(i%, unchecked As UncheckedInteger) As UncheckedInteger
        Return New UncheckedInteger(BigInteger.Pow(i, unchecked.Value).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(unchecked As UncheckedInteger) As Integer
        Return unchecked.Value.uncheckedInteger
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(unchecked As UncheckedInteger) As Long
        Return unchecked.Value.uncheckedLong
    End Operator

End Structure
