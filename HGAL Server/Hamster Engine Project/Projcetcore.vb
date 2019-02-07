Imports Hamster_Engine_Project.HE_Common_Component

'Hamster Engine v2.0용 Project Adapter
Public Class Project
    Public Shared Version As New HamsterVersion("HGAL Server", 1, 0, 190207, 40) '이 부분을 수정해 프로젝트의 버전을 수정하세요.
    '("개체 식별자 문자열", {"어셈블리 경로"})
    Private Shared LoadObject_PROJ_Info As New Dictionary(Of String, Object())

    Private Shared MainUIform As New frmMain
    Public Shared HEsock As Object = Nothing
    Public Shared HEfile As Object = Nothing

    Public Sub initialization(ByRef engineAssembly As Dictionary(Of String, Object), ByRef engineFucntion As Dictionary(Of String, [Delegate]), ByRef engineProperty As Dictionary(Of String, Boolean), ByVal engineArgs As Object(), ByRef projectAssembly As Dictionary(Of String, Object()), ByRef projectReference As Dictionary(Of String, Object()))
#Region "WrapperInit"
        EngineWrapper.EngineFunction.EFUNC_EngineShutdown = engineFucntion("EFUNC_EngineShutdown")
        EngineWrapper.EngineFunction.EFUNC_LogWrite = engineFucntion("EFUNC_LogWrite")
        EngineWrapper.EngineFunction.EFUNC_LogWriteP = engineFucntion("EFUNC_LogWriteP")
        EngineWrapper.EngineFunction.EFUNC_SetMainForm = engineFucntion("EFUNC_SetMainForm")
        EngineWrapper.EngineFunction.EFUNC_ShowWarningMsg = engineFucntion("EFUNC_ShowWarningMsg")
        EngineWrapper.EngineFunction.EFUNC_ShowWarningMsgE = engineFucntion("EFUNC_ShowWarningMsgE")
        EngineWrapper.EngineFunction.EFUNC_ShowWarningMsgS = engineFucntion("EFUNC_ShowWarningMsgS")
        EngineWrapper.EngineFunction.EFUNC_ShowErrorMsg = engineFucntion("EFUNC_ShowErrorMsg")
        EngineWrapper.EngineFunction.EFUNC_ShowErrorMsgE = engineFucntion("EFUNC_ShowErrorMsgE")
        EngineWrapper.EngineFunction.EFUNC_ShowErrorMsgS = engineFucntion("EFUNC_ShowErrorMsgS")
        EngineWrapper.EngineFunction.EFUNC_GetEngineIcon = engineFucntion("EFUNC_GetEngineIcon")
        EngineWrapper.EngineArgument.ApplicationStartupPath = engineArgs(0).ToString
        EngineWrapper.EngineArgument.Version = engineArgs(1)
#End Region
        EngineWrapper.EngineFunction.EFUNC_SetMainForm.DynamicInvoke(MainUIform)
        HEsock = engineAssembly("Hamster_Engine_Socket.HamsterSocket").CopyMe()
        HEfile = engineAssembly("Hamster_Engine_File.File").CopyMe()
        engineProperty("EPROP_Msgbox_ShowUnhandledException") = False
        projectReference.Add("Newtonsoft.Json.Linq", {EngineWrapper.EngineArgument.ApplicationStartupPath & "\library\Newtonsoft.Json.dll"})
    End Sub

    Public Sub main(ProjsideAsm As Dictionary(Of String, Object))
        MainUIform.Show()
        MainUIform.Text = Project.Version.GetName & " " & Project.Version.GetVersion(True)
    End Sub
End Class

Public Class EngineWrapper
    Public Class EngineFunction
        Public Shared EFUNC_EngineShutdown As [Delegate]
        Public Shared EFUNC_LogWrite As [Delegate]
        Public Shared EFUNC_LogWriteP As [Delegate]
        Public Shared EFUNC_SetMainForm As [Delegate]
        Public Shared EFUNC_ShowWarningMsg As [Delegate]
        Public Shared EFUNC_ShowWarningMsgE As [Delegate]
        Public Shared EFUNC_ShowWarningMsgS As [Delegate]
        Public Shared EFUNC_ShowErrorMsg As [Delegate]
        Public Shared EFUNC_ShowErrorMsgE As [Delegate]
        Public Shared EFUNC_ShowErrorMsgS As [Delegate]
        Public Shared EFUNC_GetEngineIcon As [Delegate]
    End Class

    Public Class EngineArgument
        Public Shared ApplicationStartupPath As String
        Public Shared Version As Object
    End Class
End Class
