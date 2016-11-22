Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.Language

Public Module Extensions

    ''' <summary>
    ''' 使用PCC来了解各个变量之间的相关度
    ''' </summary>
    ''' <param name="df"></param>
    ''' <returns></returns>
    <Extension> Public Function Pcc(df As ODEsOut) As DataSet()
        Dim out As New List(Of DataSet)

        For Each var As NamedValue(Of Double()) In df

        Next

        Return out
    End Function

    ''' <summary>
    ''' 使用sPCC来了解各个变量之间的相关度
    ''' </summary>
    ''' <param name="df"></param>
    ''' <returns></returns>
    <Extension> Public Function SPcc(df As ODEsOut) As DataSet()
        Dim out As New List(Of DataSet)
        Return out
    End Function
End Module
