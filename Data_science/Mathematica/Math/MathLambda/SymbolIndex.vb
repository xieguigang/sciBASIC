#Region "Microsoft.VisualBasic::75a38d2ae37213a442bae18c4847af09, Data_science\Mathematica\Math\MathLambda\SymbolIndex.vb"

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

    '   Total Lines: 46
    '    Code Lines: 36 (78.26%)
    ' Comment Lines: 3 (6.52%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (15.22%)
    '     File Size: 1.57 KB


    ' Class SymbolIndex
    ' 
    '     Properties: Alignments
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: FromLambda, GetParameterAlignment
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.application.xml

''' <summary>
''' the parameter symbol index of the lambda function
''' </summary>
Public Class SymbolIndex

    Dim m_index As Dictionary(Of String, ParameterExpression)
    Dim m_alignments As ParameterExpression()

    Default Public ReadOnly Property GetArgument(name As String) As ParameterExpression
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return m_index(name)
        End Get
    End Property

    Public ReadOnly Property Alignments As ParameterExpression()
        Get
            Return m_alignments.ToArray
        End Get
    End Property

    Private Sub New()
    End Sub

    Private Function GetParameterAlignment(lambda As MathML.LambdaExpression) As IEnumerable(Of ParameterExpression)
        Return From par As String
               In lambda.parameters
               Select m_index(par)
    End Function

    Public Shared Function FromLambda(lambda As MathML.LambdaExpression) As SymbolIndex
        Return New SymbolIndex With {
            .m_index = lambda.parameters _
                .Select(Function(name) Expression.Parameter(GetType(Double), name)) _
                .ToDictionary(Function(par)
                                  Return par.Name
                              End Function),
            .m_alignments = .GetParameterAlignment(lambda) _
                            .ToArray
        }
    End Function
End Class
