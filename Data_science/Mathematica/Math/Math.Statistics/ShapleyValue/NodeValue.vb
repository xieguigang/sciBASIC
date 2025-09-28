#Region "Microsoft.VisualBasic::f67e7ba642e89dac74b0599ad693db89, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\NodeValue.vb"

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
    '    Code Lines: 26 (72.22%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (27.78%)
    '     File Size: 1.10 KB


    '     Class NodeValue
    ' 
    '         Properties: NextValues, Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: updateValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ShapleyValue

    Public Class NodeValue

        Dim nextValuesList As IList(Of Integer)

        Public Overridable ReadOnly Property Value As Integer

        Public Overridable ReadOnly Property NextValues As IList(Of Integer)
            Get
                Return New List(Of Integer)(nextValuesList)
            End Get
        End Property

        Public Sub New(nextValues As IList(Of Integer))
            nextValuesList = New List(Of Integer)()
            CType(nextValuesList, List(Of Integer)).AddRange(nextValues)
            Value = nextValues(0)
            nextValuesList.RemoveAt(Value)
        End Sub

        Public Overridable Sub updateValue()
            If nextValuesList.Count > 0 Then
                _Value = nextValuesList(0)
            End If

            nextValuesList.RemoveAt(Value)
        End Sub

        Public Overrides Function ToString() As String
            Return "NodeValue [value=" & Value.ToString() & ", nextValues=" & nextValuesList.ToString() & "]"
        End Function

    End Class

End Namespace

