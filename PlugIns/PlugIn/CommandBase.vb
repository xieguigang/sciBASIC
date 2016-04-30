Imports System.Reflection
Imports System.Xml.Serialization

Public MustInherit Class CommandBase : Inherits Attribute

    ''' <summary>
    ''' The command item name that display on the menustrip control.
    ''' </summary>
    ''' <remarks></remarks>
    <XmlAttribute> Public Name As String
    ''' <summary>
    ''' Tooltip text or the description text.
    ''' </summary>
    ''' <remarks></remarks>
    <XmlAttribute> Public Description As String
    ''' <summary>
    ''' The icon resource name string.(图标资源名称，当本属性值为空的时候，对应的菜单项没有图标)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <XmlAttribute> Public Property Icon As String = ""

    Public Overrides Function ToString() As String
        Return String.Format("({0}) {1}", Name, Description)
    End Function
End Class

Public Enum EntryTypes
    ''' <summary>
    ''' This method is the entry point to destroy this plugin command and unload it from the target form.
    ''' </summary>
    ''' <remarks></remarks>
    Dispose
    ''' <summary>
    ''' Sometimes you need a initialize method to initialize your plugin.
    ''' </summary>
    ''' <remarks></remarks>
    Initialize
    ''' <summary>
    ''' This method is the icon loader entry point
    ''' </summary>
    ''' <remarks></remarks>
    IconLoader
End Enum

''' <summary>
''' 
''' </summary>
<AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
Public Class EntryFlag : Inherits Attribute

    <XmlAttribute> Public Property EntryType As EntryTypes

    Friend Target As MethodInfo
    Friend GetIconInvoke As Func(Of String, Object)

    Friend ReadOnly Property GetIcon(Name As String) As System.Drawing.Image
        Get
            If Not Target Is Nothing Then
                Return PlugIn.PlugInEntry.Invoke(New Object() {Name}, Target)
            Else
                If GetIconInvoke Is Nothing Then
                    Return Nothing
                Else
                    Return GetIconInvoke(Name)
                End If
            End If
        End Get
    End Property

    Friend Function Initialize(Target As MethodInfo) As EntryFlag
        Me.Target = Target
        Return Me
    End Function

    Public Overrides Function ToString() As String
        Return EntryType.ToString
    End Function
End Class