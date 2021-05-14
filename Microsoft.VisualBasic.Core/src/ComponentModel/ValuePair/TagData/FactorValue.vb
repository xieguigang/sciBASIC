#Region "Microsoft.VisualBasic::b29d4a98f728cba6bfb1a056571876c2, Microsoft.VisualBasic.Core\src\ComponentModel\ValuePair\TagData\FactorValue.vb"

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

    '     Class FactorValue
    ' 
    '         Properties: factor, result
    ' 
    '     Class FactorString
    ' 
    '         Properties: factor, text
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.TagData

    Public Class FactorValue(Of T As {Structure, IComparable(Of T)}, V)

        Public Property factor As T
        Public Property result As V

#If NET_48 Or netcore5 = 1 Then

        Public Shared Widening Operator CType(value As (factor As T, result As V)) As FactorValue(Of T, V)
            Return New FactorValue(Of T, V) With {
                .factor = value.factor,
                .result = value.result
            }
        End Operator
#End If
    End Class

    Public Class FactorString(Of T As {Structure, IComparable(Of T)})

        Public Property factor As T
        Public Property text As String

        Public Overrides Function ToString() As String
            Return $"Dim {text} As {GetType(T).FullName} = {factor.GetJson}"
        End Function
    End Class
End Namespace
