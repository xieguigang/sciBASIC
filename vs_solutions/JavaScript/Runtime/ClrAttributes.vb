
Namespace Runtime

    ''' <summary>
    ''' Renames a CLR member for script access: <c>&lt;JsName("count")&gt;</c> makes the
    ''' member visible as <c>obj.count</c>.
    ''' <para>
    ''' The bridge matches member names exactly (there is no implicit
    ''' camelCase↔PascalCase mapping), so this attribute is the way to expose
    ''' <c>TotalCount</c> as <c>obj.totalCount</c>.
    ''' </para>
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field Or AttributeTargets.Method Or
                    AttributeTargets.Constructor Or AttributeTargets.Class Or AttributeTargets.Struct,
                    AllowMultiple:=False, Inherited:=True)>
    Public NotInheritable Class JsNameAttribute
        Inherits Attribute

        ''' <summary>The script-visible name.</summary>
        Public ReadOnly Property Name As String

        Public Sub New(name As String)
            Me.Name = name
        End Sub

    End Class

    ''' <summary>
    ''' Hides a CLR member from script (it stays reachable from .NET code only).
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field Or AttributeTargets.Method Or
                    AttributeTargets.Constructor Or AttributeTargets.Class Or AttributeTargets.Struct,
                    AllowMultiple:=False, Inherited:=True)>
    Public NotInheritable Class JsIgnoreAttribute
        Inherits Attribute
    End Class

    ''' <summary>
    ''' Opts a single member into script access and — as soon as one member of a type
    ''' carries it — switches that type to WHITELIST mode: only members marked with
    ''' this attribute stay visible, so a type can be exposed selectively without
    ''' hand-maintaining an ignore list.
    ''' <para>
    ''' <see cref="ClrInterop.ExplicitOnly"/> forces whitelist mode for every type.
    ''' </para>
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field Or AttributeTargets.Method Or
                    AttributeTargets.Constructor,
                    AllowMultiple:=False, Inherited:=True)>
    Public NotInheritable Class JsExposeAttribute
        Inherits Attribute
    End Class

End Namespace
