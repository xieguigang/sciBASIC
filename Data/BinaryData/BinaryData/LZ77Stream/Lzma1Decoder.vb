Option Strict On
Option Explicit On

Imports System.IO
Imports std = System.Math

Namespace LZ77Stream

    ''' <summary>
    ''' Decoder core for the raw LZMA1 algorithm. Ported to VB.NET from the
    ''' public-domain LZMA specification (Igor Pavlov, LzmaSpec.cpp / LZMA SDK).
    ''' The 13-byte ".lzma alone" container header is parsed by <see cref="LzmaStream"/>.
    ''' <para>
    ''' Output is written into a ring buffer of <see langword="DictionarySize"/>
    ''' bytes; the owner pulls bytes out with <see cref="CopyOut"/>. Because the
    ''' owner only decodes while its read position has caught up (at most 273
    ''' bytes of look-ahead — one match), no unconsumed byte is ever overwritten.
    ''' </para>
    ''' </summary>
    Friend NotInheritable Class Lzma1Decoder

        ' ---- probability model constants ----
        Private Const kNumBitModelTotalBits As Integer = 11     ' probabilities are 11-bit (0..2047)
        Private Const kNumMoveBits As Integer = 5
        Private Const kNumPosBitsMax As Integer = 4
        Private Const kNumStates As Integer = 12
        Private Const kNumLenToPosStates As Integer = 4
        Private Const kNumAlignBits As Integer = 4
        Private Const kEndPosModelIndex As Integer = 14
        Private Const kStartPosModelIndex As Integer = 4

        ' ---- state transition tables ----
        Private Shared ReadOnly kLiteralNextStates As Byte() = {0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 4, 5}
        Private Shared ReadOnly kMatchNextStates As Byte() = {7, 7, 7, 7, 7, 7, 7, 10, 10, 10, 10, 10}
        Private Shared ReadOnly kRepNextStates As Byte() = {8, 8, 8, 8, 8, 8, 8, 11, 11, 11, 11, 11}
        Private Shared ReadOnly kShortRepNextStates As Byte() = {9, 9, 9, 9, 9, 9, 9, 11, 11, 11, 11, 11}

        ' ---- probability tables (UShort, 11-bit, initialised to 1024) ----
        Private ReadOnly _isMatch(kNumStates * (1 << kNumPosBitsMax) - 1) As UShort
        Private ReadOnly _isRep(kNumStates - 1) As UShort
        Private ReadOnly _isRepG0(kNumStates - 1) As UShort
        Private ReadOnly _isRepG1(kNumStates - 1) As UShort
        Private ReadOnly _isRepG2(kNumStates - 1) As UShort
        Private ReadOnly _isRep0Long(kNumStates * (1 << kNumPosBitsMax) - 1) As UShort
        Private ReadOnly _posSlot(kNumLenToPosStates * 64 - 1) As UShort
        Private ReadOnly _posDecoders(115 - 1) As UShort
        Private ReadOnly _align(16 - 1) As UShort

        ' literal probabilities: 0x300 per (lp-position, previous-byte) state
        Private ReadOnly _litProbs As UShort()

        ' match-length decoder (new matches)
        Private ReadOnly _lenChoice(1 - 1) As UShort
        Private ReadOnly _lenChoice2(1 - 1) As UShort
        Private ReadOnly _lenLow(16 * 8 - 1) As UShort
        Private ReadOnly _lenMid(16 * 8 - 1) As UShort
        Private ReadOnly _lenHigh(256 - 1) As UShort

        ' match-length decoder (rep matches)
        Private ReadOnly _repChoice(1 - 1) As UShort
        Private ReadOnly _repChoice2(1 - 1) As UShort
        Private ReadOnly _repLenLow(16 * 8 - 1) As UShort
        Private ReadOnly _repLenMid(16 * 8 - 1) As UShort
        Private ReadOnly _repLenHigh(256 - 1) As UShort

        ' ---- model state ----
        Private ReadOnly _rc As LzmaRangeDecoder
        Private ReadOnly _dict As Byte()
        Private _dictPos As Integer              ' next write position in the ring
        Private _total As ULong                  ' total bytes produced
        Private _state As Integer
        Private ReadOnly _reps(3) As UInteger
        Private _markerSeen As Boolean

        Public ReadOnly Property Lc As Integer
        Public ReadOnly Property Lp As Integer
        Public ReadOnly Property Pb As Integer

        Private ReadOnly _lpMask As ULong
        Private ReadOnly _posStateMask As ULong

        ''' <summary>Total number of decompressed bytes produced so far.</summary>
        Public ReadOnly Property TotalProduced As ULong
            Get
                Return _total
            End Get
        End Property

        ''' <summary>True once the end-of-stream marker has been decoded.</summary>
        Public ReadOnly Property Finished As Boolean
            Get
                Return _markerSeen
            End Get
        End Property

        ''' <param name="props">The 1-byte LZMA properties (lc / lp / pb packed as (pb*5+lp)*9+lc).</param>
        ''' <param name="dictSize">Dictionary size in bytes (will be normalised to at least 4096).</param>
        ''' <param name="input">The raw LZMA stream positioned right after the 13-byte header.</param>
        Public Sub New(props As Integer, dictSize As UInteger, input As Stream)
            If props < 0 OrElse props > 224 Then
                Throw New InvalidDataException($"LZMA: invalid properties byte {props} (must be 0..224).")
            End If
            If dictSize < 4096UI Then dictSize = 4096UI

            Dim d = CUInt(props)
            Lc = CInt(d Mod 9UI)
            d = d \ 9UI
            Lp = CInt(d Mod 5UI)
            d = d \ 5UI
            Pb = CInt(d)

            _lpMask = (1UL << Lp) - 1UL
            _posStateMask = (1UL << Pb) - 1UL
            _litProbs = New UShort(CInt((&H300UI << (Lc + Lp)) - 1UI)) {}
            _dict = New Byte(CInt(dictSize) - 1) {}
            _rc = New LzmaRangeDecoder(input)
            InitProbs()
        End Sub

        Public ReadOnly Property DictionarySize As Integer
            Get
                Return _dict.Length
            End Get
        End Property

        ''' <summary>Decodes exactly one symbol (literal, match, rep match or the EOS marker).</summary>
        Public Sub DecodeSymbol()
            Dim posState = CInt(_total And _posStateMask)

            If _rc.DecodeBit(_isMatch, (_state << kNumPosBitsMax) + posState) = 0UI Then
                ' ================= literal =================
                Dim prevByte As Byte = 0
                If _total > 0UL Then
                    Dim p = _dictPos - 1
                    If p < 0 Then p += _dict.Length
                    prevByte = _dict(p)
                End If

                ' NB: widen prevByte to Integer first — a shift of 8 on a Byte is a no-op in VB
                ' (shift count is taken modulo the operand's bit width).
                Dim litState = CUInt(((_total And _lpMask) << Lc) + CULng(CInt(prevByte) >> (8 - Lc)))
                Dim litOffset = CInt(&H300UI * litState)
                Dim symbol As UInteger = 1

                If _state >= 7 Then
                    ' Literal right after a match — decoded with the "matched byte" model.
                    Dim matchByte As UInteger = CUInt(GetByte(_reps(0) + 1UI))
                    Do While symbol < &H100UI
                        Dim matchBit = (matchByte >> 7) And 1UI
                        matchByte = (matchByte << 1) And &HFFUI
                        Dim bit = _rc.DecodeBit(_litProbs, litOffset + CInt(((1UI + matchBit) << 8) + symbol))
                        symbol = (symbol << 1) Or bit
                        If matchBit <> bit Then Exit Do
                    Loop
                End If

                Do While symbol < &H100UI
                    symbol = (symbol << 1) Or _rc.DecodeBit(_litProbs, litOffset + CInt(symbol))
                Loop

                PutByte(CByte(symbol And &HFFUI))
                _state = kLiteralNextStates(_state)

            Else
                Dim len As UInteger
                Dim dist As UInteger

                If _rc.DecodeBit(_isRep, _state) <> 0UI Then
                    ' ================= rep match =================
                    If _rc.DecodeBit(_isRepG0, _state) = 0UI Then
                        If _rc.DecodeBit(_isRep0Long, (_state << kNumPosBitsMax) + posState) = 0UI Then
                            ' short rep: repeat the byte at distance rep0
                            _state = kShortRepNextStates(_state)
                            PutByte(GetByte(_reps(0) + 1UI))
                            Return
                        End If
                    Else
                        ' rep1 / rep2 / rep3 match: the chosen slot moves to the front and
                        ' the slots above it shift down by one. NB: the IsRepG1 bit must
                        ' be decoded BEFORE any shuffle happens (LzmaSpec.cpp).
                        Dim d As UInteger
                        If _rc.DecodeBit(_isRepG1, _state) = 0UI Then
                            ' rep1: swap rep0 <-> rep1
                            d = _reps(1)
                            _reps(1) = _reps(0)
                            _reps(0) = d
                        ElseIf _rc.DecodeBit(_isRepG2, _state) = 0UI Then
                            ' rep2
                            d = _reps(2)
                            _reps(2) = _reps(1)
                            _reps(1) = _reps(0)
                            _reps(0) = d
                        Else
                            ' rep3
                            d = _reps(3)
                            _reps(3) = _reps(2)
                            _reps(2) = _reps(1)
                            _reps(1) = _reps(0)
                            _reps(0) = d
                        End If
                    End If

                    len = DecodeLength(_repChoice, _repChoice2, _repLenLow, _repLenMid, _repLenHigh, posState) + 2UI
                    _state = kRepNextStates(_state)
                    dist = _reps(0)

                Else
                    ' ================= new match =================
                    _reps(3) = _reps(2)
                    _reps(2) = _reps(1)
                    _reps(1) = _reps(0)

                    len = DecodeLength(_lenChoice, _lenChoice2, _lenLow, _lenMid, _lenHigh, posState) + 2UI
                    _state = kMatchNextStates(_state)

                    ' GetLenToPosState in the spec receives the length BEFORE the +2
                    ' (0-based, 0..271): len < 3 -> len, otherwise 3.
                    Dim lenToPosState = CInt(len) - 2
                    If lenToPosState > 3 Then lenToPosState = 3
                    dist = DecodeDistance(lenToPosState)
                    If dist = &HFFFFFFFFUI Then
                        ' end-of-stream marker
                        _markerSeen = True
                        Return
                    End If
                    _reps(0) = dist
                End If

                ' DecodeDistance returns the 0-based distance (spec: CopyMatch(Rep0 + 1, len)).
                CopyMatch(dist + 1UI, len)
            End If
        End Sub

        ''' <summary>
        ''' Copies <paramref name="count"/> decompressed bytes, starting at absolute
        ''' output position <paramref name="totalPos"/>, into the consumer buffer.
        ''' </summary>
        Public Sub CopyOut(totalPos As ULong, dst As Byte(), dstOffset As Integer, count As Integer)
            Dim n = _dict.Length
            Dim p = CInt(totalPos Mod CULng(n))
            Dim i = dstOffset
            Dim remaining = count
            While remaining > 0
                Dim chunk = std.Min(remaining, n - p)
                Array.Copy(_dict, p, dst, i, chunk)
                p = 0
                i += chunk
                remaining -= chunk
            End While
        End Sub

        ' =====================================================================
        '  Output window
        ' =====================================================================

        Private Sub PutByte(b As Byte)
            _dict(_dictPos) = b
            _dictPos += 1
            If _dictPos = _dict.Length Then _dictPos = 0
            _total += 1UL
        End Sub

        ''' <summary>Returns the byte <paramref name="dist"/> positions before the next write position.</summary>
        Private Function GetByte(dist As UInteger) As Byte
            Dim s = CInt(_dictPos - dist)
            If s < 0 Then s += _dict.Length
            Return _dict(s)
        End Function

        Private Sub CopyMatch(dist As UInteger, len As UInteger)
            If CULng(dist) > _total Then
                Throw New InvalidDataException("LZMA: match distance points before the start of the stream.")
            End If
            If dist > CUInt(_dict.Length) Then
                Throw New InvalidDataException("LZMA: match distance exceeds the dictionary size.")
            End If

            Dim n = _dict.Length
            Dim w = _dictPos
            Dim s = CInt(w - dist)
            If s < 0 Then s += n

            Dim remaining = CInt(len)
            Do While remaining > 0
                If dist >= CUInt(remaining) Then
                    ' Non-overlapping: bulk copy, at most up to the ring boundary.
                    Dim c = remaining
                    If w + c > n Then c = n - w
                    If s + c > n Then c = n - s
                    Array.Copy(_dict, s, _dict, w, c)
                    w += c
                    If w = n Then w = 0
                    s += c
                    If s = n Then s = 0
                    remaining -= c
                    _total += CULng(c)
                Else
                    ' Overlapping (RLE-like): byte by byte.
                    Do
                        _dict(w) = _dict(s)
                        w += 1
                        If w = n Then w = 0
                        s += 1
                        If s = n Then s = 0
                        remaining -= 1
                        _total += 1UL
                    Loop While remaining > 0
                End If
            Loop

            _dictPos = w
        End Sub

        ' =====================================================================
        '  Sub-decoders
        ' =====================================================================

        Private Function DecodeLength(choice As UShort(), choice2 As UShort(),
                                  low As UShort(), mid As UShort(), high As UShort(),
                                  posState As Integer) As UInteger
            If _rc.DecodeBit(choice, 0) = 0UI Then
                Return BitTreeDecode(low, posState << 3, 3)
            ElseIf _rc.DecodeBit(choice2, 0) = 0UI Then
                Return 8UI + BitTreeDecode(mid, posState << 3, 3)
            Else
                Return 16UI + BitTreeDecode(high, 0, 8)
            End If
        End Function

        ''' <summary>Forward bit-tree decode; returns the numBits-bit value.</summary>
        Private Function BitTreeDecode(probs As UShort(), baseIndex As Integer, numBits As Integer) As UInteger
            Dim m = 1
            For i = 1 To numBits
                m = (m << 1) + CInt(_rc.DecodeBit(probs, baseIndex + m))
            Next
            Return CUInt(m - (1 << numBits))
        End Function

        ''' <summary>Reverse bit-tree decode; returns the numBits-bit value, LSB first.</summary>
        Private Function BitTreeReverseDecode(probs As UShort(), baseIndex As Integer, numBits As Integer) As UInteger
            Dim m = 1
            Dim symbol = 0UI
            For i = 0 To numBits - 1
                Dim bit = _rc.DecodeBit(probs, baseIndex + m)
                m = (m << 1) + CInt(bit)
                symbol = symbol Or (bit << i)
            Next
            Return symbol
        End Function

        Private Function DecodeDistance(lenToPosState As Integer) As UInteger
            ' position slot: 6-bit forward tree
            Dim m = 1
            For i = 1 To 6
                m = (m << 1) + CInt(_rc.DecodeBit(_posSlot, (lenToPosState << 6) + m))
            Next
            Dim posSlot = CUInt(m - 64)

            If posSlot < kStartPosModelIndex Then
                Return posSlot
            End If

            Dim numDirectBits = CInt((posSlot >> 1) - 1UI)
            Dim dist = (2UI Or (posSlot And 1UI)) << numDirectBits

            If posSlot < kEndPosModelIndex Then
                Return dist + BitTreeReverseDecode(_posDecoders, CInt(dist - posSlot), numDirectBits)
            Else
                dist += _rc.DecodeDirectBits(numDirectBits - kNumAlignBits) << kNumAlignBits
                dist += BitTreeReverseDecode(_align, 0, kNumAlignBits)
                Return dist
            End If
        End Function

        Private Sub InitProbs()
            For Each t In {_isMatch, _isRep, _isRepG0, _isRepG1, _isRepG2, _isRep0Long,
                       _posSlot, _posDecoders, _align, _litProbs,
                       _lenChoice, _lenChoice2, _lenLow, _lenMid, _lenHigh,
                       _repChoice, _repChoice2, _repLenLow, _repLenMid, _repLenHigh}
                For i = 0 To t.Length - 1
                    t(i) = 1024US
                Next
            Next
            _state = 0
            _reps(0) = 0UI : _reps(1) = 0UI : _reps(2) = 0UI : _reps(3) = 0UI
            _dictPos = 0
            _total = 0UL
            _markerSeen = False
        End Sub

    End Class

    ''' <summary>
    ''' The LZMA binary range decoder (carryless range coder, 32-bit range,
    ''' normalisation threshold 2^24).
    ''' </summary>
    Friend NotInheritable Class LzmaRangeDecoder

        Private Const kTopValue As UInteger = 1UI << 24

        Private ReadOnly _input As Stream
        Private _range As UInteger
        Private _code As UInteger

        Public Sub New(input As Stream)
            _input = input
            _range = UInteger.MaxValue
            _code = 0UI

            Dim first = input.ReadByte()
            If first < 0 Then
                Throw New InvalidDataException("LZMA: the stream ends before the range coder header.")
            End If
            If first <> 0 Then
                Throw New InvalidDataException("LZMA: the first byte of the range coder header must be zero.")
            End If

            For i = 1 To 4
                Dim b = input.ReadByte()
                If b < 0 Then
                    Throw New InvalidDataException("LZMA: the range coder header is truncated.")
                End If
                _code = (_code << 8) Or CUInt(b)
            Next
        End Sub

        ''' <summary>Decodes one bit against an 11-bit probability, updating the model.</summary>
        Public Function DecodeBit(probs As UShort(), index As Integer) As UInteger
            Const kNumMoveBits As Integer = 5
            Dim v = CUInt(probs(index))
            Dim bound = (_range >> 11) * v
            Dim bit As UInteger

            If _code < bound Then
                probs(index) = CUShort(v + ((2048UI - v) >> kNumMoveBits))
                _range = bound
                bit = 0UI
            Else
                probs(index) = CUShort(v - (v >> kNumMoveBits))
                _code -= bound
                _range -= bound
                bit = 1UI
            End If

            Normalize()
            Return bit
        End Function

        ''' <summary>Decodes numBits bits with uniform (1/2) probability, MSB first.</summary>
        Public Function DecodeDirectBits(numBits As Integer) As UInteger
            Dim result = 0UI
            For i = 1 To numBits
                _range >>= 1
                _code -= _range
                result <<= 1
                If (_code And &H80000000UI) <> 0UI Then
                    ' underflow -> bit is 0; restore the code word
                    _code += _range
                Else
                    result += 1UI
                End If
                Normalize()
            Next
            Return result
        End Function

        Private Sub Normalize()
            If _range < kTopValue Then
                _range <<= 8
                Dim b = _input.ReadByte()
                If b < 0 Then
                    Throw New InvalidDataException("LZMA: the input stream is truncated inside the range-coded data.")
                End If
                _code = (_code << 8) Or CUInt(b)
            End If
        End Sub

    End Class
End Namespace