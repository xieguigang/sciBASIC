#Region "Microsoft.VisualBasic::883efcb5ab21f62333f0b4d7c2beb12c, Microsoft.VisualBasic.Core\src\Scripting\Expressions\Aggregate.vb"

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

    '   Total Lines: 34
    '    Code Lines: 17 (50.00%)
    ' Comment Lines: 13 (38.24%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 4 (11.76%)
    '     File Size: 1.03 KB


    '     Module Aggregate
    ' 
    '         Function: GetAggregateFunction
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Scripting.Expressions

    ''' <summary>
    ''' Helper module for get lambda function by scripting text
    ''' </summary>
    Public Module Aggregate

        ''' <summary>
        ''' Get ``Aggregate`` function by term.
        ''' </summary>
        ''' <param name="name$">
        ''' + max
        ''' + min
        ''' + average
        ''' </param>
        ''' <returns></returns>
        ''' 
        <Extension> Public Function GetAggregateFunction(name$) As Func(Of IEnumerable(Of Double), Double)
            Select Case LCase(name)
                Case "max"
                    Return Function(x) x.Max
                Case "min"
                    Return Function(x) x.Min
                Case "average", "avg", "mean"
                    Return Function(x) x.Average

                Case Else
                    Throw New NotImplementedException(name)
            End Select
        End Function
    End Module
End Namespace
