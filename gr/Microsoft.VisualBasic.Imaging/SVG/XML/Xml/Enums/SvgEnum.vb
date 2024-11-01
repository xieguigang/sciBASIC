#Region "Microsoft.VisualBasic::416458e2926982c7c47999fa77f6a550, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Xml\Enums\SvgEnum.vb"

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

'   Total Lines: 42
'    Code Lines: 26 (61.90%)
' Comment Lines: 7 (16.67%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 9 (21.43%)
'     File Size: 1.36 KB


'     Class SvgEnum
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Equals, GetHashCode, ToString
' 
' 
' /********************************************************************************/

#End Region


Namespace SVG.XML.Enums

    ''' <summary>
    ''' a string constant value
    ''' </summary>
    Public MustInherit Class SvgEnum

        ReadOnly _value As String

        Protected Sub New(value As String)
            _value = value
        End Sub

        Public Overrides Function ToString() As String
            Return _value
        End Function

        Public Shared Widening Operator CType(value As SvgEnum) As String
            Return value.ToString()
        End Operator

        Public Overrides Function Equals(obj As Object) As Boolean
            If obj Is Nothing Then Return False
            If Me.GetType() IsNot obj.GetType() Then Return False

            Return _value = DirectCast(obj, SvgEnum)._value
        End Function

        ''' <summary>
        ''' the hashcode is related to the constant string value
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetHashCode() As Integer
            Dim hashCode = -862051595
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(_value)
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode([GetType]().Name)
            Return hashCode
        End Function
    End Class
End Namespace
