#Region "Microsoft.VisualBasic::ed5ba19c67abf11edf0e3844aef51abf, Microsoft.VisualBasic.Core\src\ComponentModel\ValuePair\TagData\LineValue.vb"

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
    '    Code Lines: 17
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 719 B


    '     Structure LineValue
    ' 
    '         Properties: line, value
    ' 
    '         Function: ToString
    ' 
    '         Sub: Assign
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.TagData

    Public Structure LineValue(Of T)
        Implements IAddress(Of Integer)
        Implements Value(Of T).IValueOf

        <XmlAttribute>
        Public Property line As Integer Implements IAddress(Of Integer).Address
        Public Property value As T Implements Value(Of T).IValueOf.Value

        Private Sub Assign(address As Integer) Implements IAddress(Of Integer).Assign
            line = address
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{line}] {Scripting.ToString(value)}"
        End Function
    End Structure
End Namespace
