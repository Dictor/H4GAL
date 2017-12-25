Public Class HE_Common_Component
	Public Shared Version As New HamsterVersion("Hamster Common Component", 1, 0, 170323, 2)
	
	#region "HamsterReturn"
    Public Class HamsterReturn
        Private ProgressResult As Boolean
        Private RtrData As Object()
        Private ErrException As Exception
        Private ErrDescription As String
        Private ErrName As String

        Public Sub New(ProgRes As Boolean)
            ProgressResult = ProgRes
        End Sub

        Public Sub New(ProgRes As Boolean, ErrorName As String)
            ProgressResult = ProgRes
        End Sub

        Public Sub New(ProgRes As Boolean, Returndata As Object())
            ProgressResult = ProgRes
            Returndata = Returndata
        End Sub
        
        Public Sub New(ProgRes As Boolean, Returndata As Object)
            ProgressResult = ProgRes
            Returndata = Returndata
        End Sub

        Public Sub New(ProgRes As Boolean, ErrorException As Exception, ErrorName As String)
            ProgressResult = ProgRes
            ErrException = ErrorException
            ErrName = ErrorName
            ErrDescription = ErrDescription
        End Sub

        Public Sub New(ProgRes As Boolean, ErrorException As Exception, ErrorName As String, ErrorDescription As String)
            ProgressResult = ProgRes
            ErrException = ErrorException
            ErrName = ErrorName
            ErrDescription = ErrDescription
        End Sub



        Public Sub New(ProgRes As Boolean, ReturnData As Object(), ErrorException As Exception)
            ProgressResult = ProgRes
            RtrData = ReturnData
            ErrException = ErrorException
        End Sub

        Public Sub New(ProgRes As Boolean, ReturnData As Object(), ErrorException As Exception, ErrorName As String, ErrorDescription As String)
            ProgressResult = ProgRes
            RtrData = ReturnData
            ErrException = ErrorException
            ErrName = ErrorName
            ErrDescription = ErrDescription
        End Sub

        Public Sub New(ProgRes As Boolean, ReturnData As Object(), ErrorException As Exception, ErrorName As String)
            ProgressResult = ProgRes
            RtrData = ReturnData
            ErrException = ErrorException
            ErrName = ErrorName
        End Sub

        Public Function GetResult() As Boolean
            Return ProgressResult
        End Function

        Public Function GetData() As Object()
            Return RtrData
        End Function

        Public Function GetException() As Exception
            Return ErrException
        End Function

        Public Function GetExceptionInfo(ErrDesc As Boolean) As String
            If ErrDesc Then
                Return ErrDescription
            Else
                Return ErrName
            End If
        End Function

    End Class
	#End region

	#region "HamsterVersion"
    Public Class HamsterVersion '버전정보를 담기위한 클래스
        Private StrVersion As String '문자열 버젼
        Private MajorVersion, MinorVersion, BuildDateNumber, BuildCountNumber As Integer '메이저버젼,마이너버전,빌드번호,수정번호
        Private ModuleName As String '모듈이름

        Private setStatus As Boolean = False '초기화 되지 않았거나, 정의하지 않았거나, 비포함의 경우 FALSE
        Private setStatusEXP As Byte = 0 'setStatus가 false일경우, 0=초기화 되지않음, 1=정의하지 않음, 2=포함하지 않음


        Public Sub New(ModuleNam As String, MajorVer As Byte, MinorVer As Byte, BuildNum As Integer, ChangeNum As Integer)
            '버젼을 초기화 합니다         
            setStatus = True

            ModuleName = ModuleNam
            MajorVersion = MajorVer
            MinorVersion = MinorVer
            BuildDateNumber = BuildNum
            BuildCountNumber = ChangeNum
        End Sub


        Public Sub SetVersion(NotDefine As Boolean)
            '정의하지 않았거나, 포함하지 않았을 경우 FALSE를 넘겨주면 비포함, TRUE는 정의되지 않음 입니다
            MajorVersion = 0
            MinorVersion = 0
            BuildDateNumber = 0
            BuildCountNumber = 0

            If NotDefine Then
                setStatus = False
                setStatusEXP = 1
            Else
                setStatus = False
                setStatusEXP = 2
            End If
        End Sub

        Public Function GetVersion(returnString As Boolean) As Object
            'returnString이 TRUE 일경우 문자열로 버젼을 반환하며, FALSE 일 경우 정수 배열로 반환합니다
            If setStatusEXP = 0 And setStatus = False Then
                Throw New VersionNotSetException
            End If

            If returnString Then
                Dim ver As String = MajorVersion & "." & MinorVersion & "." & BuildDateNumber & "." & BuildCountNumber
                Return ver
            Else
                Dim ver As Integer() = {MajorVersion, MinorVersion, BuildDateNumber, BuildCountNumber}
                Return ver
            End If
        End Function

        Public Function GetName() As String
            Return ModuleName
        End Function

    End Class
    #End Region
    
    #region "HamsterCustomException"
    '햄스터 엔진 커스텀 예외
    Public Class VersionNotSetException
        Inherits Exception

        Public Const expMessage As String = "[햄스터 엔진 내부 예외]버젼이 초기화되지 않았습니다, 초기화되지 않은 버젼객체에 엑세스하려고 했습니다"

        Sub New()
            MyBase.New(expMessage)
        End Sub
    End Class
    #End Region
End Class