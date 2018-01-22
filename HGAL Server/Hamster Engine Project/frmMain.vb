Imports System.Reflection

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
        Print("UI 로드 완료")
        'Print(Engine.ProjVersion.GetName & "  " & Engine.ProjVersion.GetVersion(True))
        Print("소켓을 초기화합니다.")

        Try
            ServerSoc = Project.Hamsoc.CopyMe()
            Print("0.0.0.0:81에서 Listen모드로 소켓을 초기화합니다.")
            ServerSoc.Init(True, "0.0.0.0", 81, New socevt(AddressOf SocListen))
            Print("Listen을 시작합니다")
            ServerSoc.SetListen()
        Catch ex As Exception
            Print("소켓을 초기화하는중 오류가 발생했습니다!!")
            Print(ex.ToString)
            MsgBox(ex.ToString)
        End Try
    End Sub

    Public Sub SocListen(kind As Object, args() As Object)
        Print(kind.ToString & " / " & args(0).ToString)
        If kind = 0 Then 'Connect
        ElseIf kind = 1 Then 'ConnectListen
        ElseIf kind = 2 Then 'Disconnect
        ElseIf kind = 3 Then 'Listen
            Print(System.Text.Encoding.UTF8.GetString(args(0)))
        End If
    End Sub

    Public Sub Print(data As String)
        lstLog.Items.Add(data)
        lstLog.SelectedIndex = lstLog.Items.Count - 1
    End Sub
End Class