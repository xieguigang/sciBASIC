#Region "Microsoft.VisualBasic::7b0eb9ede464d99beff52f2f9c922782, Data\BinaryData\HDSPack\FileSystem\StreamObject.vb"

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

    '   Total Lines: 83
    '    Code Lines: 54
    ' Comment Lines: 16
    '   Blank Lines: 13
    '     File Size: 2.79 KB


    '     Class StreamObject
    ' 
    '         Properties: attributes, description, fileName, referencePath
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetAttribute, hasAttribute, hasAttributes, ToString
    ' 
    '         Sub: AddAttributes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.FileIO.Path
Imports Microsoft.VisualBasic.Linq
Imports any = Microsoft.VisualBasic.Scripting

Namespace FileSystem

    ''' <summary>
    ''' the abstract type of the file or directory object
    ''' </summary>
    Public MustInherit Class StreamObject

        Public ReadOnly Property referencePath As FilePath

        ''' <summary>
        ''' get the file basename, not full path
        ''' </summary>
        ''' <returns>file name with extension suffix</returns>
        Public ReadOnly Property fileName As String
            Get
                Return referencePath.FileName
            End Get
        End Property

        ''' <summary>
        ''' comments about this file object
        ''' </summary>
        ''' <returns></returns>
        Public Property description As String
        Public Property attributes As New LazyAttribute

        Sub New(path As FilePath)
            referencePath = path
        End Sub

        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function hasAttributes() As Boolean
            Return attributes IsNot Nothing AndAlso Not attributes.attributes.IsNullOrEmpty
        End Function

        Public Function hasAttribute(name As String) As Boolean
            If attributes IsNot Nothing AndAlso Not attributes.attributes.IsNullOrEmpty Then
                Return attributes.attributes.ContainsKey(name)
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' get attribute value by a given attribute name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function GetAttribute(name As String) As Object
            If Not attributes.attributes.ContainsKey(name) Then
                Return Nothing
            Else
                Return attributes _
                    .attributes(name) _
                    .DoCall(AddressOf LazyAttribute.GetValue)
            End If
        End Function

        Public Sub AddAttributes(attrs As Dictionary(Of String, Object))
            For Each item As KeyValuePair(Of String, Object) In attrs.SafeQuery
                If item.Key = NameOf(description) Then
                    description = any.ToString(item.Value)
                Else
                    attributes.Add(item.Key, item.Value)
                End If
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return referencePath.ToString
        End Function

    End Class
End Namespace
