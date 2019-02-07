Imports System.Text

Module Websocket
    Public Function EncodeMessage(data As String) As Byte()
        Dim lb = New List(Of Byte)()
        lb.Add(&H81)
        Dim bytedata As Byte() = Encoding.UTF8.GetBytes(data)
        Dim size = bytedata.Length

        If size > 127 Then
            'Throw New Exception("단일프레임내 127바이트 이상의 데이터가 포함될수 없습니다. (Data must be less than 128 bytes)")
        End If

        lb.Add(CByte(size))
        lb.AddRange(bytedata)
        Return lb.ToArray
    End Function

    Public Enum messageType
        Continuos
        Text
        Binary
        Close
        Ping
        Pong
    End Enum

    Public Function CreateFrame(message As String, Optional messageType As messageType = messageType.Text, Optional messageContinues As Boolean = False) As Byte()
        Dim b1 As Byte = 0
        Dim b2 As Byte = 0
        Select Case messageType
            Case messageType.Continuos
                b1 = 0
            Case messageType.Text
                b1 = 1
            Case messageType.Binary
                b1 = 2
            Case messageType.Close
                b1 = 8
            Case messageType.Ping
                b1 = 9
            Case messageType.Pong
                b1 = 10
        End Select

        b1 = CByte((b1 + 128))
        Dim messageBytes As Byte() = Encoding.UTF8.GetBytes(message)
        If messageBytes.Length < 126 Then
            b2 = CByte(messageBytes.Length)
        Else
            If messageBytes.Length < Math.Pow(2, 16) - 1 Then
                b2 = 126
            Else
                b2 = 127
            End If
        End If

        Dim frame As Byte() = Nothing
        If b2 < 126 Then
            frame = New Byte(messageBytes.Length + 2 - 1) {}
            frame(0) = b1
            frame(1) = b2
            Array.Copy(messageBytes, 0, frame, 2, messageBytes.Length)
        End If

        If b2 = 126 Then
            frame = New Byte(messageBytes.Length + 4 - 1) {}
            frame(0) = b1
            frame(1) = b2
            Dim lenght As Byte() = BitConverter.GetBytes(messageBytes.Length)
            frame(2) = lenght(1)
            frame(3) = lenght(0)
            Array.Copy(messageBytes, 0, frame, 4, messageBytes.Length)
        End If

        If b2 = 127 Then
            frame = New Byte(messageBytes.Length + 10 - 1) {}
            frame(0) = b1
            frame(1) = b2
            Dim lenght As Byte() = BitConverter.GetBytes(CLng(messageBytes.Length))

            Dim i = 7, j = 2
            While i >= 0
                If Not i >= 0 Then
                    Exit While
                End If
                frame(j) = lenght(i)
                i -= 1
                j += 1
            End While
        End If

        Return frame
    End Function

    Public Function DecodeMessage(buffer As Byte()) As String
        Dim b As Byte = buffer(1)
        Dim dataLength As Integer = 0
        Dim totalLength As Integer = 0
        Dim keyIndex As Integer = 0

        If b - 128 <= 125 Then
            dataLength = b - 128
            keyIndex = 2
            totalLength = dataLength + 6
        End If

        If b - 128 = 126 Then
            dataLength = BitConverter.ToInt16(New Byte() {buffer(3), buffer(2)}, 0)
            keyIndex = 4
            totalLength = dataLength + 8
        End If

        If b - 128 = 127 Then
            dataLength = CInt(BitConverter.ToInt64(New Byte() {buffer(9), buffer(8), buffer(7), buffer(6), buffer(5), buffer(4),
                buffer(3), buffer(2)}, 0))
            keyIndex = 10
            totalLength = dataLength + 14
        End If

        Dim key As Byte() = New Byte() {buffer(keyIndex), buffer(keyIndex + 1), buffer(keyIndex + 2), buffer(keyIndex + 3)}

        Dim dataIndex As Integer = keyIndex + 4
        Dim count As Integer = 0
        For i As Integer = dataIndex To totalLength - 1
            buffer(i) = CByte(buffer(i) Xor key(count Mod 4))
            count += 1
        Next

        Return Encoding.ASCII.GetString(buffer, dataIndex, dataLength)
    End Function
End Module
