#Region "Microsoft.VisualBasic::1eddead2ce4775213838fe66a845fc35, Microsoft.VisualBasic.Core\src\Data\Trinity\Word.vb"

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

    '   Total Lines: 58
    '    Code Lines: 33 (56.90%)
    ' Comment Lines: 15 (25.86%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (17.24%)
    '     File Size: 1.76 KB


    '     Class Word
    ' 
    '         Properties: [class], num, str
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: StartsWith, ToLower, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language

Namespace Data.Trinity.NLP

    ''' <summary>
    ''' A single word text token
    ''' </summary>
    Public Class Word : Implements Value(Of String).IValueOf

        ''' <summary>
        ''' the word class type
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property [class] As WordClass

        ''' <summary>
        ''' the word text
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property str As String Implements Value(Of String).IValueOf.Value

        ''' <summary>
        ''' the reference count of current word
        ''' </summary>
        ''' <returns></returns>
        Public Property num As Integer

        Public Sub New(s As String)
            Me.str = s
            Me.num = 1
        End Sub

        Public Overrides Function ToString() As String
            If [Class] = WordClass.NA Then
                Return str
            Else
                Return $"{[class].Description} {str}"
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function StartsWith(token As String) As Boolean
            Return str.StartsWith(token)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToLower() As String
            Return Strings.LCase(str)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(w As Word) As String
            Return w.str
        End Operator
    End Class
End Namespace
