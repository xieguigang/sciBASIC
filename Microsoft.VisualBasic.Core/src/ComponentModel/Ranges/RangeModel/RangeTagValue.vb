#Region "Microsoft.VisualBasic::8a3f49b2ca0dc87c7d54e6e5185b6903, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\RangeModel\RangeTagValue.vb"

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

    '   Total Lines: 22
    '    Code Lines: 16
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 576 B


    '     Class RangeTagValue
    ' 
    '         Properties: Value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Ranges.Model

    Public Class RangeTagValue(Of T As IComparable, V) : Inherits Range(Of T)

        Public Property Value As V

        Sub New(min As T, max As T)
            Call MyBase.New(min, max)
        End Sub

        Sub New(min As T, max As T, value As V)
            MyBase.New(min, max)
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
