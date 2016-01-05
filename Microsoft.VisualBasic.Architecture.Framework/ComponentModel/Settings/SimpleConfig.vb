Namespace ComponentModel.Settings

#If NET_40 = 0 Then

    <AttributeUsage(AttributeTargets.Property, allowmultiple:=False, inherited:=True)>
    Public Class SimpleConfig : Inherits Attribute
        Dim _ToLower As Boolean

        Public Shared ReadOnly Property TypeInfo As System.Type = GetType(SimpleConfig)
        Public ReadOnly Property Name As String

        Sub New(Optional Name As String = "", Optional toLower As Boolean = True)
            Me._Name = Name
            Me._ToLower = toLower
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Private Shared ReadOnly SimpleDataTypes As Type() = New System.Reflection.TypeInfo() {
            GetType(String),
            GetType(Integer),
            GetType(Double),
            GetType(Single),
            GetType(Boolean),
            GetType(Char),
            GetType(DateTime)
        }

        ''' <summary>
        '''
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TConfig"></typeparam>
        ''' <param name="canRead">向文件之中写数据的时候，需要设置为真</param>
        ''' <param name="canWrite">从文件之中读取数据的时候，需要设置为真</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function TryParse(Of T As Class, TConfig As SimpleConfig)(canRead As Boolean, canWrite As Boolean) As Dictionary(Of TConfig, System.Reflection.PropertyInfo)
            Dim TypeInfo As System.Reflection.TypeInfo = GetType(T), ConfigType As System.Type = GetType(TConfig)
            Dim Properties = TypeInfo.GetProperties(System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.Public)
            Dim LQuery = (From [property] As System.Reflection.PropertyInfo
                          In Properties
                          Let attrs As Object() = [property].GetCustomAttributes(attributeType:=ConfigType, inherit:=True)
                          Where Not attrs.IsNullOrEmpty AndAlso Array.IndexOf(SimpleDataTypes, [property].PropertyType) > -1
                          Select New KeyValuePair(Of TConfig, System.Reflection.PropertyInfo)(DirectCast(attrs.First, TConfig), [property])).ToArray

            If LQuery.IsNullOrEmpty Then Return Nothing

            Dim Schema As Dictionary(Of TConfig, System.Reflection.PropertyInfo) =
                New Dictionary(Of TConfig, System.Reflection.PropertyInfo)

            For Each Line In LQuery
                If Line.Value.CanRead AndAlso Line.Value.CanWrite Then  '同时满足可读和可写的属性直接添加
                    GoTo INSERT
                End If

                '从这里开始的属性都是只读属性或者只写属性
                If canRead = True Then
                    If Line.Value.CanRead = False Then
                        Continue For
                    End If
                End If
                If canWrite = True Then
                    If Line.Value.CanWrite = False Then
                        Continue For
                    End If
                End If
INSERT:
                If String.IsNullOrEmpty(Line.Key._Name) Then
                    Line.Key._Name = If(Line.Key._ToLower, Line.Value.Name.ToLower, Line.Value.Name)
                End If
                Call Schema.Add(Line.Key, Line.Value)
            Next

            Return Schema
        End Function

        ''' <summary>
        ''' 从类型实体生成配置文件数据
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="target"></param>
        ''' <returns></returns>
        ''' <remarks>类型实体之中的简单属性，只要具备可读属性即可被解析出来</remarks>
        Public Shared Function GenerateConfigurations(Of T As Class)(target As T) As String()
            Dim TypeInfo As System.Type = GetType(T)
            Dim Schema = TryParse(Of T, SimpleConfig)(canRead:=True, canWrite:=False)
            Dim MaxLength As Integer = (From item In Schema.Keys Select Len(item._Name)).ToArray.Max
            Dim Chunkbuffer As List(Of String) = New List(Of String)
            For Each [property] In Schema
                Dim Name As String = String.Format("{0}{1}", [property].Key._Name, New String(" ", MaxLength - Len([property].Key._Name) + 2))
                Call Chunkbuffer.Add(String.Format("{0}= {1}", Name, [property].Value.GetValue(target).ToString))
            Next

            Return Chunkbuffer.ToArray
        End Function
    End Class

#End If

End Namespace