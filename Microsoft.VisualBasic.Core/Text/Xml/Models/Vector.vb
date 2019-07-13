#Region "Microsoft.VisualBasic::4f0214ae618650f3a99c95a879d49d42, Microsoft.VisualBasic.Core\Text\Xml\Models\Vector.vb"

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

    '     Class NumericVector
    ' 
    '         Properties: Length, name, vector
    ' 
    '         Function: GenericEnumerator, GetEnumerator, SequenceEqual, ToString
    ' 
    '     Class TermsVector
    ' 
    '         Properties: terms
    ' 
    '         Function: GenericEnumerator, GetEnumerator, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text.Xml.Models

    ''' <summary>
    ''' A <see cref="Double"/> type numeric sequence container
    ''' </summary>
    <XmlType("numerics")>
    Public Class NumericVector : Implements Enumeration(Of Double)

        ''' <summary>
        ''' 可以用这个属性来简单的标记这个向量所属的对象名称
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property name As String
        ''' <summary>
        ''' 存储于XML文档之中的数据向量
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property vector As Double()

        ''' <summary>
        ''' Get/Set Element ``Xi``
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Default Public Property Xi(i As Integer) As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return vector(i)
            End Get
            Set(value As Double)
                vector(i) = value
            End Set
        End Property

        ''' <summary>
        ''' The vector length for counting the elements in <see cref="Vector"/> property.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return CInt(vector?.Length)
            End Get
        End Property

        Public Overrides Function ToString() As String
            If name.StringEmpty Then
                Return vector.GetJson
            Else
                Return $"Dim {name} As Vector = {vector.GetJson}"
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(v As Double()) As NumericVector
            Return New NumericVector With {.name = "NULL", .vector = v}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(v As Integer()) As NumericVector
            Return New NumericVector With {
                .name = "NULL",
                .vector = v.Select(Function(x) CDbl(x)).ToArray
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(v As NumericVector) As Double()
            Return v.vector
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SequenceEqual(input() As Double) As Boolean
            Return vector.SequenceEqual(input)
        End Function

        Public Function GenericEnumerator() As IEnumerator(Of Double) Implements Enumeration(Of Double).GenericEnumerator
            Return vector.AsEnumerable.GetEnumerator
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements Enumeration(Of Double).GetEnumerator
            Yield GenericEnumerator()
        End Function
    End Class

    Public Class TermsVector : Implements Enumeration(Of Double)

        <XmlAttribute>
        Public Property terms As String()

        Public Overrides Function ToString() As String
            Return terms.GetJson
        End Function

        Public Function GenericEnumerator() As IEnumerator(Of Double) Implements Enumeration(Of Double).GenericEnumerator
            Return terms.AsEnumerable.GetEnumerator
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements Enumeration(Of Double).GetEnumerator
            Yield GenericEnumerator()
        End Function
    End Class
End Namespace
