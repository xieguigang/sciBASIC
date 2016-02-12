'出处：PConline 2004年02月14日 作者：Kain/CSDN 

'=================================说明============================== 　　
'一、原理说明：
'这只是一个简单的Gif图片播放控件　　
'原理其实很简单Gif文件是由三部分构成
'1、头文件
'2、帧
'3、文件结束标志
'头文件前五个字母固定由Gif89构成，借此可以判断是否为Gif文件
'头文件、帧与帧之间固定由标志 &H21 & HF9 连接，
'从&H21开始的第四个字节表示帧之间的延迟。
'由此就可以由头文件、每一帧和文件结束标志 &H3B 来构成单帧Gif文件
'由程序一帧帧的来显示
'=====================/下面是源程序代码，本人未调试===============================

Imports System.IO
Imports System.Drawing
Imports System.Threading
Imports System.ComponentModel

Public Class GifAnimation
    Inherits System.Windows.Forms.UserControl
    Const GifBz1 As Byte = 33 '帧标志 &H21
    Const GifBz2 As Byte = 249 '帧标志 &HF9
    Const GifEnd As Byte = 179 '结尾标志 &H3B

#Region " Windows 窗体设计器生成的代码 "
    Public Sub New()
        MyBase.New()
        '该调用是 Windows 窗体设计器所必需的。

        InitializeComponent()
        '在 InitializeComponent() 调用之后添加任何初始化
    End Sub

    'UserControl 重写 dispose 以清理组件列表。　　　　

    Protected Overloads Overrides Sub Dispose( disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Windows 窗体设计器所必需的　　　　
    Private components As System.ComponentModel.IContainer
    '注意: 以下过程是 Windows 窗体设计器所必需的　　　　
    '可以使用 Windows 窗体设计器修改此过程。　　　　
    '不要使用代码编辑器修改它。　　　　
    Private WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container

        Me.PictureBox1 = New System.Windows.Forms.PictureBox
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()     ' 　　　　
        'PictureBox1 　　　　' 　　　　
        Me.PictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.PictureBox1.Location = New System.Drawing.Point(4, 1)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(72, 70)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False     ' 　　　　
        'Timer1 　　　　' 　　　　' 　　　　
        'GifAnimation 　　　　' 　　　　
        Me.Controls.Add(Me.PictureBox1)
        Me.Name = "GifAnimation"
        Me.Size = New System.Drawing.Size(77, 72)
        Me.ResumeLayout(False)
    End Sub
#End Region


    '集合用以获取每帧信息
    Private m_col_Gif As Collection
    '停止标志
    Private mblnStop As Boolean = True
    Private mintCurrentPosition As Integer '当前播放位置
    Private mimgGif As Image
    Private mth As Thread

    Public Event Stoped( sender As Object) '停止事件

    Public WriteOnly Property GifFile() As String
        Set( Value As String)
            If Value Is Nothing Then Exit Property
            mimgGif = LoadImage(Value)
            m_col_Gif = FormatGIF(Value)
            If m_col_Gif Is Nothing OrElse m_col_Gif.Count = 0 Then
                Me.PictureBox1.Image = Nothing
                mimgGif = Nothing
                Exit Property
            End If
            SetPicToPicturbox(CType(m_col_Gif.Item(1), GifFrame).Frame)
        End Set
    End Property

    Public Property SizeModel() As PictureBoxSizeMode
        Get
            Return Me.PictureBox1.SizeMode
        End Get
        Set( Value As PictureBoxSizeMode)
            Me.PictureBox1.SizeMode = Value
        End Set
    End Property

    Public Property GifImage() As Image
        Get
            Return mimgGif
        End Get

        Set( Value As Image)
            If Value Is Nothing Then Exit Property
            mimgGif = Value
            m_col_Gif = FormatGIF(Value)
            If m_col_Gif Is Nothing OrElse m_col_Gif.Count = 0 Then
                Me.PictureBox1.Image = Nothing
                Exit Property
            End If
            SetPicToPicturbox(CType(m_col_Gif.Item(1), GifFrame).Frame)
        End Set
    End Property

    Public Sub StopView()
        mblnStop = True
    End Sub

    Private Sub Start()
        Dim i As Integer
        Dim gif As GifFrame
        Do Until mblnStop = True
            i += 1
            If i > m_col_Gif.Count Then
                i = 1
            End If
            gif = m_col_Gif.Item(i)
            Thread.Sleep(gif.Invert * 10) '帧与帧之间的延迟
            '显示图像
            SetPicToPicturbox(gif.Frame)
        Loop
        SetPicToPicturbox(CType(m_col_Gif.Item(1), GifFrame).Frame)
        RaiseEvent Stoped(Me)
    End Sub

    Public Sub StartView(Optional  useTimer As Boolean = True)
        If m_col_Gif Is Nothing Then Exit Sub
        If m_col_Gif.Count = 0 Then Exit Sub
        mblnStop = False
        If useTimer Then
            If m_col_Gif Is Nothing Then Exit Sub
            If m_col_Gif.Count = 0 Then Exit Sub
            mblnStop = False
            Timer1.Enabled = True
        Else
            mth = New Thread(AddressOf Start)
            mth.Priority = ThreadPriority.Normal
            mth.Start()
        End If
    End Sub

    '从文获得帧信息
    Private Function FormatGIF( GifFile As String) As Collection

        '打开图片文件
        Dim fs As New FileStream(GifFile, FileMode.Open, FileAccess.Read)
        '文件长度
        Dim fileLen As Long = fs.Length
        Dim br As New BinaryReader(fs)
        Dim buff As Byte()
        '将图片信息全部读入一个字节数组
        buff = br.ReadBytes(fileLen)
        Return FormatGIF(buff)
    End Function

    '从Image对象获取帧信息
    Private Function FormatGIF( GifImage As Image) As Collection
        Dim col As New Collection
        '创建一个内存流
        Dim sr As New MemoryStream
        '将图像写入到流
        GifImage.Save(sr, Imaging.ImageFormat.Gif)
        Dim buff As Byte()
        '将图像转换成字节数组
        buff = sr.ToArray
        Return FormatGIF(buff)
    End Function

    '从一个字节数组获得帧信息
    Private Function FormatGIF( buff() As Byte) As Collection
        Dim col As New Collection
        Dim Index1 As Integer
        Dim Index2 As Integer
        Dim IndexTmp As Long
        Dim intTime As Integer
        Dim buffHead() As Byte
        Dim buffBody() As Byte
        Dim img As Image
        If buff.Length < 3 Then Return col
        '将头三字节转换成字符串
        Dim tmp As String = System.Text.Encoding.Default.GetString(buff, 0, 3).ToLower
        '是否是Gif文件
        If tmp <> "gif" Then
            Throw New Exception("图片格式错误！必须是Gif文件")
        End If
        Do
            '查找第一个标志 &H21
            Index1 = Array.IndexOf(buff, GifBz1, Index1 + 1)
            If Index1 < 0 Or Index1 >= buff.Length - 1 Then Return col
            '查找第二个标志 &H21
            Index2 = Array.IndexOf(buff, GifBz2, Index1)
            '两个标志是否连续
            If Index2 - Index1 = 1 Then Exit Do
        Loop
        IndexTmp = Index1
        '创建一个缓冲
        buffHead = Array.CreateInstance(GetType(System.Byte), Index1)
        '获得头部信息
        Array.Copy(buff, buffHead, Index1) 'read gifhead
        Do
            Do
                Index1 = Array.IndexOf(buff, GifBz1, Index1 + 1)
                If Index1 < 0 Or Index1 >= buff.Length - 1 Then Exit Do
                Index2 = Array.IndexOf(buff, GifBz2, Index1)
                If Index2 - Index1 = 1 Then Exit Do
            Loop
            '是否是最后一帧
            If Index1 < 0 Or Index1 >= buff.Length - 1 Then Exit Do
            '创建缓冲
            buffBody = Array.CreateInstance(GetType(Byte), Index1 - IndexTmp)

            '获取帧信息
            Array.Copy(buff, IndexTmp, buffBody, 0, Index1 - IndexTmp)
            ' 获取每帧的间隔时间
            intTime = Val(buff(IndexTmp + 4))
            '重建每帧图像
            img = CreateImage(buffHead, buffBody)
            '创建一个帧对象
            Dim gif As New GifFrame(img, intTime)
            '添加到集合
            col.Add(gif)
            IndexTmp = Index1
        Loop
        '最后一帧
        buffBody = Array.CreateInstance(GetType(Byte), buff.Length - IndexTmp)
        Array.Copy(buff, IndexTmp, buffBody, 0, buff.Length - IndexTmp)
        '获取每帧的间隔时间
        intTime = Val(buff(IndexTmp + 4))
        img = CreateImage(buffHead, buffBody, False)
        Dim gifs As New GifFrame(img, intTime)
        col.Add(gifs)
        Return col
    End Function

    '创建一个帧图像
    Private Function CreateImage( gifHead() As Byte,  gifBody() As Byte, Optional  AddEnd As Boolean = True) As Image
        '创建一个内存流
        Dim sm As New MemoryStream
        Dim img As Image
        '写入头部信息
        sm.Write(gifHead, 0, gifHead.Length)
        '写入帧信息
        sm.Write(gifBody, 0, gifBody.Length)
        '如果不是最后一帧则写入结束标志
        If AddEnd Then sm.WriteByte(GifAnimation.GifEnd)
        '创建图形
        img = Image.FromStream(sm)
        '关闭流
        sm.Close()
        Return img
    End Function

    '显示一个帧图像
    Private Sub SetPicToPicturbox( img As Image)
        Me.PictureBox1.Image = img
        PictureBox1.Top = 0
        PictureBox1.Left = 0
        If PictureBox1.SizeMode <> PictureBoxSizeMode.AutoSize Then Exit Sub
        Me.Width = Me.PictureBox1.Width
        Me.Height = Me.PictureBox1.Height
    End Sub

    Private Sub Timer1_Tick( sender As Object,  e As System.EventArgs) Handles Timer1.Tick
        If mintCurrentPosition < m_col_Gif.Count Then
            mintCurrentPosition = mintCurrentPosition + 1
        Else
            mintCurrentPosition = 1
        End If
        '获取当前帧
        Dim gif As GifFrame = m_col_Gif.Item(mintCurrentPosition)
        SetPicToPicturbox(gif.Frame) '显示帧图像
        Timer1.Interval = gif.Invert * 10 '帧延迟
        If mblnStop = True Then
            Timer1.Enabled = False
            SetPicToPicturbox(CType(m_col_Gif.Item(1), GifFrame).Frame)
            RaiseEvent Stoped(Me) '触发事件
        End If
    End Sub

    Private Sub GifAnimation_BackColorChanged( sender As Object,  e As System.EventArgs) Handles MyBase.BackColorChanged
        PictureBox1.BackColor = Me.BackColor
    End Sub

    Private Sub GifAnimation_Resize( sender As Object,  e As System.EventArgs) Handles MyBase.Resize
        PictureBox1.Top = 0
        PictureBox1.Left = 0
        If PictureBox1.SizeMode <> PictureBoxSizeMode.AutoSize Then
            PictureBox1.Width = Me.Width
            PictureBox1.Height = Me.Height
        Else
            Me.Width = PictureBox1.Width
            Me.Height = PictureBox1.Height
        End If
    End Sub

    Private Sub GifAnimation_Disposed( sender As Object,  e As System.EventArgs) Handles MyBase.Disposed
        Try
            Me.mblnStop = True
            If Not mth Is Nothing Then
                '停止线程
                mth.Abort()
            End If
        Catch ex As Exception
        End Try
    End Sub

    '帧对象
    '用以储存每帧的信息：帧延迟、图像
    Private Class GifFrame
        Public Frame As Image '帧图像
        Public Invert As Integer '帧延迟

        Public Sub New( img As Image,  time As Integer)
            Frame = img
            Invert = time
        End Sub
    End Class
End Class