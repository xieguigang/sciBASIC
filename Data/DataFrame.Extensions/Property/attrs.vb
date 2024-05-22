#Region "Microsoft.VisualBasic::992dbf5d7cf823f178ffa09180917085, Data\DataFrame.Extensions\Property\attrs.vb"

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

    '   Total Lines: 52
    '    Code Lines: 33 (63.46%)
    ' Comment Lines: 9 (17.31%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 10 (19.23%)
    '     File Size: 1.93 KB


    '     Class SectionRegion
    ' 
    '         Properties: Name
    ' 
    '     Class Comment
    ' 
    '         Properties: Order, TypeInfo, Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: WriteComment
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace IO.Properties

    ''' <summary>
    ''' This attribute for separate the configuration data into another section region using the comment lines.
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field Or AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class SectionRegion : Inherits Attribute
        Public Property Name As String
    End Class

    ''' <summary>
    ''' Comment data string about this configuration item.(当前的配置数据项的在数据文件之中的注释字符串)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field Or AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class Comment : Inherits Attribute

        Public ReadOnly Property Value As String
        Public ReadOnly Property Order As Integer

        Sub New(s As String, order As Integer)
            Value = s
            order = order
        End Sub

        Public Overrides Function ToString() As String
            Return Value
        End Function

        Const LIMITED_LENGHT As Integer = 128

        Public Sub WriteComment(sBuilder As StringBuilder)
            Dim Tokens As String() = Strings.Split(Me.Value, vbCrLf)

            For Each strLine As String In Tokens
                If Len(strLine) > 100 Then
                    For i As Integer = 1 To Len(strLine) Step LIMITED_LENGHT
                        Call sBuilder.AppendLine("# " & Mid(strLine, i, LIMITED_LENGHT))
                    Next
                Else
                    Call sBuilder.AppendLine("# " & strLine)
                End If
            Next
        End Sub

        Public Shared ReadOnly Property TypeInfo As Global.System.Type = GetType(Comment)
    End Class
End Namespace
