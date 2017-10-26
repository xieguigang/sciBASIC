#Region "Microsoft.VisualBasic::1584278b60fd0e659a2ec194b2ad6aa9, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Reflection\Parameters\Simple.vb"

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

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace Emit.Parameters

    Partial Module ParamLogUtility

        ''' <summary>
        ''' 这个方法要求传递进来的参数的顺序要和原函数的参数的顺序一致，故而不太推荐使用这个方法
        ''' </summary>
        ''' <param name="parameters"></param>
        Public Function AcquireOrder(ParamArray parameters As Object()) As Dictionary(Of Value)
            Dim invoke As MethodBase = New StackTrace().GetFrame(1).GetMethod()
            Dim trace As New NamedValue(Of MethodBase) With {
                .Name = invoke.Name,
                .Value = invoke
            }
            Dim methodParameters = invoke.GetParameters()
            Dim out As New Dictionary(Of Value)

            For Each aMethodParameter As ParameterInfo In methodParameters
                Dim value As Object =
                    parameters(aMethodParameter.Position)
                out += New Value With {
                    .Name = aMethodParameter.Name,
                    .Trace = trace,
                    .Type = aMethodParameter.ParameterType,
                    .Value = value
                }
            Next

            Return out
        End Function
    End Module
End Namespace
