<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form은 Dispose를 재정의하여 구성 요소 목록을 정리합니다.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows Form 디자이너에 필요합니다.
    Private components As System.ComponentModel.IContainer

    '참고: 다음 프로시저는 Windows Form 디자이너에 필요합니다.
    '수정하려면 Windows Form 디자이너를 사용하십시오.  
    '코드 편집기에서는 수정하지 마세요.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lstLog = New System.Windows.Forms.ListBox()
        Me.txtSentBytes = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lstLog
        '
        Me.lstLog.Font = New System.Drawing.Font("맑은 고딕", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.lstLog.FormattingEnabled = True
        Me.lstLog.HorizontalExtent = 700
        Me.lstLog.HorizontalScrollbar = True
        Me.lstLog.ItemHeight = 21
        Me.lstLog.Location = New System.Drawing.Point(13, 13)
        Me.lstLog.Name = "lstLog"
        Me.lstLog.Size = New System.Drawing.Size(793, 361)
        Me.lstLog.TabIndex = 0
        '
        'txtSentBytes
        '
        Me.txtSentBytes.AutoSize = True
        Me.txtSentBytes.Font = New System.Drawing.Font("맑은 고딕", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.txtSentBytes.Location = New System.Drawing.Point(12, 387)
        Me.txtSentBytes.Name = "txtSentBytes"
        Me.txtSentBytes.Size = New System.Drawing.Size(98, 28)
        Me.txtSentBytes.TabIndex = 1
        Me.txtSentBytes.Text = "총 전송 : "
        '
        'frmMain
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(818, 431)
        Me.Controls.Add(Me.txtSentBytes)
        Me.Controls.Add(Me.lstLog)
        Me.MaximumSize = New System.Drawing.Size(840, 487)
        Me.MinimumSize = New System.Drawing.Size(840, 487)
        Me.Name = "frmMain"
        Me.Text = "frmMain"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lstLog As System.Windows.Forms.ListBox
    Friend WithEvents txtSentBytes As System.Windows.Forms.Label
End Class
