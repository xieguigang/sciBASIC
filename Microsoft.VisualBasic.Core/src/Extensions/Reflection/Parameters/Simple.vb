#Region "Microsoft.VisualBasic::07f5fac2f3041e7e554f2117d54e25ca, Microsoft.VisualBasic.Core\src\Extensions\Reflection\Parameters\Simple.vb"

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


    ' Code Statistics:

    '   Total Lines: 37
    '    Code Lines: 28 (75.68%)
    ' Comment Lines: 4 (10.81%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (13.51%)
    '     File Size: 1.44 KB


    '     Module ParamLogUtility
    ' 
    '         Function: AcquireOrder
    ' 
    ' 
    ' /********************************************************************************/

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
