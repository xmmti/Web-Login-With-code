Imports System.Text.RegularExpressions
Imports System.Net
Imports System.IO
Imports System.Web.Script.Serialization
Imports System.ComponentModel

Public Class Form1
    Public cookie_ As New Cookies_instagram
    Dim user_agent As String = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36"
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim data_login As Object = IG_login(TextBox1.Text, TextBox2.Text)
        If IsArray(data_login) Then 'Secure
            ' data_login = {Url path, ig_did, csrftoken, mid}
            cookie_.Make_Temp_Cookie(data_login(3), data_login(2), data_login(1))
            Dim data_Secure = IG_GetSecureInfo(data_login(0), cookie_.get_temp_cookie)
            If IsArray(data_Secure) Then
                ' choice values
                Dim icheck As New List(Of Control)
                ' Get RadioButton from (Form2)
                For Each i As Control In Form2.Controls
                    If i.Name.Contains("RadioButton") Then
                        icheck.Add(i)
                    End If
                Next
                ' Put Data in RadioButtons
                For i As Integer = 0 To data_Secure(0) - 1
                    icheck(i).Text = data_Secure(1)(i)
                    icheck(i).Tag = data_Secure(2)(i)
                    icheck(i).Visible = True
                Next
                Form2.Show()
                'Put Data in Form2
                Form2.URL_path = data_login(0)
                Form2.Cookie_ = cookie_.get_temp_cookie
            Else
                ' Type of Secure is Unknown
                MsgBox("Error, Something is wrong with your account try login with browser and see..., And try click it's was me! if your account logged in instagram app already", MsgBoxStyle.Critical)
            End If
        ElseIf data_login = False Then 'Error
            MsgBox("Error, Worng info!!", MsgBoxStyle.Critical)
        ElseIf data_login Then 'Done Login
            MsgBox("logged !!", MsgBoxStyle.Information)
            Label3.Text = "logged"
        ElseIf Not Regex.IsMatch(data_login.ToString.ToLower, "true|false") Then
            MsgBox(data_login)
        ElseIf IsNumeric(data_login) Then
            MsgBox(data_login)
        End If
    End Sub

    Function IG_login(ByVal user As String, Password As String) As Object
        Dim _mid, ig_did, cs, _sessionid As String
        Dim dataPOST As String
        dataPOST = "username=" & user & "&enc_password=" & System.Uri.EscapeDataString("#PWD_INSTAGRAM_BROWSER:0:" & time_new() & ":" & Password) & "&queryParams={}&optIntoOneTap=false"
        Using w As New WebClient
            w.Headers.Add("Accept: */*")
            w.Headers.Add("Content-Type: application/x-www-form-urlencoded")
            w.Headers.Add("X-Instagram-AJAX: 1")
            w.Headers.Add("User-Agent", user_agent)
            w.Headers.Add("X-Requested-With: XMLHttpRequest")
            w.Headers.Add("X-CSRFToken: missing")
            w.Headers.Add("X-IG-App-ID: 936619743392459")
            w.Headers.Add("Sec-Fetch-Site: same-origin")
            w.Headers.Add("Sec-Fetch-Mode: cors")
            w.Headers.Add("Sec-Fetch-Dest: empty")
            Try
                Dim respon_ As String = w.UploadString("https://www.instagram.com/accounts/login/ajax/", "POST", dataPOST)
                If w.ResponseHeaders("Set-Cookie").ToString.Contains("sessionid") Then
                    ig_did = Regex.Match(w.ResponseHeaders("Set-Cookie"), "ig_did=(.*?);").Groups(1).Value
                    cs = Regex.Match(w.ResponseHeaders("Set-Cookie"), "csrftoken=(.*?);").Groups(1).Value
                    _mid = Regex.Match(w.ResponseHeaders("Set-Cookie"), "mid=(.*?);").Groups(1).Value
                    _sessionid = Regex.Match(w.ResponseHeaders("Set-Cookie"), "sessionid=(.*?);").Groups(1).Value
                    ' Get Cookies With Session_id
                    If cookie_.Set_cookies(user, _mid, cs, _sessionid, ig_did) Then
                        Return True
                    End If
                End If
            Catch ex As WebException
                Dim response As HttpWebResponse = DirectCast(ex.Response, HttpWebResponse)
                Dim respon_ As String = New StreamReader(ex.Response.GetResponseStream).ReadToEnd
                If respon_.Contains("checkpoint_url") Then
                    ' Get Temp Cookie
                    ig_did = Regex.Match(response.Headers("Set-Cookie"), "ig_did=(.*?);").Groups(1).Value
                    cs = Regex.Match(response.Headers("Set-Cookie"), "csrftoken=(.*?);").Groups(1).Value
                    _mid = Regex.Match(response.Headers("Set-Cookie"), "mid=(.*?);").Groups(1).Value
                    Dim JSONRSP As Object = New JavaScriptSerializer().Deserialize(Of Object)(respon_)
                    Return {JSONRSP("checkpoint_url"), ig_did, cs, _mid}
                ElseIf respon_.Contains("""status"": ""fail""") AndAlso respon_.Contains("error_type") Then
                    Dim JSONRSP As Object = New JavaScriptSerializer().Deserialize(Of Object)(respon_)
                ElseIf respon_.Contains("Please wait a few minutes") Then
                    MsgBox("429 Too Many Requests")
                End If
            End Try
            Return False
        End Using
    End Function

    Function IG_GetSecureInfo(ByVal URL_ As String, Cookies As String) As Object
        Using w As New WebClient
            w.Headers.Add("Accept: */*")
            w.Headers.Add("Content-Type: application/x-www-form-urlencoded")
            w.Headers.Add("User-Agent", user_agent)
            w.Headers.Add("Cookie", Cookies)
            Try
                Dim respon_ As String = w.DownloadString("https://i.instagram.com" & URL_ & "?__a=1")
                Dim JSONRSP As Object = New JavaScriptSerializer().Deserialize(Of Object)(respon_)
                Dim count_ As Integer = 0
                Dim data_, value_ As New List(Of String)
                If JSONRSP("challengeType") = "RecaptchaChallengeForm" Then
                    MsgBox("Recaptcha!", MsgBoxStyle.Critical)
                ElseIf JSONRSP("challengeType") = "SubmitPhoneNumberForm" Then
                    MsgBox("Need Phone Number!", MsgBoxStyle.Critical)
                Else
                    If Not Regex.IsMatch(New JavaScriptSerializer().Serialize(JSONRSP("extraData")), ("To secure your account, you need to request help logging in.")) Then
                        If JSONRSP("challengeType") = "SelectVerificationMethodForm" Then
                            For Each i As Object In JSONRSP("extraData")("content")(3)("fields")(0)("values")
                                data_.Add(i("label"))
                                value_.Add(i("value"))
                                count_ += 1
                            Next
                            Return {count_, data_, value_}
                        Else

                        End If
                    Else
                        MsgBox(JSONRSP("extraData")("content")(1)("text"), MsgBoxStyle.Critical)
                    End If
                End If

            Catch ex As WebException
                Dim response As HttpWebResponse = DirectCast(ex.Response, HttpWebResponse)
                Dim respon_ As String = New StreamReader(ex.Response.GetResponseStream).ReadToEnd
                If respon_.Contains("""status"": ""fail""") AndAlso respon_.Contains("error_type") Then
                    Dim JSONRSP As Object = New JavaScriptSerializer().Deserialize(Of Object)(respon_)
                    Return (JSONRSP("error_type"))
                End If
                Return response.StatusCode
            End Try
            Return False
        End Using
    End Function
    Public Function IG_Send_Code(_choice As String, cookies As String, url_ As String) As Boolean
        Dim dataPOST As String
        dataPOST = "choice=" & _choice
        Using w As New WebClient
            w.Headers.Add("Accept: */*")
            w.Headers.Add("Content-Type: application/x-www-form-urlencoded")
            w.Headers.Add("X-Instagram-AJAX: 1")
            w.Headers.Add("User-Agent", user_agent)
            w.Headers.Add("X-Requested-With: XMLHttpRequest")
            w.Headers.Add("X-CSRFToken", Regex.Match(cookies, "csrftoken=(.*?);").Groups(1).Value)
            w.Headers.Add("X-IG-App-ID: 936619743392459")
            w.Headers.Add("Cookie", cookies)
            w.Headers.Add("Sec-Fetch-Site: same-origin")
            w.Headers.Add("Sec-Fetch-Mode: cors")
            w.Headers.Add("Sec-Fetch-Dest: empty")
            Try
                Dim respon_ As String = w.UploadString("https://i.instagram.com" & url_, "POST", dataPOST)
                If Regex.IsMatch(respon_, """challengeType"": ""(VerifySMSCodeForm|VerifyEmailCodeForm)") Then
                    Return True
                End If
            Catch ex As WebException
                Dim response As HttpWebResponse = DirectCast(ex.Response, HttpWebResponse)
                'Return response.StatusCode
            End Try
            Return False
        End Using
    End Function
    Public Function IG_Enter_Code(_Code As String, cookies As String, url_ As String) As Boolean
        Dim ig_did, cs, _sessionid As String
        Dim dataPOST As String
        dataPOST = "security_code=" & _Code
        Using w As New WebClient
            w.Headers.Add("Accept: */*")
            w.Headers.Add("Content-Type: application/x-www-form-urlencoded")
            w.Headers.Add("X-Instagram-AJAX: 1")
            w.Headers.Add("User-Agent", user_agent)
            w.Headers.Add("X-Requested-With: XMLHttpRequest")
            w.Headers.Add("X-CSRFToken", Regex.Match(cookies, "csrftoken=(.*?);").Groups(1).Value)
            w.Headers.Add("X-IG-App-ID: 936619743392459")
            w.Headers.Add("Cookie", cookies)
            w.Headers.Add("Sec-Fetch-Site: same-origin")
            w.Headers.Add("Sec-Fetch-Mode: cors")
            w.Headers.Add("Sec-Fetch-Dest: empty")
            Try
                Dim respon_ As String = w.UploadString("https://i.instagram.com" & url_, "POST", dataPOST)
                If Regex.IsMatch(respon_, """status"": ""ok""") Then
                    If w.ResponseHeaders("Set-Cookie").ToString.Contains("sessionid") Then
                        ig_did = Regex.Match(cookie_.get_temp_cookie, "ig_did=(.*?);").Groups(1).Value
                        cs = Regex.Match(w.ResponseHeaders("Set-Cookie"), "csrftoken=(.*?);").Groups(1).Value
                        '_mid = Regex.Match(w.ResponseHeaders("Set-Cookie"), "mid=(.*?);").Groups(1).Value
                        _sessionid = Regex.Match(w.ResponseHeaders("Set-Cookie"), "sessionid=(.*?);").Groups(1).Value
                        If cookie_.Set_cookies(TextBox1.Text, Regex.Match(cookie_.get_temp_cookie, "mid=(.*?);").Groups(1).Value, cs, _sessionid, ig_did) Then
                            Return True
                        End If
                    End If
                End If
            Catch ex As WebException
                Dim response As HttpWebResponse = DirectCast(ex.Response, HttpWebResponse)
                'Return response.StatusCode
            End Try
            Return False
        End Using
    End Function

    Function time_new() As String
        Dim time_is As String = (DateTime.UtcNow.Subtract(New DateTime(1970, 1, 1)).TotalMilliseconds)
        Return time_is.Split(".")(0)
    End Function
    '---> String Cookies <---
    Class Cookies_instagram
        Public Shared _fullStringCookies As String = String.Empty
        Public Shared id_account As String = String.Empty
        Public Shared tempCookie As String = String.Empty
        Sub New() : End Sub
        Function Set_cookies(Username_ As String, mid_ As String, CSRFToken_ As String, sessionid_ As String, Optional ByVal Ig_did As String = "0") As Boolean
            Dim data_cookie() As String
            If Ig_did = "0" Then
                data_cookie = {Username_, mid_, CSRFToken_, sessionid_}
            Else
                data_cookie = {Username_, mid_, CSRFToken_, sessionid_, Ig_did}
            End If
            Dim is_true As Boolean = False
            ' Check All Function Parameter Isn't Null Or Empty
            For Each i In data_cookie
                If i.Length > 0 AndAlso Not String.IsNullOrEmpty(i) Then
                    is_true = True
                Else
                    Return False : End If
            Next
            If is_true Then
                Try
                    ' Get ID_Account From SessionID 
                    id_account = System.Uri.UnescapeDataString(sessionid_).Split(":")(0)
                    ' Check If ID_Account is true
                    If IsNumeric(id_account) Then
                        If data_cookie.ToArray.Length > 4 Then
                            _fullStringCookies = String.Format("Cookie: ig_did={4}; mid={1}; csrftoken={2}; ds_user_id=" & id_account & "; sessionid={3}", data_cookie)
                        Else
                            _fullStringCookies = String.Format("Cookie: mid={1}; csrftoken={2}; ds_user_id=" & id_account & "; sessionid={3}", data_cookie)
                        End If
                        Return is_true : End If
                Catch ex As Exception : End Try
            End If : Return False
        End Function
        Sub Make_Temp_Cookie(mid_ As String, cs_ As String, Optional ig_did As String = "0", Optional rur As String = "FRC")
            Try
                If ig_did = "0" Then
                    tempCookie = "csrftoken=" & cs_ & "; mid=" & mid_ & "; rur=" & rur & ""
                Else
                    tempCookie = "ig_did=" & ig_did & "; csrftoken=" & cs_ & "; mid=" & mid_ & "; rur=" & rur & ""
                End If
            Catch ex As Exception : End Try
        End Sub
        Function get_temp_cookie()
            Return tempCookie
        End Function
        Function get_cookies() As String
            Return _fullStringCookies.ToString()
        End Function
    End Class

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        TextBox2.UseSystemPasswordChar = Not CheckBox1.Checked
    End Sub
    Function IG_logOut(ByVal cookies_ As String)
        Dim w As New WebClient
        w.Headers.Add("X-Instagram-AJAX: 1")
        w.Headers.Add("Content-Type: application/x-www-form-urlencoded")
        w.Headers.Add("Accept: */*")
        w.Headers.Add("User-Agent", user_agent)
        w.Headers.Add("X-Requested-With: XMLHttpRequest")
        w.Headers.Add("X-CSRFToken", Regex.Match(cookies_, "csrftoken=(.*?)(;|$)").Groups(1).Value)
        w.Headers.Add("Sec-Fetch-Site: same-origin")
        w.Headers.Add("Sec-Fetch-Mode: cors")
        w.Headers.Add("Sec-Fetch-Dest: empty")
        w.Headers.Add(cookies_)
        Dim respone As String = w.UploadString("https://www.instagram.com/accounts/logout/ajax/", "POST", "one_tap_app_login=0")
    End Function

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If cookie_.get_cookies().ToString.Length > 0 Then
            IG_logOut(cookie_.get_cookies())
        End If
        End
    End Sub
End Class
