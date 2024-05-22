#Region "Microsoft.VisualBasic::a1db17a1dd6ff68dfd1cc962313bf941, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\XmlDoc\Serialization\Models\Member.vb"

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

    '   Total Lines: 101
    '    Code Lines: 61 (60.40%)
    ' Comment Lines: 23 (22.77%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (16.83%)
    '     File Size: 2.82 KB


    '     Class member
    ' 
    '         Properties: memberRef, name, param, remarks, returns
    '                     summary, typeparam
    ' 
    '         Function: ToString
    ' 
    '     Structure memberName
    ' 
    '         Properties: Name, Type
    ' 
    '         Function: RefParser, ToString
    ' 
    '     Enum memberTypes
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class param
    ' 
    '         Properties: name, text
    ' 
    '         Function: ToString
    ' 
    '     Class typeparam
    ' 
    ' 
    ' 
    '     Structure CrossReferred
    ' 
    '         Properties: cref
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ApplicationServices.Development.XmlDoc.Serialization

    Public Class member : Implements IMember

        <XmlAttribute> Public Property name As String Implements IMember.name
            Get
                Return _name
            End Get
            Set(value As String)
                _name = value
                _memberRef = memberName.RefParser(value)
            End Set
        End Property
        Public Property summary As String
        Public Property typeparam As typeparam
        Public Property param As param()
        Public Property returns As String
        Public Property remarks As String

        Dim _name As String

        Public ReadOnly Property memberRef As memberName

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Structure memberName

        Public Property Type As memberTypes
        Public Property Name As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Shared Function RefParser(name As String) As memberName
            Dim type As Char = name.First
            name = Mid(name, 3)
            Return New memberName With {
                .Name = name,
                .Type = APIExtensions.Types(type)
            }
        End Function
    End Structure

    Public Enum memberTypes
        ''' <summary>
        ''' T
        ''' </summary>
        <Description("T")> Type
        ''' <summary>
        ''' F
        ''' </summary>
        <Description("F")> Filed
        ''' <summary>
        ''' M
        ''' </summary>
        <Description("M")> Method
        ''' <summary>
        ''' P
        ''' </summary>
        <Description("P")> [Property]
        ''' <summary>
        ''' E
        ''' </summary>
        <Description("E")> [Event]
    End Enum

    Public Class param : Implements IMember

        ''' <summary>
        ''' the parameter name
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property name As String Implements IMember.name

        ''' <summary>
        ''' the parameter description text
        ''' </summary>
        ''' <returns></returns>
        <XmlText> Public Property text As String

        Public Overrides Function ToString() As String
            Return $"{name} = {text}"
        End Function
    End Class

    Public Class typeparam : Inherits param
    End Class

    Public Structure CrossReferred
        <XmlAttribute> Public Property cref As String
    End Structure
End Namespace
