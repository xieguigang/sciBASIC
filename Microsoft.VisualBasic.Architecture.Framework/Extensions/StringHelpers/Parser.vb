#Region "Microsoft.VisualBasic::b5c2e29cf872044c2ce5472bfe8a8047, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\StringHelpers\Parser.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Text

''' <summary>
''' Simple type parser extension function for <see cref="String"/>
''' </summary>
Public Module PrimitiveParser

    ''' <summary>
    ''' <see cref="Integer"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ParseInteger(s As String) As Integer
        Return CInt(Val(Trim(s)))
    End Function

    ''' <summary>
    ''' <see cref="Long"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ParseLong(s As String) As Long
        Return CLng(Val(Trim(s)))
    End Function

    ''' <summary>
    ''' <see cref="Double"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ParseDouble(s As String) As Double
        Return ParseNumeric(s)
    End Function

    ''' <summary>
    ''' <see cref="Single"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ParseSingle(s As String) As Single
        Return CSng(Val(Trim(s)))
    End Function

    ''' <summary>
    ''' <see cref="Date"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ParseDate(s As String) As Date
        Return Date.Parse(Trim(s))
    End Function

    ''' <summary>
    ''' Convert the string value into the boolean value, this is useful to the text format configuration file into data model.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property BooleanValues As New SortedDictionary(Of String, Boolean) From {
 _
            {"t", True}, {"true", True},
            {"1", True},
            {"y", True}, {"yes", True}, {"ok", True},
            {"ok!", True},
            {"success", True}, {"successful", True}, {"successfully", True}, {"succeeded", True},
            {"right", True},
            {"wrong", False},
            {"failure", False}, {"failures", False},
            {"exception", False},
            {"error", False}, {"err", False},
            {"f", False}, {"false", False},
            {"0", False},
            {"n", False}, {"no", False}
        }

    ''' <summary>
    ''' Convert the string value into the boolean value, this is useful to the text format configuration file into data model.
    ''' (请注意，空值字符串为False)
    ''' </summary>
    ''' <param name="str"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("Get.Boolean")> <Extension> Public Function ParseBoolean(str$) As Boolean
        If String.IsNullOrEmpty(str) Then
            Return False
        Else
            str = str.ToLower.Trim
        End If

        If BooleanValues.ContainsKey(key:=str) Then
            Return BooleanValues(str)
        Else
#If DEBUG Then
            Call $"""{str}"" {NameOf([Boolean])} (null_value_definition)  ==> False".__DEBUG_ECHO
#End If
            Return False
        End If
    End Function

    <Extension> <ExportAPI("Get.Boolean")> Public Function ParseBoolean(ch As Char) As Boolean
        If ch = ASCII.NUL Then
            Return False
        End If

        Select Case ch
            Case "y"c, "Y"c, "t"c, "T"c, "1"c
                Return True
            Case "n"c, "N"c, "f"c, "F"c, "0"c
                Return False
        End Select

        Return True
    End Function
End Module
