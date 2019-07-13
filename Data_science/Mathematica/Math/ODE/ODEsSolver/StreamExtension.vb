#Region "Microsoft.VisualBasic::f013ae86d5b50e91d48b6a4063109bb3, Data_science\Mathematica\Math\ODE\ODEsSolver\StreamExtension.vb"

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

    ' Module StreamExtension
    ' 
    '     Function: merge, Merge, mergeParameters, mergeY0
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq

<HideModuleName>
Public Module StreamExtension

    ReadOnly average As New [Default](Of Func(Of IEnumerable(Of Double), Double))(AddressOf Enumerable.Average)

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="source">这些对象的X的尺度和范围必须都是一致的</param>
    ''' <param name="method">Default is <see cref="Enumerable.Average"/>.(默认是平均值)</param>
    ''' <returns></returns>
    <Extension>
    Public Function Merge(source As IEnumerable(Of ODEsOut), Optional method As Func(Of IEnumerable(Of Double), Double) = Nothing) As ODEsOut
        Return source.ToArray.merge(method Or average)
    End Function

    <Extension>
    Private Function mergeParameters(data As ODEsOut(), aggregate As Func(Of IEnumerable(Of Double), Double)) As Dictionary(Of String, Double)
        Dim params As New Dictionary(Of String, Double)
        Dim value As Double

        Try
            For Each k As String In data(0).params.Keys
                value = data.Select(Function(o) o.params(k)).DoCall(aggregate)
                params.Add(k, value)
            Next
        Catch ex As Exception

        End Try

        Return params
    End Function

    <Extension>
    Private Function mergeY0(data As ODEsOut(), aggregate As Func(Of IEnumerable(Of Double), Double)) As Dictionary(Of String, Double)
        Dim y0 As New Dictionary(Of String, Double)
        Dim value As Double

        Try
            For Each k In data(0).y0.Keys
                value = data.Select(Function(o) o.y0(k)).DoCall(aggregate)
                y0.Add(k, value)
            Next
        Catch ex As Exception

        End Try

        Return y0
    End Function

    <Extension>
    Private Function merge(data As ODEsOut(), aggregate As Func(Of IEnumerable(Of Double), Double))
        Dim minLen% = data.Min(Function(x) x.x.Length)
        Dim vars As String() = data.First.y.Keys.ToArray
        Dim y As New Dictionary(Of NamedCollection(Of Double))
        Dim params As Dictionary(Of String, Double) = data.mergeParameters(aggregate)
        Dim y0 As Dictionary(Of String, Double) = data.mergeY0(aggregate)
        Dim value As Double

        For Each k In vars
            y += New NamedCollection(Of Double) With {
                .Name = k,
                .value = New Double(minLen) {}
            }
        Next

        For i As Integer = 0 To minLen - 1
            Dim index As Integer = i

            For Each k As String In vars
                value = data _
                    .Select(Function(v) v.y(k).Value(index)) _
                    .DoCall(aggregate)

                y(k).Value(i) = value
            Next
        Next

        Return New ODEsOut With {
            .y = y,
            .params = params,
            .x = data(0).x,
            .y0 = y0
        }
    End Function

End Module
