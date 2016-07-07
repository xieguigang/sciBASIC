#Region "Microsoft.VisualBasic::28c5f98ac1b9d30a0be50776aec5114a, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\Tools\SoftwareToolkits\ChangeLog.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Text

Namespace SoftwareToolkits

#If NET_40 = 0 Then

    ''' <summary>
    ''' Tools for generate the program change log document.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ChangeLog : Inherits Microsoft.VisualBasic.ComponentModel.ITextFile

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Path">ChangeLog的文件路径</param>
        ''' <param name="ApplyOn">目标程序的主程序的文件路径</param>
        ''' <remarks></remarks>
        Sub New(Path As String, ApplyOn As String)
            Me.FilePath = Path
            Dim Assembly As System.Reflection.Assembly = System.Reflection.Assembly.LoadFrom(FileIO.FileSystem.GetFileInfo(ApplyOn).FullName)
            Dim Properties = Assembly.CustomAttributes.ToArray

        End Sub

        Sub New()

        End Sub

        Public Class UpdateInformation

            Public Property Version As Version
            Public Property VerStatus As String
            Public Property Changes As String()
            Public Property UpdateTime As String

            Public Overrides Function ToString() As String
                Dim sBuilder As StringBuilder = New StringBuilder(1024)
                Call sBuilder.AppendLine(New String("="c, 200))
                Call sBuilder.AppendLine()
                Call sBuilder.AppendLine(String.Format("Version {0} {1} ({2})", Version.ToString, If(String.IsNullOrEmpty(VerStatus), "", String.Format("{0} ", VerStatus)), UpdateTime))
                Call sBuilder.AppendLine()
                Call sBuilder.AppendLine(New String("-"c, 100))
                Call sBuilder.AppendLine()

                If Changes.IsNullOrEmpty Then
                    Call sBuilder.AppendLine("  No updates description.")
                Else
                    For i As Integer = 0 To Changes.Length - 1
                        Call sBuilder.AppendLine(String.Format("  {0}. {1}", i, Changes(i)))
                    Next
                End If

                Call sBuilder.AppendLine()

                Return sBuilder.ToString
            End Function

            Public Shared Function CreateObject(strValue As String) As UpdateInformation
                Throw New NotImplementedException
            End Function
        End Class

        Dim _UpdateList As List(Of UpdateInformation) = New List(Of UpdateInformation)
        Dim _Company As String

        Public Property SoftwareName As String
        Public Property SoftwareDescription As String
        Public Property Company As String
            Get
                Return String.Format("Copyright © {0} {1}", _Company, Now.Year)
            End Get
            Set(value As String)
                _Company = value
            End Set
        End Property
        Public Property SoftwareAuthors As List(Of String)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Changes"></param>
        ''' <param name="version">假若为空的话，会自动的根据上一次版本的号码叠加1</param>
        ''' <remarks></remarks>
        Public Sub AppendChangeInformation(Changes As Generic.IEnumerable(Of String), Optional version As Version = Nothing, Optional Status As String = "")
            Dim UpdateRecord As UpdateInformation = New UpdateInformation With {.UpdateTime = Now.ToString, .Changes = Changes.ToArray, .VerStatus = Status}
            If version Is Nothing Then
                If _UpdateList.IsNullOrEmpty Then
                    version = New Version
                Else
                    Dim PreviousVersion = _UpdateList.Last.Version
                    version = New Version(major:=PreviousVersion.Major, build:=PreviousVersion.Build, minor:=PreviousVersion.Minor, revision:=PreviousVersion.Revision + 1)
                End If
            End If

            UpdateRecord.Version = version

            Call _UpdateList.Add(UpdateRecord)
        End Sub

        Public Function GenerateDocument() As String
            Dim sBuilder As StringBuilder = New StringBuilder(1024)
            Call sBuilder.AppendLine(SoftwareName)
            Call sBuilder.AppendLine(SoftwareDescription)
            Call sBuilder.AppendLine(Company)
            Call sBuilder.AppendLine(String.Format("Authors: {0}", String.Join(";" & vbCrLf & "        ", SoftwareAuthors.ToArray)))
            Call sBuilder.AppendLine()
            Call sBuilder.AppendLine("---------------------------------Release Notes--------------------------------------")
            Call sBuilder.AppendLine()
            Call sBuilder.AppendLine()

            For Each item In _UpdateList
                Call sBuilder.AppendLine(item.ToString)
            Next

            Return sBuilder.ToString
        End Function

        Public Shared Function LoadDocument(Path As String) As List(Of ChangeLog.UpdateInformation)
            Throw New NotImplementedException
        End Function

        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As System.Text.Encoding = Nothing) As Boolean
            FilePath = MyBase.getPath(FilePath)
            Return GenerateDocument.SaveTo(FilePath, Encoding)
        End Function

        Protected Overrides Function __getDefaultPath() As String
            Return FilePath
        End Function
    End Class

#End If
End Namespace
