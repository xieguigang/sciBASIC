#Region "Microsoft.VisualBasic::90f8facbd74f84bae14f92bfab3c0af2, Microsoft.VisualBasic.Core\src\Text\Xml\Models\Numeric.vb"

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

    '   Total Lines: 50
    '    Code Lines: 38 (76.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (24.00%)
    '     File Size: 1.34 KB


    '     Class ints
    ' 
    '         Properties: ints
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class uints
    ' 
    '         Properties: uints
    ' 
    '     Class longs
    ' 
    '         Properties: longs
    ' 
    '     Class ulongs
    ' 
    '         Properties: ulongs
    ' 
    '     Class doubles
    ' 
    '         Properties: doubles
    ' 
    '     Class floats
    ' 
    '         Properties: floats
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text.Xml.Models

    Public Class ints

        <XmlAttribute> Public Property ints As Integer()

        Sub New()
        End Sub

        Sub New(ints As IEnumerable(Of Integer))
            If ints IsNot Nothing Then
                _ints = ints.ToArray
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return ints.GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(xml As ints) As Integer()
            Return If(xml Is Nothing, Nothing, xml.ints)
        End Operator
    End Class

    Public Class uints
        <XmlAttribute> Public Property uints As UInteger()
    End Class

    Public Class longs
        <XmlAttribute> Public Property longs As Long()
    End Class

    Public Class ulongs
        <XmlAttribute> Public Property ulongs As ULong()
    End Class

    Public Class doubles
        <XmlAttribute> Public Property doubles As Double()
    End Class

    Public Class floats
        <XmlAttribute> Public Property floats As Single()
    End Class
End Namespace
