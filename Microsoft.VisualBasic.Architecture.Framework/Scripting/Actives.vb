#Region "Microsoft.VisualBasic::c072a08162607f04b67e968d0b110e91, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Scripting\Actives.vb"

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

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting

    Public Module Actives

        <Extension> Public Function DisplType(type As Type) As String
            Dim sb As New StringBuilder

            Call sb.AppendLine($"**Decalre**:  _{type.FullName}_")
            Call sb.AppendLine("Example: ")
            Call sb.AppendLine("```json")
            Call sb.AppendLine(Active(type))
            Call sb.AppendLine("```")

            Return sb.ToString
        End Function

        ''' <summary>
        ''' Display the json of the target type its instance object.
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function Active(type As Type) As String
            Dim obj As Object = type.__active
            Return GetObjectJson(obj, type)
        End Function

        ''' <summary>
        ''' Creates the example instance object for the example
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension> Private Function __active(type As Type) As Object
            If type.Equals(GetType(String)) Then
                Return type.FullName
            End If
            If type.Equals(GetType(Char)) Then
                Return "c"c
            End If
            If type.Equals(GetType(Date)) OrElse type.Equals(GetType(DateTime)) Then
                Return Now
            End If
            If __examples.ContainsKey(type) Then
                Return __examples(type)
            End If
            If type.IsInheritsFrom(GetType(Array)) Then
                Dim e As Object = type.GetElementType.__active
                Dim array As Array =
                    System.Array.CreateInstance(type.GetElementType, 1)

                Call array.SetValue(e, Scan0)

                Return array
            End If

            Try
                Dim obj As Object = Activator.CreateInstance(type)

                For Each prop As PropertyInfo In type.GetProperties.Where(
                    Function(x) x.CanWrite AndAlso
                    x.GetIndexParameters.IsNullOrEmpty)

                    Dim value As Object = prop.PropertyType.__active

                    Call prop.SetValue(obj, value)
                Next

                Return obj
            Catch ex As Exception
                ex = New Exception(type.FullName, ex)
                Call App.LogException(ex)
                Return Nothing
            End Try
        End Function

        ReadOnly __examples As IReadOnlyDictionary(Of Type, Object) =
            New Dictionary(Of Type, Object) From {
 _
                {GetType(Double), 0R},
                {GetType(Double?), 0R},
                {GetType(Single), 0!},
                {GetType(Single?), 0!},
                {GetType(Integer), 0},
                {GetType(Integer?), 0},
                {GetType(Long), 0L},
                {GetType(Long?), 0L},
                {GetType(Short), 0S},
                {GetType(Short?), 0S},
                {GetType(Byte), 0},
                {GetType(SByte), 0},
                {GetType(Boolean), True},
                {GetType(Decimal), 0@}
        }
    End Module
End Namespace
