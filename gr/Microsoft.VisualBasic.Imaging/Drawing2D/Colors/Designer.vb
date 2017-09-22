#Region "Microsoft.VisualBasic::9f59435564b10840e80278c48aeb56e6, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Designer.vb"

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

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors

    ''' <summary>
    ''' Generate color sequence
    ''' </summary>
    Public Module Designer

        '''<summary>
        ''' {  
        '''  &quot;Color [PapayaWhip]&quot;: [
        '''    {
        '''      &quot;knownColor&quot;: 93,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: 0
        '''    },
        '''    {
        '''      &quot;knownColor&quot;: 119,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: 0
        '''    },
        '''    {
        '''      &quot;knownColor&quot;: 30,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: 0
        '''    },
        '''    {
        '''      &quot;knownColor&quot;: 165,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: 0
        '''    },
        '''    {
        '''      &quot;knownColor&quot;: 81,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: [rest of string was truncated]&quot;;.
        '''</summary>
        Public ReadOnly Property AvailableInterpolates As IReadOnlyDictionary(Of Color, Color())
        Public ReadOnly Property ColorBrewer As Dictionary(Of String, ColorBrewer)
        Public ReadOnly Property Rainbow As Color() = {
            Color.Red,
            Color.Orange,
            Color.Yellow,
            Color.Green,
            Color.Lime,
            Color.Blue,
            Color.Violet
        }

        ''' <summary>
        ''' 10 category colors for the data object cluster result
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ClusterColour As Color() = {
            Color.FromArgb(128, 200, 180),
            Color.FromArgb(135, 70, 194),
            Color.FromArgb(140, 210, 90),
            Color.FromArgb(200, 80, 147),
            Color.FromArgb(201, 169, 79),
            Color.FromArgb(112, 127, 189),
            Color.FromArgb(192, 82, 58),
            Color.FromArgb(83, 99, 60),
            Color.FromArgb(78, 45, 69),
            Color.FromArgb(202, 161, 169)
        }

        ''' <summary>
        ''' From TSF launcher on Android
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TSF As Color() = {
            {247, 69, 58},
            {230, 28, 99},
            {156, 36, 173},
            {107, 57, 181},
            {66, 81, 181},
            {33, 150, 238},
            {8, 170, 247},
            {0, 190, 214},
            {0, 150, 132},
            {74, 174, 82},
            {132, 194, 74},
            {206, 223, 58},
            {255, 235, 58},
            {255, 190, 0},
            {255, 150, 0},
            {255, 85, 33},
            {115, 85, 66},
            {156, 158, 156},
            {99, 125, 140}
        }.RowIterator _
         .Select(Function(c)
                     Return Color.FromArgb(c(0), c(1), c(2))
                 End Function) _
         .ToArray

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks>
        ''' ###### 2017-9-7
        ''' 
        ''' mono的Json反序列化任然存在问题
        ''' 
        ''' [ERROR 2017/9/7 10:03:20] &lt;Print>::System.Exception: Print ---> System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation. ---> System.TypeInitializationException: The type initializer for 'Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer' threw an exception. ---> System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation. ---> System.ArgumentNullException: Value cannot be null.
        ''' Parameter name :  key
        '''   at System.Collections.Generic.Dictionary`2[TKey,TValue].TryInsert (TKey key, TValue value, System.Collections.Generic.InsertionBehavior behavior) [0x00008] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''   at System.Collections.Generic.Dictionary`2[TKey,TValue].Add (TKey key, TValue value) [0x00000] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''   at (wrapper managed-to-native) System.Reflection.MonoMethod:InternalInvoke (System.Reflection.MonoMethod,object,object[],System.Exception&amp;)
        '''   at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00032] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''    --- End of inner exception stack trace ---
        '''   at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00048] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''   at System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters) [0x00000] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''   at System.Runtime.Serialization.Json.JsonFormatReaderInterpreter.ReadSimpleDictionary (System.Runtime.Serialization.CollectionDataContract collectionContract, System.Type keyValueType) [0x0014a] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.JsonFormatReaderInterpreter.ReadCollection (System.Runtime.Serialization.CollectionDataContract collectionContract) [0x000fe] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.JsonFormatReaderInterpreter.ReadCollectionFromJson (System.Runtime.Serialization.XmlReaderDelegator xmlReader, System.Runtime.Serialization.Json.XmlObjectSerializerReadContextComplexJson context, System.Xml.XmlDictionaryString emptyDictionaryString, System.Xml.XmlDictionaryString itemName, System.Runtime.Serialization.CollectionDataContract collectionContract) [0x00025] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.JsonFormatReaderGenerator+CriticalHelper+&lt;>c__DisplayClass1_0.&lt;GenerateCollectionReader>b__0 (System.Runtime.Serialization.XmlReaderDelegator xr, System.Runtime.Serialization.Json.XmlObjectSerializerReadContextComplexJson ctx, System.Xml.XmlDictionaryString emptyDS, System.Xml.XmlDictionaryString inm, System.Runtime.Serialization.CollectionDataContract cc) [0x0000c] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.JsonCollectionDataContract.ReadJsonValueCore (System.Runtime.Serialization.XmlReaderDelegator jsonReader, System.Runtime.Serialization.Json.XmlObjectSerializerReadContextComplexJson context) [0x0004f] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.JsonDataContract.ReadJsonValue (System.Runtime.Serialization.XmlReaderDelegator jsonReader, System.Runtime.Serialization.Json.XmlObjectSerializerReadContextComplexJson context) [0x00007] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.DataContractJsonSerializer.ReadJsonValue (System.Runtime.Serialization.DataContract contract, System.Runtime.Serialization.XmlReaderDelegator reader, System.Runtime.Serialization.Json.XmlObjectSerializerReadContextComplexJson context) [0x00006] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.XmlObjectSerializerReadContextComplexJson.ReadDataContractValue (System.Runtime.Serialization.DataContract dataContract, System.Runtime.Serialization.XmlReaderDelegator reader) [0x00000] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.XmlObjectSerializerReadContext.InternalDeserialize (System.Runtime.Serialization.XmlReaderDelegator reader, System.String name, System.String ns, System.Type declaredType, System.Runtime.Serialization.DataContract&amp; dataContract) [0x00264] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.XmlObjectSerializerReadContext.InternalDeserialize (System.Runtime.Serialization.XmlReaderDelegator xmlReader, System.Type declaredType, System.Runtime.Serialization.DataContract dataContract, System.String name, System.String ns) [0x0000b] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.XmlObjectSerializerReadContextComplex.InternalDeserialize (System.Runtime.Serialization.XmlReaderDelegator xmlReader, System.Type declaredType, System.Runtime.Serialization.DataContract dataContract, System.String name, System.String ns) [0x00010] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.DataContractJsonSerializer.InternalReadObject (System.Runtime.Serialization.XmlReaderDelegator xmlReader, System.Boolean verifyObjectName) [0x000c5] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.XmlObjectSerializer.InternalReadObject (System.Runtime.Serialization.XmlReaderDelegator reader, System.Boolean verifyObjectName, System.Runtime.Serialization.DataContractResolver dataContractResolver) [0x00000] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.XmlObjectSerializer.ReadObjectHandleExceptions (System.Runtime.Serialization.XmlReaderDelegator reader, System.Boolean verifyObjectName, System.Runtime.Serialization.DataContractResolver dataContractResolver) [0x00072] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.XmlObjectSerializer.ReadObjectHandleExceptions (System.Runtime.Serialization.XmlReaderDelegator reader, System.Boolean verifyObjectName) [0x00000] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.DataContractJsonSerializer.ReadObject (System.Xml.XmlDictionaryReader reader) [0x0000d] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.DataContractJsonSerializer.ReadObject (System.IO.Stream stream) [0x00017] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at Microsoft.VisualBasic.Serialization.JSON.JsonContract.LoadObject (System.String json, System.Type type, System.Boolean simpleDict) [0x0003e] In &lt;fb45d3e2e23e49ecae88d0914baf5cb7>:0
        '''   at Microsoft.VisualBasic.Serialization.JSON.JsonContract.LoadObject[T] (System.String json, System.Boolean simpleDict) [0x00000] In &lt;fb45d3e2e23e49ecae88d0914baf5cb7>:0
        '''   at Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer..cctor () [0x0040f] In &lt;fe90788c758b4b75b8659df0623dea4b>:0
        '''    --- End of inner exception stack trace ---
        '''   at json.Program.Convert (System.String In, System.String nodesTable, System.String kegKCF, System.Boolean degreeSize, System.Boolean compress, System.String style, System.Boolean nodeID, System.String[] maps) [0x00058] In &lt;f0aa82bd17214027b520f8953095c372>:0
        '''   at Biodeep.KEGG.Network.Common.KEGG.BuildNetworkJSON (System.Collections.Generic.IEnumerable`1[T] compounds, System.String repo, System.String out, System.String[] mapIDs) [0x00145] In &lt;598a64b776e440d8b3f5e763e2b2cdd0>:0
        '''   at cytonetwork.Program.BiodeepKEGGNetwork (Microsoft.VisualBasic.CommandLine.CommandLine args) [0x000c1] In &lt;2ebe3e57fb4d4cafb0d4cd22a0a764c2>:0
        '''   at (wrapper managed-to-native) System.Reflection.MonoMethod:InternalInvoke (System.Reflection.MonoMethod,object,object[],System.Exception&amp;)
        '''   at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00032] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''    --- End of inner exception stack trace ---
        '''   at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00048] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''   at System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters) [0x00000] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''   at Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.__directInvoke (System.Object[] callParameters, System.Object target, System.Boolean Throw) [0x0000c] In &lt;fb45d3e2e23e49ecae88d0914baf5cb7>:0
        '''    --- End of inner exception stack trace ---
        ''' </remarks>
        Sub New()
            Try

                Dim colors As Dictionary(Of String, String()) = My.Resources _
                    .designer_colors _
                    .LoadObject(Of Dictionary(Of String, String()))
                Dim valids As New Dictionary(Of Color, Color())

                For Each x In colors
                    valids(ColorTranslator.FromHtml(x.Key)) =
                        x.Value.ToArray(AddressOf ColorTranslator.FromHtml)
                Next

                AvailableInterpolates = valids

                Dim ns = Regex.Matches(My.Resources.colorbrewer, """\d+""") _
                    .ToArray(Function(m) m.Trim(""""c))
                Dim sb As New StringBuilder(My.Resources.colorbrewer)

                For Each n In ns.Distinct
                    Call sb.Replace($"""{n}""", $"""c{n}""")
                Next

                ColorBrewer = sb.ToString _
                    .LoadObject(Of Dictionary(Of String, ColorBrewer))

            Catch ex As Exception
                Call App.LogException(ex)
                Call "ColorBrewer module is not available on Linux server".Warning

                ColorBrewer = New Dictionary(Of String, ColorBrewer)
                AvailableInterpolates = New Dictionary(Of Color, Color())
            End Try
        End Sub

        ''' <summary>
        ''' <see cref="ColorMap"/> pattern names
        ''' </summary>
        ReadOnly __allColorMapNames$() = {
            ColorMap.PatternAutumn,
            ColorMap.PatternCool,
            ColorMap.PatternGray,
            ColorMap.PatternHot,
            ColorMap.PatternJet,
            ColorMap.PatternSpring,
            ColorMap.PatternSummer,
            ColorMap.PatternWinter
        }.ToArray(AddressOf LCase)

        ''' <summary>
        ''' Google material design colors
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property MaterialPalette As Color() = {
            Color.Red, Color.Pink, Color.Black, Color.Purple, Color.DarkViolet,
            Color.Indigo, Color.Blue, Color.LightBlue, Color.Cyan, Color.Teal,
            Color.Green, Color.LightGreen, Color.Lime, Color.Yellow, Color.Orchid,
            Color.Orange, Color.DarkOrange, Color.Brown, Color.Gray, Color.CadetBlue
        }

        Public ReadOnly Property Category31 As Color() = {
            "#BCBD22", "#4AD693", "#1F77B4", "#CDE64C", "#9AC901", "#56028E", "#A2D1CF", "#C8B631",
            "#6DBCEC", "#C24642", "#5F71BC", "#798DCD", "#292E76", "#FF7256", "#91CF4F", "#FE5050",
            "#FEBF00", "#FD7E00", "#07A7E4", "#51AC81", "#FE00FF", "#FDE688", "#7E00FD", "#CD00FF",
            "#988ED5", "#027093", "#73945A", "#8C564B", "#9467BD", "#D62829", "#2CA02C"
        }.AsColor()

        <Extension>
        Private Function IsColorNameList(exp$) As Boolean
            If Not exp.IsPattern(DesignerExpression.FunctionPattern) AndAlso InStr(exp, ",") > 0 Then
                If exp.IsPattern("rgb\(\d+\s*(,\s*\d+\s*)+\)") Then
                    Return False
                Else
                    Return True
                End If
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' 对于无效的键名称，默认是返回<see cref="Office2016"/>，请注意，如果是所有的.net的颜色的话，这里面还会包含有白色，所以还需要手工去除掉白色
        ''' </summary>
        ''' <param name="exp$">
        ''' <see cref="DesignerExpression"/>.
        ''' (假若这里所输入的是一组颜色值，则必须是htmlcolor或者颜色名称，RGB表达式将不会被允许)
        ''' </param>
        ''' <returns></returns>
        Public Function GetColors(exp$) As Color()
            If exp.IsColorNameList Then
                ' 设计器的表达式解析器目前不兼容颜色列表的表达式
                Return exp _
                    .StringSplit(",\s*") _
                    .Select(Function(c) c.TranslateColor) _
                    .ToArray
            Else
                With New DesignerExpression(exp)
                    Return .Modify(Designer.GetColorsInternal(.Term))
                End With
            End If
        End Function

        Private Function GetColorsInternal(term$) As Color()
            If Array.IndexOf(__allColorMapNames, term.ToLower) > -1 Then
                Return New ColorMap(20, 255).ColorSequence(term)
            End If

            Dim key As NamedValue(Of String) =
                Drawing2D.Colors.ColorBrewer.ParseName(term)

            If ColorBrewer.ContainsKey(key.Name) Then
                Return ColorBrewer(key.Name).GetColors(key.Value)
            End If

            If term.TextEquals("material") Then
                Return MaterialPalette
            ElseIf term.TextEquals("TSF") Then
                Return TSF
            ElseIf term.TextEquals("rainbow") Then
                Return Rainbow
            ElseIf term.TextEquals("dotnet.colors") Then
                Return AllDotNetPrefixColors
            ElseIf term.TextEquals("scibasic.chart()") Then
                Return ChartColors
            ElseIf term.TextEquals("scibasic.category31()") Then
                Return Category31
            ElseIf term.TextEquals("clusters") Then
                Return ClusterColour
            End If

            ' d3.js colors
            If term.TextEquals("d3.scale.category10()") Then
                Return d3js.category10
            ElseIf term.TextEquals("d3.scale.category20()") Then
                Return d3js.category20
            ElseIf term.TextEquals("d3.scale.category20b()") Then
                Return d3js.category20b
            ElseIf term.TextEquals("d3.scale.category20c()") Then
                Return d3js.category20c
            End If

            Return OfficeColorThemes.GetAccentColors(term)
        End Function

        ''' <summary>
        ''' 这个函数是获取得到一个连续的颜色谱
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <param name="n%"></param>
        ''' <param name="alpha%"></param>
        ''' <returns></returns>
        Public Function GetColors(term$, Optional n% = 256, Optional alpha% = 255) As Color()
            Return CubicSpline(GetColors(term), n, alpha)
        End Function

        ''' <summary>
        ''' ``New <see cref="SolidBrush"/>(<see cref="GetColors(String, Integer, Integer)"/>)``
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <param name="n%"></param>
        ''' <param name="alpha%"></param>
        ''' <returns></returns>
        Public Function GetBrushes(term$, Optional n% = 256, Optional alpha% = 255) As SolidBrush()
            Return GetColors(term, n, alpha).ToArray(Function(c) New SolidBrush(c))
        End Function

        ''' <summary>
        ''' 相对于<see cref="GetColors"/>函数而言，这个函数是返回非连续的颜色谱，假若数量不足，会重新使用开头的起始颜色连续填充
        ''' </summary>
        ''' <param name="colors$"></param>
        ''' <param name="n%"></param>
        ''' <returns></returns>
        <Extension> Public Function FromNames(colors$(), n%) As Color()
            Return colors.Select(AddressOf ToColor).__internalFills(n)
        End Function

        <Extension>
        Private Function __internalFills(colors As IEnumerable(Of Color), n As Integer) As Color()
            Dim out As New List(Of Color)(colors)
            Dim i As Integer = Scan0

            Do While out.Count < n
                out.Add(out(i))
                i += 1
            Loop

            Return out.ToArray
        End Function

        ''' <summary>
        ''' <see cref="FromSchema"/>和<see cref="FromNames"/>适用于函数绘图之类需要区分数据系列的颜色谱的生成
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <param name="n%"></param>
        ''' <returns></returns>
        Public Function FromSchema(term$, n%) As Color()
            Return GetColors(term).__internalFills(n)
        End Function

        ''' <summary>
        ''' **<see cref="ColorCube.GetColorSequence"/>**
        ''' 
        ''' Some useful color tables for images and tools to handle them.
        ''' Several color scales useful for image plots: a pleasing rainbow style color table patterned after 
        ''' that used in Matlab by Tim Hoar and also some simple color interpolation schemes between two or 
        ''' more colors. There is also a function that converts between colors and a real valued vector.
        ''' </summary>
        ''' <param name="col">A list of colors (names or hex values) to interpolate</param>
        ''' <param name="n%">Number of color levels. The setting n=64 is the orignal definition.</param>
        ''' <param name="alpha%">
        ''' The transparency of the color – 255 is opaque and 0 is transparent. This is useful for 
        ''' overlays of color and still being able to view the graphics that is covered.
        ''' </param>
        ''' <returns>
        ''' A vector giving the colors in a hexadecimal format, two extra hex digits are added for the alpha channel.
        ''' </returns>
        Public Function Colors(col As Color(), Optional n% = 256, Optional alpha% = 255) As Color()
            Dim out As New List(Of Color)
            Dim steps! = n / col.Length
            Dim previous As Value(Of Color) = col.First

            For Each c As Color In col.Skip(1)
                out += ColorCube.GetColorSequence(
                    source:=previous,
                    target:=previous = c,
                    increment:=steps!,
                    alpha:=alpha%)
            Next

            Return out
        End Function

        ''' <summary>
        ''' 这个函数并不会计算alpha的值
        ''' </summary>
        ''' <param name="colors"></param>
        ''' <param name="n">所期望的颜色的数量</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function CubicSpline(colors As IEnumerable(Of Color), Optional n% = 256, Optional alpha% = 255) As Color()
            Dim source As Color() = colors.ToArray
            Dim x As New CubicSplineVector(source.Select(Function(c) CSng(c.R)))
            Dim y As New CubicSplineVector(source.Select(Function(c) CSng(c.G)))
            Dim z As New CubicSplineVector(source.Select(Function(c) CSng(c.B)))

            Call x.CalcSpline()
            Call y.CalcSpline()
            Call z.CalcSpline()

            Dim delta! = 1 / n
            Dim out As New List(Of Color)

            For f! = 0 To 1.0! Step delta!
                Dim r% = __constraint(x.GetPoint(f))
                Dim g% = __constraint(y.GetPoint(f))
                Dim b% = __constraint(z.GetPoint(f))

                out += Color.FromArgb(alpha, r, g, b)
            Next

            Return out
        End Function

        Private Function __constraint(x!) As Integer
            If x < 0! Then
                x = 0!
            ElseIf x > 255.0! Then
                x = 255.0!
            End If

            Return x
        End Function
    End Module
End Namespace
