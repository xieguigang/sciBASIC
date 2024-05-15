#Region "Microsoft.VisualBasic::9b735852b52c17dfef9ee31b5fb654aa, gr\Microsoft.VisualBasic.Imaging\d3js\scale\constant.vb"

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

    '   Total Lines: 36
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 992 B


    '     Class ConstantScale
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.Expressions

Namespace d3js.scale

    Public Class ConstantScale : Inherits LinearScale

        ReadOnly [const] As Double

        Default Public Overrides ReadOnly Property Value(term As String) As Double
            Get
                Return [const]
            End Get
        End Property

        Default Public Overrides ReadOnly Property Value(vector As Array) As Double()
            Get
                Return (From null In vector.AsQueryable Select [const]).ToArray
            End Get
        End Property

        Default Public Overrides ReadOnly Property Value(x As Double) As Double
            Get
                Return [const]
            End Get
        End Property

        Sub New(const_val As Double)
            [const] = const_val
        End Sub

        Public Overrides Function ToString() As String
            Return [const].ToString
        End Function

    End Class
End Namespace
