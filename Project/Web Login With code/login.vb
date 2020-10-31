Imports System.Net
Imports System.Web.Script.Serialization
Imports System.Management
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions

Public Class LOGIN
    Dim HWID As String
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If Not TextBox1.Text = String.Empty And Not TextBox2.Text = String.Empty Then
            Dim responString As String = (New WebClient).UploadString($"https://script.google.com/macros/s/AKfycbxP-h4rDjH8J8rWllqTQ-_UuZrmLcaXwvMIFWdMhUk5kPAA7cA/exec?login", "POST", New JavaScriptSerializer().Serialize(New Dictionary(Of String, Object) From {{"username", TextBox1.Text}, {"password", TextBox2.Text}}))
            Try
                If (New JavaScriptSerializer).DeserializeObject(responString)("msg") Then
                    Label1.Text = "Hi " & TextBox1.Text & " :)"
                    Task.Delay(1700)
                    Form1.Show()
                    Me.Hide()
                Else
                    Label1.Text = "wrong info!"
                End If
            Catch ex As Exception
                Label1.Text = "Error!"
            End Try
        Else
            MessageBox.Show("Username/Password isn't entered", "double check!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk)
        End If

    End Sub
    Sub New()
        HWID = String.Join("-", Regex.Split(Get_HWID, "(?<=\G.{8})", RegexOptions.Singleline)).TrimEnd("-")
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
    End Sub
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        TextBox2.UseSystemPasswordChar = Not CheckBox1.Checked
    End Sub

    Private Sub LOGIN_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        TextBox3.Text = HWID
    End Sub

    Public Function Get_HWID() As String
        'Information Handler
        Dim hw As New clsComputerInfo
        'Decalre variables
        Dim hdd, cpu As String
        'Get all the values
        cpu = hw.GetProcessorId()
        hdd = hw.GetVolumeSerial("C")
        'Generate the hash
        Dim hwid As String = GenerateSHA512String(cpu & hdd)
        Return hwid
    End Function

    Public Shared Function GenerateSHA512String(ByVal inputString) As String
        Dim sha1 As SHA1 = SHA1Managed.Create
        Dim bytes As Byte() = Encoding.UTF8.GetBytes(inputString)
        Dim hash As Byte() = sha1.ComputeHash(bytes)
        Dim stringBuilder As New StringBuilder()
        For i As Integer = 0 To hash.Length - 1
            stringBuilder.Append(hash(i).ToString("X2"))
        Next
        Return stringBuilder.ToString()
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

    End Sub
End Class

Public Class clsComputerInfo
    Friend Function GetProcessorId() As String
        Dim strProcessorId As String = String.Empty
        Dim query As New SelectQuery("Win32_processor")
        Dim search As New ManagementObjectSearcher(query)
        Dim info As ManagementObject
        For Each info In search.Get()
            strProcessorId = info("processorId").ToString()
        Next
        Return strProcessorId
    End Function
    Friend Function GetVolumeSerial(Optional ByVal strDriveLetter As String = "C") As String
        Dim disk As ManagementObject = New ManagementObject(String.Format("win32_logicaldisk.deviceid=""{0}:""", strDriveLetter))
        disk.Get()
        Return disk("VolumeSerialNumber").ToString()
    End Function
End Class
