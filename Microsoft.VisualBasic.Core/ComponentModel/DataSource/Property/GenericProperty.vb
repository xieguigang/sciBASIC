Imports System.Runtime.CompilerServices
Imports System.Web.Script.Serialization

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' Dictionary for [<see cref="String"/>, <typeparamref name="T"/>]
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class [Property](Of T) : Inherits DynamicPropertyBase(Of T)

        Sub New()
        End Sub

        ''' <summary>
        ''' New with a init property value
        ''' </summary>
        ''' <param name="initKey"></param>
        ''' <param name="initValue"></param>
        Sub New(initKey$, initValue As T)
            Call Properties.Add(initKey, initValue)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        <ScriptIgnore> Public Iterator Property src As IEnumerable(Of NamedValue(Of T))
            Get
                For Each x In Properties
                    Yield New NamedValue(Of T) With {
                        .Name = x.Key,
                        .Value = x.Value
                    }
                Next
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As IEnumerable(Of NamedValue(Of T)))
                Properties = value.ToDictionary(Function(x) x.Name, Function(x) x.Value)
            End Set
        End Property
    End Class
End Namespace