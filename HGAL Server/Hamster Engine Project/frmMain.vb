Imports System.Reflection
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions

Public Class frmMain
    Private Class SessionData
        Public ID As String
        Public MadeTime As Date
        Public AuthID As String
        Public AuthExisted As Boolean = False
    End Class

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler Me.Shown, AddressOf initproc
    End Sub

    Private ServerSoc As Object
    Delegate Sub socevt(kind As Object, args() As Object)
    Public Sub initproc()
        Print("[INIT]UI 로드 완료")
        Print(Project.Version.GetName & "  " & Project.Version.GetVersion(True))
        Try
            ServerSoc = Project.Hamsoc.CopyMe()
            Print("[INIT]0.0.0.0:81에서 Listen모드로 소켓을 초기화합니다.")
            ServerSoc.Init(True, "0.0.0.0", 81, New socevt(AddressOf SocListen))
            Print("[INIT]Listen을 시작합니다")
            ServerSoc.SetListen()
            Print("[INIT]소켓 시작 작업 완료")
        Catch ex As Exception
            Print("[ERROR]소켓을 초기화하는중 오류가 발생했습니다!!")
            Print(ex.ToString)
        End Try
    End Sub

    Public Sub SocListen(kind As Object, args() As Object)
        If kind = 0 Then 'Connect
            Print("[SOCKET] 어떤 클라이언트가 접속에 실패함")
        ElseIf kind = 1 Then 'ConnectListen
            Print("[SOCKET]" & args(1).ToString & "에서 접속, " & args(2) & "번에 소켓에 할당됨")
        ElseIf kind = 2 Then 'Disconnect
            Print("[SOCKET]" & args(0) & "번 소켓 연결 해제")
        ElseIf kind = 3 Then 'Listen
            Print("[SOCKET]" & args(1) & "번 소켓에서 데이터 수신 : '" & System.Text.Encoding.UTF8.GetString(args(0)) & "'")
            ProcessMsg(args(0), args(1))
        End If
    End Sub

    Private Sub ProcessMsg(data As Byte(), socnum As Integer)
        Dim httpmsg = Encoding.UTF8.GetString(data)

        If New Regex("^GET").IsMatch(httpmsg) Then 'GET REQ시	
            Dim response As [Byte]() = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine + "Connection: Upgrade" + Environment.NewLine + "Upgrade: websocket" + Environment.NewLine + "Sec-WebSocket-Accept: " + Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(New Regex("Sec-WebSocket-Key: (.*)").Match(httpmsg).Groups(1).Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))) + Environment.NewLine + Environment.NewLine)
            ServerSoc.Send(response, socnum)
            Print("[SOCKET]" & socnum & "번 소켓에서 웹소켓 핸드셰이크")
        Else 'NON-GET REQ시
            Dim msg As String = DecodeMessage(data)
            Print("[SOCKET]" & socnum & "번 소켓에서 데이터 수신 : '" & msg & "'")
        End If
    End Sub

    Public Sub Print(data As String)
        lstLog.Items.Add(data)
        lstLog.SelectedIndex = lstLog.Items.Count - 1
        Project.LogWrite.DynamicInvoke("data")
    End Sub

    Private Sub lstLog_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstLog.SelectedIndexChanged

    End Sub

    Public Function EncodeMessage(data As String) As Byte()
        Dim lb = New List(Of Byte)()
        lb.Add(&H81)
        Dim bytedata As Byte() = Encoding.UTF8.GetBytes(data)
        Dim size = bytedata.Length

        If size > 127 Then
            Throw New Exception("단일프레임내 127바이트 이상의 데이터가 포함될수 없습니다. (Data must be less than 128 bytes)")
        End If

        lb.Add(CByte(size))
        lb.AddRange(bytedata)
        Return lb.ToArray
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
End Class