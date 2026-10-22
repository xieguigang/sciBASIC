# Data/MyersDiff/MyersDiff.vbproj

- RootNamespace : Microsoft.VisualBasic.Data.MyersDiff
- AssemblyName  : Microsoft.VisualBasic.Data.MyersDiff
- TargetFramework: net10.0
- Source files  : 7
- Existing Title: Myers O(ND) Diff Algorithm for Text Comparison
- Existing Desc : A text differencing toolkit for sciBASIC#: implements the Myers O((N+M)D) shortest edit script algorithm to compare files, line sequences and character strings, producing structured diff blocks, unified and side-by-side output, and a sequence similarity score.
- Existing Tags : scibasic;diff;myers-algorithm;text-comparison;unified-diff;similarity

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Data.MyersDiff)

## Public types
- Class DiffBlock (DiffBlock.vb) - 表示一个差异块，即连续相同类型差异项的分组。
- Class DiffItem (DiffItem.vb) - 表示一个差异项，包含编辑类型、旧序列索引和新序列索引。
- Class DiffResult (DiffResult.vb) - 表示两个序列比较后的完整差异结果。
- Module DiffUtils (DiffUtils.vb) - 提供静态便捷方法的差异比较工具类。
- Enum EditType (EditType.vb) - 表示差异项的编辑操作类型。
- Class MyersDiff (MyersDiff.vb) - Myers 差异算法的 VB.NET 实现。 该算法以 O((N+M)D) 的时间复杂度计算两个序列之间的最短编辑脚本(SES)， 其中 N、M 分别为两个序列的长度，D 为编辑距离。
- Class NamespaceDoc (NamespaceDoc.vb) - The Myers O(ND) difference algorithm for text comparison: computes shortest edit scripts, structured diff blocks and unified diff output at both line level and character level.

## Notable public members
- Public Property OldStart As Integer
- Public Property OldCount As Integer
- Public Property NewStart As Integer
- Public Property NewCount As Integer
- Public Property Items As New List(Of DiffItem)()
- Public Sub New(oldStart As Integer, oldCount As Integer,
- Public Overrides Function ToString() As String
- Public Property Type As EditType
- Public Property OldIndex As Integer
- Public Property NewIndex As Integer
- Public Property Value As String
- Public Sub New(type As EditType, oldIndex As Integer, newIndex As Integer, value As String)
- Public Overrides Function ToString() As String
- Public Property Items As New List(Of DiffItem)()
- Public Property OldCount As Integer
- Public Property NewCount As Integer
- Public Property Truncated As Boolean
- Public ReadOnly Property EqualCount As Integer
- Public ReadOnly Property DeleteCount As Integer
- Public ReadOnly Property InsertCount As Integer
- Public ReadOnly Property Similarity As Double
- Public Function ToUnifiedDiff(oldLabel As String, newLabel As String,
- Public Function ToSideBySide(Optional maxWidth As Integer = 50) As String
- Public Function ToSummary() As String
- Public Function DiffFiles(oldFilePath As String, newFilePath As String,
- Public Function DiffLines(oldLines As String(), newLines As String(),
- Public Function DiffChars(oldText As String, newText As String) As DiffResult
- Public Function CompareFiles(oldFilePath As String, newFilePath As String) As DiffResult
- Public Function CompareLines(oldLines As String(), newLines As String()) As DiffResult
- Public Property MaxEditDistance As Integer = 5000
- Public Function Compare(oldLines As String(), newLines As String()) As DiffResult
- Public Function CompareChars(oldText As String, newText As String) As DiffResult
- Public Function CompareFiles(oldFilePath As String, newFilePath As String, Optional encoding As Encoding = Nothing) As DiffResult
- Public Property Type As EditType
- Public Property OldIndex As Integer
- Public Property NewIndex As Integer
- Public Sub New(type As EditType, oldIndex As Integer, newIndex As Integer)

## Imports
- Microsoft.VisualBasic.Serialization.JSON
- std = System.Math
- System.Text

## File tree
- DiffBlock.vb
- DiffItem.vb
- DiffResult.vb
- DiffUtils.vb
- EditType.vb
- MyersDiff.vb
- NamespaceDoc.vb

