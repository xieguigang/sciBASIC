#Region "Microsoft.VisualBasic::7b8b26eba820a42da629f6bd3a819fbe, Data_science\Mathematica\Math\ODE\ODEsSolver\StreamExtension.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    ' Module StreamExtension
    ' 
    '     Function: __getArgs, DataFrame, LoadFromDataFrame, Merge
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

<HideModuleName>
Public Module StreamExtension

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="source">这些对象的X的尺度和范围必须都是一致的</param>
    ''' <param name="method">默认是平均值</param>
    ''' <returns></returns>
    <Extension>
    Public Function Merge(source As IEnumerable(Of ODEsOut), Optional method As Func(Of IEnumerable(Of Double), Double) = Nothing) As ODEsOut
        Dim data = source.ToArray
        Dim minLen% = data.Min(Function(x) x.x.Length)
        Dim vars = data.First.y.Keys
        Dim y As New Dictionary(Of NamedCollection(Of Double))
        Dim params As New Dictionary(Of String, Double)
        Dim y0 As New Dictionary(Of String, Double)

        If method Is Nothing Then
            method = AddressOf Enumerable.Average
        End If

        Try
            For Each k In data(0).params.Keys
                params.Add(k, method(data.Select(Function(o) o.params(k))))
            Next
        Catch ex As Exception

        End Try

        Try
            For Each k In data(0).y0.Keys
                y0.Add(k, method(data.Select(Function(o) o.y0(k))))
            Next
        Catch ex As Exception

        End Try

        For Each k In vars
            y += New NamedCollection(Of Double) With {
                .Name = k,
                .Value = New Double(minLen) {}
            }
        Next

        For i As Integer = 0 To minLen - 1
            Dim index As Integer = i

            For Each k As String In vars
                y(k).Value(i) = method(
                    data.Select(Function(v) v.y(k).Value(index)))
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
