#Region "Microsoft.VisualBasic::2f89230cdb7fbf2fc0f684a49b97c167, ..\sciBASIC#\Data_science\Mathematical\ODE\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

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
        Dim vars$() = df.y.Keys.ToArray

        For Each var As NamedValue(Of Double()) In df
            Dim x As New DataSet With {
                .Identifier = var.Name,
                .Properties = New Dictionary(Of String, Double)
            }

            For Each name$ In vars
                Dim __pcc# = Correlations _
                    .GetPearson(var.Value, df.y(name).Value)
                x.Properties(name$) = __pcc
            Next

            out += x
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
        Dim vars$() = df.y.Keys.ToArray

        For Each var As NamedValue(Of Double()) In df
            Dim x As New DataSet With {
                .Identifier = var.Name,
                .Properties = New Dictionary(Of String, Double)
            }

            For Each name$ In vars
                Dim __spcc# = Correlations _
                    .Spearman(var.Value, df.y(name).Value)
                x.Properties(name$) = __spcc
            Next

            out += x
        Next

        Return out
    End Function
End Module
