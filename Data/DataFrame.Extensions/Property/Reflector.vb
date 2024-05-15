#Region "Microsoft.VisualBasic::a23303fe05719389b64f6902e4051330, Data\DataFrame.Extensions\Property\Reflector.vb"

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

    '   Total Lines: 115
    '    Code Lines: 85
    ' Comment Lines: 14
    '   Blank Lines: 16
    '     File Size: 5.13 KB


    '     Module Reflector
    ' 
    '         Function: (+2 Overloads) FillObject, GetDoc, LoadConfiguration, LoadPropertyFile, ToConfigDoc
    ' 
    '         Sub: __writeComments
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.InputHandler
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace IO.Properties

    <Package("Java.IO.Properties")>
    Public Module Reflector

        <ExportAPI("Read.Properties")>
        <Extension> Public Function LoadPropertyFile(File As String) As Properties
            Return Properties.Load(File)
        End Function

        ''' <summary>
        ''' If the target file <paramref name="file"></paramref> is not exists on the filesystem, then function will return a null object.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="file"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function LoadConfiguration(Of T As Class)(file As String, Optional getDefault As Func(Of String, T) = Nothing) As T
            If Not file.FileExists Then
                If Not getDefault Is Nothing Then
                    Return getDefault(file)
                Else
                    Return Nothing
                End If
            End If

            Return Properties.Load(file).FillObject(Of T)()
        End Function

        <Extension> Public Function FillObject(Of T As Class)(propertiesDoc As Properties) As T
            Return DirectCast(FillObject(propertiesDoc, GetType(T)), T)
        End Function

        <ExportAPI("FillObject")>
        Public Function FillObject(propertiesDoc As Properties, type As Type) As Object
            Dim Properties = (From p As PropertyInfo
                              In type.GetReadWriteProperties
                              Where (p.PropertyType.IsClass AndAlso p.PropertyType = GetType(String)) OrElse
                                  p.PropertyType.IsValueType
                              Select p).ToArray
            Dim filledObject As Object = Activator.CreateInstance(type)

            For Each [Property] As PropertyInfo In Properties
                If CasterString.ContainsKey([Property].PropertyType) Then
                    Dim TypeCastMethod = CasterString([Property].PropertyType)
                    Dim value = TypeCastMethod(propertiesDoc.getProperty([Property].Name))
                    Call [Property].SetValue(filledObject, value)
                End If
            Next

            Return filledObject
        End Function

        <ExportAPI("Get.Doc")>
        Public Function GetDoc(obj As Object, Optional type As Value(Of Type) = Nothing) As String
            Dim readWrites = If(type Is Nothing, type = obj.GetType, +type).GetReadWriteProperties
            Dim Properties = (From p As PropertyInfo
                              In readWrites
                              Where (p.PropertyType.IsClass AndAlso p.PropertyType = GetType(String)) OrElse
                                  p.PropertyType.IsValueType
                              Select p).ToArray
            Dim propBuilder As StringBuilder = New StringBuilder(1024)
            Dim typeComments = (+type).GetCustomAttributes(Comment.TypeInfo, True)
            If Not typeComments.IsNullOrEmpty Then
                Call __writeComments(typeComments, propBuilder)
                Call propBuilder.AppendLine()
            End If

            For Each prop As PropertyInfo In Properties
                Dim value As Object = prop.GetValue(obj)
                Dim comments = prop.GetCustomAttributes(Comment.TypeInfo, True)

                If Not comments.IsNullOrEmpty Then
                    Call __writeComments(comments, propBuilder)
                End If

                Call propBuilder.AppendLine($"{prop.Name}={Scripting.ToString(value)}")
                Call propBuilder.AppendLine()
            Next

            Return propBuilder.ToString
        End Function

        ''' <summary>
        ''' 从一个配置文件对象之中生成一个配置文件文本数据
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function ToConfigDoc(Of T As Class)(obj As T) As String
            Dim typeInfo As Type = GetType(T)
            Return GetDoc(obj, typeInfo)
        End Function

        Private Sub __writeComments(comments As Object(), PropertyData As StringBuilder)
            For Each CommentValue In (From x As Object In comments
                                      Let value As Comment = DirectCast(x, Comment)
                                      Select value
                                      Order By value.Order Ascending)
                Call CommentValue.WriteComment(PropertyData)
                Call PropertyData.AppendLine()
            Next
            Call PropertyData.Remove(PropertyData.Length - 2, 2)
        End Sub
    End Module
End Namespace
