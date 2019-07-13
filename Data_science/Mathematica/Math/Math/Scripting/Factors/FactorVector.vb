#Region "Microsoft.VisualBasic::b2de9d1a1f862d27f4c63d3699c80f6a, Data_science\Mathematica\Math\Math\Scripting\Factors\FactorVector.vb"

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

    '     Interface IFactorVector
    ' 
    '         Properties: index
    ' 
    '     Class FactorVector
    ' 
    '         Properties: index, Keys
    ' 
    '         Function: AsTable, GetJson, ToString, Vector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.SyntaxAPI.Vectors
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting

    Public Interface IFactorVector
        Property index As Dictionary(Of String, Integer)
    End Interface

    ''' <summary>
    ''' 提供和R之中的向量类似的行为：可以用两种方式来访问向量之中的成员，名字或者向量数组的下表
    ''' </summary>
    Public Class FactorVector(Of T) : Inherits GenericVector(Of T)
        Implements IFactorVector

        Public Property index As Dictionary(Of String, Integer) Implements IFactorVector.index

        Default Public Overloads Property Item(name$) As T
            Get
                If Not index.ContainsKey(name) Then
                    Return Nothing
                Else
                    Return buffer(index(name))
                End If
            End Get
            Set(value As T)
                If Not index.ContainsKey(name) Then
                    Call buffer.Add(value)
                    Call index.Add(name, buffer.Length - 1)
                Else
                    buffer(index(name)) = value
                End If
            End Set
        End Property

        Public ReadOnly Property Keys As IEnumerable(Of String)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return index.Keys
            End Get
        End Property

        Public Iterator Function Vector(names As IEnumerable(Of String)) As IEnumerable(Of T)
            For Each name As String In names
                Yield Me(name)
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AsTable() As Dictionary(Of String, T)
            Return index _
                .ToDictionary(Function(k, i) k,
                              Function(k, i) buffer(i))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetJson() As String
            Return AsTable.GetJson
        End Function

        Public Overrides Function ToString() As String
            Return index.Keys.ToArray.GetJson
        End Function
    End Class
End Namespace
