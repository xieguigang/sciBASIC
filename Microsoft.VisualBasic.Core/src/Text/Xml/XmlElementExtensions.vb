#Region "Microsoft.VisualBasic::5096b4669c88d337625e36c58a016b44, Microsoft.VisualBasic.Core\src\Text\Xml\XmlElementExtensions.vb"

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

    '   Total Lines: 44
    '    Code Lines: 38
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.62 KB


    '     Module XmlElementExtensions
    ' 
    '         Function: (+2 Overloads) GetAttribute
    ' 
    '         Sub: (+2 Overloads) SetAttribute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Xml

Namespace Text.Xml

    <HideModuleName>
    Public Module XmlElementExtensions

        <Extension()>
        Public Sub SetAttribute(element As XmlElement, name As String, value As Double)
            element.SetAttribute(name, value.ToString(CultureInfo.InvariantCulture))
        End Sub

        <Extension()>
        Public Sub SetAttribute(element As XmlElement, name As String, value As Integer?)
            If value.HasValue Then
                element.SetAttribute(name, value.ToString())
            Else
                element.RemoveAttribute(name)
            End If
        End Sub

        <Extension()>
        Public Function GetAttribute(Of T)(element As XmlElement, name As String, defaultValue As T) As T
            If Not element.HasAttribute(name) Then Return defaultValue
            Try
                Return Convert.ChangeType(element.GetAttribute(name), GetType(T), CultureInfo.InvariantCulture)
            Catch
                Return defaultValue
            End Try
        End Function

        <Extension>
        Public Function GetAttribute(Of T)(element As XmlElement, name As String, ns As String, [default] As T) As T
            If Not element.HasAttribute(name, ns) Then Return [default]
            Try
                Return Convert.ChangeType(element.GetAttribute(name, ns), GetType(T), CultureInfo.InvariantCulture)
            Catch
                Return [default]
            End Try
        End Function
    End Module
End Namespace
