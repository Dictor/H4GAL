Imports Hamster_Engine_Project.HE_Common_Component

Public Class Project
    Public Shared Version As New HamsterVersion("HGAL Server", 0, 0, 180422, 11)

    '("개체 식별자 문자열", {"어셈블리 경로"})
    Private Shared LoadObject_PROJ_Info As New Dictionary(Of String, Object())

    Private Shared ApplicationStartupPath As String

    Private Shared MainUIform As New frmMain

    Public Shared LogWrite As [Delegate]

    Public Shared Hamsoc As Object
    Public Shared Hamfile As Object
    Public Function initialization(EngineAsm As Dictionary(Of String, Object), enginefunc As [Delegate]()(), args As Object()) As Dictionary(Of String, Object())
        ApplicationStartupPath = args(0)

        Dim setmainUi As [Delegate] = enginefunc(2)(0)
        LogWrite = enginefunc(1)(0)
        setmainUi.DynamicInvoke(MainUIform)
        Hamsoc = EngineAsm("Hamster_Engine_Socket.HamsterSocket").CopyMe()
        Hamfile = EngineAsm("Hamster_Engine_File.File").CopyMe()
        Return LoadObject_PROJ_Info
    End Function

    Public Sub main(ProjsideAsm As Dictionary(Of String, Object))
        MainUIform.Show()
        MainUIform.Text = Project.Version.GetName & " " & Project.Version.GetVersion(True)
    End Sub
End Class
