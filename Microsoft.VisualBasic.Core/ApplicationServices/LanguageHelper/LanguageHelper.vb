#Region "Microsoft.VisualBasic::aa2ce89be28d9bbb102c6c1ba1c87a7a, Microsoft.VisualBasic.Core\ApplicationServices\LanguageHelper\LanguageHelper.vb"

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

    '     Class LanguageHelper
    ' 
    '         Properties: CurrentLanguage, DeclaringType, LanguageResources
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: __init
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ApplicationServices.Globalization

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="TLanguage">必须是枚举类型</typeparam>
    Public Class LanguageHelper(Of TLanguage)

        Public ReadOnly Property LanguageResources As SortedDictionary(Of String, StringResources(Of TLanguage))
        Public ReadOnly Property DeclaringType As Type

        Public Property CurrentLanguage As TLanguage

        ''' <summary>
        ''' Get the string that store as specific language.
        ''' (获取指定语言的字符串)
        ''' </summary>
        ''' <param name="Language">字符串的语言</param>
        ''' <value></value>
        ''' <param name="res">请使用 NameOf 操作符来获取</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Default Public ReadOnly Property Text(Language As TLanguage, res As String) As String
            Get
                If Not _LanguageResources.ContainsKey(res) Then
                    Return ""
                Else
                    Dim resource = _LanguageResources(res)

                    If resource.Resources.ContainsKey(Language) Then
                        Return resource.Resources(Language).Text
                    Else
                        Return resource.Default
                    End If
                End If
            End Get
        End Property

        Default Public ReadOnly Property Text(res As String) As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Text(CurrentLanguage, res)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{DeclaringType.FullName}@{DirectCast(DirectCast(CurrentLanguage, Object), [Enum]).Description}"
        End Function

        ''' <summary>
        ''' 解析出所有属性，域上面的语言定义
        ''' </summary>
        ''' <param name="type">必须是模块类型或者属性或者域是共享类型的</param>
        Sub New(type As Type)
            Dim Propertys = type.GetProperties(BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static)
            Dim Fields = type.GetFields(BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static)
            Dim Members As New List(Of MemberInfo)

            Call Members.AddRange(Propertys)
            Call Members.AddRange(Fields)
            Call __init(Members)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="members"></param>
        Private Sub __init(members As IEnumerable(Of MemberInfo))
            Dim LQuery = LinqAPI.Exec(Of StringResources(Of TLanguage)) _
 _
                () <= From member As MemberInfo
                      In members
                      Let res As StringResources(Of TLanguage) = StringResources(Of TLanguage).SafelyGenerates(member)
                      Where Not res Is Nothing
                      Select res

            _LanguageResources = New SortedDictionary(Of String, StringResources(Of TLanguage))(LQuery.ToDictionary(Function(res) res.Name))
            _DeclaringType = members(Scan0).DeclaringType
        End Sub
    End Class
End Namespace
