#Region "Microsoft.VisualBasic::ba85f5e7914cccfad1a4a2adcb3d9fac, Microsoft.VisualBasic.Core\ApplicationServices\LanguageHelper\StringResources.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class StringResources
    ' 
    '         Properties: [Default], Name, Resources
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __getLanguageResources, __getValue, SafelyGenerates, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace ApplicationServices.Globalization

    Public Class StringResources(Of TLanguage)

        Public ReadOnly Property Resources As Dictionary(Of TLanguage, LanguageAttribute)
        Public ReadOnly Property Name As String
        ''' <summary>
        ''' 从属性或者域上面解析出来的默认的语言值
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property [Default] As String

        Sub New(name$, resources As LanguageAttribute())
            Me.Name = name
            Me.Resources = resources _
                .ToDictionary(Function(lang) CTypeDynamic(Of TLanguage)(lang.Language),
                              Function(lang) lang)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Name}({Resources.Count}) = {[Default]}"
        End Function

        Public Shared Function SafelyGenerates(member As MemberInfo) As StringResources(Of TLanguage)
            Dim langResources = __getLanguageResources(member)

            If langResources.IsNullOrEmpty Then
                Return Nothing
            Else
                Return New StringResources(Of TLanguage)(member.Name, langResources) With {
                    ._Default = __getValue(member)
                }
            End If
        End Function

        Private Shared Function __getValue(member As MemberInfo) As String
            Dim value As Object

            If (member.MemberType = MemberTypes.Property) Then
                value = DirectCast(member, PropertyInfo).GetValue(Nothing, Nothing)
            Else
                value = DirectCast(member, FieldInfo).GetValue(Nothing)
            End If

            Return Scripting.ToString(value)
        End Function

        Private Shared Function __getLanguageResources(member As MemberInfo) As LanguageAttribute()
            Dim attrs As Object() = member.GetCustomAttributes(attributeType:=GetType(LanguageAttribute), inherit:=True)

            If attrs.IsNullOrEmpty Then
                Return Nothing
            Else
                Return attrs _
                    .Select(Function(attr) DirectCast(attr, LanguageAttribute)) _
                    .ToArray
            End If
        End Function
    End Class
End Namespace
