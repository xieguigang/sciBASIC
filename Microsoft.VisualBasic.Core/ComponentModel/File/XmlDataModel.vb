#Region "Microsoft.VisualBasic::d0237d563046230683dc5139f2602070, Microsoft.VisualBasic.Core\ComponentModel\File\XmlDataModel.vb"

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

    '     Class XmlDataModel
    ' 
    '         Properties: TypeComment
    ' 
    '         Function: GetTypeReferenceComment
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports System.Web.Script.Serialization
Imports System.Xml
Imports System.Xml.Serialization

Namespace ComponentModel

    Public MustInherit Class XmlDataModel

        ''' <summary>
        ''' ReadOnly, Data model type tracking use Xml Comment.
        ''' </summary>
        ''' <returns></returns>
        '''
        <DataMember>
        <IgnoreDataMember>
        <ScriptIgnore>
        <SoapIgnore>
        <XmlAnyElement>
        Public Property TypeComment As XmlComment
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetTypeReferenceComment()
            End Get
            Set(value As XmlComment)
                ' Do Nothing
                ' 2018-6-5 this xml comment node cause bug when using xml deserialization
            End Set
        End Property

        Private Function GetTypeReferenceComment() As XmlComment
            Dim modelType As Type = Me.GetType
            Dim fullName$ = modelType.FullName
            Dim assembly$ = modelType.Assembly.FullName
            Dim trace$ = vbCrLf &
                "     model:    " & fullName & vbCrLf &
                "     assembly: " & assembly & vbCrLf &
                "  "

            Return New XmlDocument().CreateComment(trace)
        End Function
    End Class
End Namespace
