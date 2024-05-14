#Region "Microsoft.VisualBasic::f29898f13d70f2a08a8edd6b54b6de3b, Microsoft.VisualBasic.Core\src\Scripting\Runtime\OverloadsFunction.vb"

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

    '   Total Lines: 67
    '    Code Lines: 46
    ' Comment Lines: 6
    '   Blank Lines: 15
    '     File Size: 2.24 KB


    '     Class OverloadsFunction
    ' 
    '         Properties: Name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Align, Match, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Linq

Namespace Scripting.Runtime

    Public Class OverloadsFunction
        Implements INamedValue

        Public Property Name As String Implements IKeyedEntity(Of String).Key

        ReadOnly functions As MethodInfo()

        Sub New(name$, methods As IEnumerable(Of MethodInfo))
            Me.Name = name
            Me.functions = methods.ToArray
        End Sub

        Public Function Match(args As Type()) As MethodInfo
            Dim alignments = functions.Select(Function(m) Align(m, args)).ToArray
            Dim p = which.Max(alignments)

            If alignments(p) <= 0 Then
                Return Nothing
            End If

            Dim method As MethodInfo = functions(p)
            Return method
        End Function

        ''' <summary>
        ''' Find the best matched overloads function based on the input parameter
        ''' </summary>
        ''' <param name="target"></param>
        ''' <param name="args">The type of the input parameter values</param>
        ''' <returns></returns>
        Public Shared Function Align(target As MethodInfo, args As Type()) As Double
            Dim params = target.GetParameters

            If args.Length > params.Length Then
                Return -1
            ElseIf params.Length = args.Length AndAlso args.Length = 0 Then
                Return 100000000
            End If

            Dim score#
            Dim tmp%

            For i As Integer = 0 To args.Length - 1
                tmp = 1000

                If Not args(i).IsInheritsFrom(params(i).ParameterType, False, tmp) Then
                    Return -1  ' 类型不符，则肯定不可以使用这个方法
                Else
                    score += (Short.MaxValue - tmp)
                End If
            Next

            Return score
        End Function

        Public Overrides Function ToString() As String
            Return $"{Name} (+{functions.Length} Overloads)"
        End Function
    End Class
End Namespace
