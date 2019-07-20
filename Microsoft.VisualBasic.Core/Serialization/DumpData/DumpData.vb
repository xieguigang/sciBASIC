#Region "Microsoft.VisualBasic::9004d0f648faadf51b3390b67a180512, Serialization\DumpData\DumpData.vb"

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

    '     Module MemoryDump
    ' 
    '         Function: __dumpInvoke, DumpArray, DumpFieldCollection, DumpPropertyCollection, DumpPropertyOrField
    '                   GetArray, I_CreateDump, IsGenericEnumerable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Runtime.CompilerServices

Namespace Serialization

    Public Module MemoryDump

        <Extension> Public Function I_CreateDump(Of T As Class)(obj As T, Optional DumpLevel As UInteger = 0) As String
            Dim DumpBuilder As StringBuilder = New StringBuilder(1024)

            Call Console.WriteLine("Create memory dump for {0}.", obj.GetType.FullName)

            Call DumpBuilder.AppendLine("//                                                                ")
            Call DumpBuilder.AppendLine(String.Format("//  Microsoft (R) VisualBasic.NET Memory Dump Creator.  Version {0}", Application.ProductVersion))
            Call DumpBuilder.AppendLine("//  Copyright (c) Microsoft Corporation.  All rights reserved.")
            Call DumpBuilder.AppendLine("//                                                                ")
            Call DumpBuilder.AppendLine(String.Format("//  Dump Time {0}  ", Now.ToString))
            Call DumpBuilder.AppendLine("//                                                                ")

            Call DumpBuilder.AppendLine(__dumpInvoke(obj, DumpLevel))
            Return DumpBuilder.ToString
        End Function

        ''' <summary>
        ''' Create memory dump for a class object instance
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="DumpLevel"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function __dumpInvoke(obj As Object, DumpLevel As Integer) As String
            Dim DumpBuilder As StringBuilder = New StringBuilder(1024)
            Dim Type As System.Type = obj.GetType
            Dim LevelBlanks As String = New String(vbTab, DumpLevel)
            Dim PropertyCollection As System.Reflection.PropertyInfo() = (From [property] As System.Reflection.PropertyInfo
                                                                      In Type.GetProperties(System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.Public)
                                                                          Let attrs As Object() = [property].GetCustomAttributes(attributeType:=DumpNode.GetTypeId, inherit:=True)
                                                                          Where Not attrs.IsNullOrEmpty
                                                                          Select [property]).ToArray
            Dim FieldsCollection As System.Reflection.FieldInfo() = (From Field As System.Reflection.FieldInfo
                                                                 In Type.GetFields(System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.Public)
                                                                     Let attrs As Object() = Field.GetCustomAttributes(attributeType:=DumpNode.GetTypeId, inherit:=True)
                                                                     Where Not attrs.IsNullOrEmpty
                                                                     Select Field).ToArray

            If PropertyCollection.IsNullOrEmpty AndAlso FieldsCollection.IsNullOrEmpty Then
                Return ""
            End If

            Call DumpBuilder.AppendLine()
            Call DumpBuilder.AppendLine(String.Format("{0}/* =============== DUMP_CLASS_TYPE {1} =================== */" & vbCrLf, LevelBlanks, Type.FullName))
            Call DumpBuilder.AppendLine(String.Format("{0}{1}", LevelBlanks, Type.FullName))
            Call DumpBuilder.AppendLine(LevelBlanks & "{")

            Call DumpBuilder.AppendLine(DumpFieldCollection(obj, FieldsCollection, DumpLevel))
            Call DumpBuilder.AppendLine(DumpPropertyCollection(obj, PropertyCollection, DumpLevel))

            Call DumpBuilder.AppendLine(String.Format("{0}! // end of dump {1}", LevelBlanks, Type.FullName).Replace("!", "}"))

            Return DumpBuilder.ToString
        End Function

        Private Function DumpFieldCollection(obj As Object, FieldCollection As System.Reflection.FieldInfo(), DumpLevel As UInteger) As String
            Dim DumpBuilder As StringBuilder = New StringBuilder(1024)
            Dim LevelBlanks As String = New String(vbTab, DumpLevel + 1)

            If Not FieldCollection.IsNullOrEmpty Then Call DumpBuilder.AppendLine(LevelBlanks & "// class type fields" & vbCrLf)

            For Each Field As System.Reflection.FieldInfo In FieldCollection
                Call DumpBuilder.AppendLine(DumpPropertyOrField(Field.GetValue(obj), Field.FieldType, Field.Name, DumpLevel, "field"))
            Next

            Return DumpBuilder.ToString
        End Function

        Private Function DumpPropertyCollection(obj As Object, PropertyCollection As System.Reflection.PropertyInfo(), DumpLevel As UInteger) As String
            Dim DumpBuilder As StringBuilder = New StringBuilder(1024)
            Dim LevelBlanks As String = New String(vbTab, DumpLevel + 1)

            If Not PropertyCollection.IsNullOrEmpty Then Call DumpBuilder.AppendLine(LevelBlanks & "// class type properties" & vbCrLf)

            For Each [property] As System.Reflection.PropertyInfo In PropertyCollection
                Call DumpBuilder.AppendLine(DumpPropertyOrField([property].GetValue(obj, Nothing), [property].PropertyType, [property].Name, DumpLevel, "property"))
            Next

            Return DumpBuilder.ToString
        End Function

        Private Function DumpArray(ElementType As System.Type, ArrayTitle As String, ArrayData As Object(), DumpLevel As Integer) As String
            Dim DumpBuilder As StringBuilder = New StringBuilder(1024) : DumpLevel += 1
            Dim LevelBlanks As String = New String(vbTab, DumpLevel)
            Dim ElementIsArrayType As Boolean = ElementType.IsArray

            If ElementIsArrayType Then
                For i As Integer = 0 To ArrayData.Length - 1
                    Dim ArrayItem As Object() = GetArray(DirectCast(ArrayData(i), IEnumerable))
                    Dim ItemTitle As String = String.Format("{0}{1}[{2},] -->" & vbCrLf, LevelBlanks, ArrayTitle, i)

                    Call DumpBuilder.AppendLine(ItemTitle)

                    If ArrayItem.IsNullOrEmpty Then
                        Call DumpBuilder.AppendLine(New String(vbTab, DumpLevel + 1) & "null inner array")
                    Else
                        ElementType = ArrayItem.First.GetType
                        Call DumpBuilder.AppendLine(DumpArray(ElementType, ItemTitle, ArrayItem, DumpLevel))
                    End If
                Next
            ElseIf ElementType.IsClass AndAlso ElementType IsNot GetType(String) Then
                For i As Integer = 0 To ArrayData.Length - 1
                    Dim item = ArrayData(i)
                    Call DumpBuilder.AppendFormat("{0}[{1}] --> {2} ", LevelBlanks, i, ElementType.Name)
                    Call DumpBuilder.AppendLine("{")
                    Call DumpBuilder.AppendLine(vbCrLf & __dumpInvoke(item, DumpLevel + 1))
                    Call DumpBuilder.AppendLine(LevelBlanks & "}")
                Next
            Else
                For i As Integer = 0 To ArrayData.Length - 1
                    Dim strData As String = ArrayData(i).ToString
                    Call DumpBuilder.AppendLine(String.Format("{0}[{1}] --> {2}", LevelBlanks, i, strData))
                Next
            End If

            Return DumpBuilder.ToString
        End Function

        Private Function IsGenericEnumerable(Type As Type) As Boolean
            Dim IsGenericType = Type.IsGenericType
            Dim p = Array.IndexOf(Type.GetInterfaces, GetType(IEnumerable))
            Dim f = IsGenericType AndAlso p > -1
            Return f
        End Function

        Private Function GetArray(en As IEnumerable) As Object()
            Dim LQuery As Object() = (From obj As Object In en Select obj).ToArray
            Return LQuery
        End Function

        Private Function DumpPropertyOrField(value As Object, TypeInfo As Type, Name As String, DumpLevel As UInteger, def As String) As String
            Dim LevelBlanks As String = New String(vbTab, DumpLevel + 1)
            Dim DumpBuilder As StringBuilder = New StringBuilder(1024)

            Dim IsArrayType As Boolean = TypeInfo.IsArray
            Dim IsEnumerableType As Boolean = IsGenericEnumerable(TypeInfo)

            If value Is Nothing Then
                Return ""
            End If

            If TypeInfo.IsArray OrElse IsEnumerableType Then
                Dim ArrayData As Object() = GetArray(DirectCast(value, IEnumerable))

                If ArrayData.IsNullOrEmpty Then
                    Return ""
                End If

                Dim ElementType = ArrayData.First.GetType()

                Call DumpBuilder.AppendLine(String.Format("{0}.{1} {2} ({3}) =>", LevelBlanks, def, Name, TypeInfo.FullName))
                Call DumpBuilder.AppendLine(LevelBlanks & "{")
                Call DumpBuilder.AppendLine(DumpArray(ElementType, Name, ArrayData, DumpLevel + 1))
                Call DumpBuilder.AppendLine(LevelBlanks & "}" & String.Format(" // end of array {0} {1}", def, Name))
            ElseIf TypeInfo.IsClass AndAlso Not TypeInfo Is GetType(String) Then
                Call DumpBuilder.AppendLine(String.Format("{0}.{1} {2} {3} =>", LevelBlanks, def, Name, TypeInfo.FullName))
                Call DumpBuilder.AppendLine(LevelBlanks & "{")
                Call DumpBuilder.AppendLine(__dumpInvoke(value, DumpLevel + 2))
                Call DumpBuilder.AppendLine(LevelBlanks & "} // end of " & Name)
            Else
                Return String.Format("{0}.{1} {2} ({3}) = {4}", LevelBlanks, def, Name, TypeInfo.FullName, value.ToString)
            End If

            Return DumpBuilder.ToString
        End Function
    End Module
End Namespace
