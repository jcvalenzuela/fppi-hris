Imports DPFP
Imports System.IO
Imports System.IO.FileStream
Imports System.Data.SqlClient
Public Class frmDailyTimeMonitoring
    Dim listofarray As New ArrayList
    Dim dta As Integer
    Dim di As DateInterval = 1000
    Dim msTrm As New MemoryStream
    Dim arrPicture As Byte()
    Dim bytes As Byte()

    Private WithEvents verifyControl As Gui.Verification.VerificationControl
    Private matcher As Verification.Verification
    Private matchResult As Verification.Verification.Result
    Private allReaderSerial As String = "00000000-0000-0000-0000-000000000000"
    Private template As Template
    Dim ms As MemoryStream = New MemoryStream
    Private Event OnTemplate(ByVal template)
    Private Enroller As Processing.Enrollment
    Private Sub CreateDPControl(ByRef control As DPFP.Gui.Verification.VerificationControl)
        Try
            control = New Gui.Verification.VerificationControl()
            control.AutoSizeMode = AutoSizeMode.GrowAndShrink
            control.Name = "verifyControl"
            control.Location = New Point(0, 0)
            control.ReaderSerialNumber = "00000000-0000-0000-0000-000000000000"
            control.Visible = True
            control.Enabled = True
            Me.Controls.Add(control)
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub verifyControl_OnComplete(ByVal Control As Object, ByVal FeatureSet As FeatureSet, ByRef EventHandlerStatus As Gui.EventHandlerStatus) Handles verifyControl.OnComplete
        Try
            Dim fp
            Using conn = New SqlConnection(conString)
                conn.Open()
                sqlqry = "SELECT FingerPrint FROM tblMasterWorkInfo WHERE DATALENGTH(FingerPrint)>0"
                cmd = New SqlCommand(sqlqry, conn)
                cmd = conn.CreateCommand
                With cmd
                    .Connection = conn
                    .CommandText = sqlqry
                    .CommandType = CommandType.Text
                End With
                dr = cmd.ExecuteReader()
                While dr.Read()
                    listofarray.Add(dr(dta))
                End While
                dr.Close()
                conn.Close()

                For Each usr In listofarray
                    template = New Template()
                    template.DeSerialize(usr)
                    matcher.Verify(FeatureSet, template, matchResult)

                    If matchResult.Verified Then
                        EventHandlerStatus = Gui.EventHandlerStatus.Success
                        conn.Open()
                        Dim sqlStr As String = "SELECT * FROM tblMasterWorkInfo WHERE FingerPrint = @FingerPrint"
                        cmd = New SqlCommand(sqlStr, conn)
                        cmd = conn.CreateCommand

                        With cmd
                            .Connection = conn
                            .CommandText = sqlStr
                            .Parameters.AddWithValue("@FingerPrint", usr)
                        End With
                        dr = cmd.ExecuteReader
                        While dr.Read()
                            fp = usr
                            lblEmployeeID.Text = dr("EmployeeID").ToString
                            lblFullName.Text = dr("FullName").ToString
                            lblDepartment.Text = dr("Department").ToString
                            Dim data As Byte() = DirectCast(dr("EmployeePic"), Byte())
                            Dim mStrm As New MemoryStream(data)
                            EmployeePic.Image = Image.FromStream(mStrm)
                            Try
                                dr.close()
                                Dim sSQL As String = "SELECT * FROM tblDailyTimeRecord WHERE EmployeeID = @EmployeeID AND FullName = @FullName AND WorkDate = @WorkDate"
                                cmd = New SqlCommand(sSQL, conn)
                                cmd = conn.CreateCommand
                                With cmd
                                    .Connection = conn
                                    .CommandText = sSQL
                                    .CommandType = CommandType.Text
                                    .Parameters.AddWithValue("@EmployeeID", lblEmployeeID.Text)
                                    .Parameters.AddWithValue("@FullName", lblFullName.Text)
                                    .Parameters.AddWithValue("@WorkDate", Date.Now.ToShortDateString)

                                End With
                                dr = cmd.ExecuteReader()
                                If dr.Read() Then
                                    lblEmployeeID.Text = dr.Item("EmployeeID").ToString
                                    lblFullName.Text = dr.Item("FullName").ToString
                                    lblDepartment.Text = dr.Item("Department").ToString
                                    lblTimeIN.Text = dr.Item("TimeIN").ToString
                                    lblTimeOUT.Text = dr.Item("TimeOUT").ToString

                                    If lblTimeOUT.Text = String.Empty Then
                                        TimeOUT()
                                    End If
                                Else
                                    TimeIN()
                                    Exit Sub
                                End If
                            Catch ex As Exception
                                MsgBox(ex.Message)
                            End Try
                        End While
                        dr.Close()
                        conn.Close()
                        ' lblStatus.Text = "Fingerprint Verified"
                        tmrClear.Enabled = True
                    End If
                    If lblEmployeeID.Text = String.Empty And lblFullName.Text = String.Empty And lblDepartment.Text = String.Empty Then
                        lblStatus.Text = "Fingerprint Not Found!"
                    End If
                Next
            End Using
        Catch ex As Exception
            EventHandlerStatus = Gui.EventHandlerStatus.Failure
            RaiseEvent OnTemplate(Enroller.Template)
            RaiseEvent OnTemplate(Nothing)
            ms = Nothing
            MessageBox.Show(ex.Message)
        End Try
    End Sub
    Private Sub tmrSystemTime_Tick(sender As Object, e As EventArgs) Handles tmrSystemTime.Tick

        DTTimeFormat("", lblTime)
        lblDate.Text = Now.ToString("dddd, MMMM dd, yyyy")
    End Sub
    Private Sub frmDailyTimeMonitoring_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        matcher = New Verification.Verification()
        matchResult = New Verification.Verification.Result
        CreateDPControl(verifyControl)
    End Sub

    Private Sub tmrClear_Tick(sender As Object, e As EventArgs) Handles tmrClear.Tick
        lblDepartment.Text = String.Empty
        lblEmployeeID.Text = String.Empty
        lblFullName.Text = String.Empty
        lblTimeIN.Text = String.Empty
        lblTimeOUT.Text = String.Empty
        lblStatus.Text = String.Empty
        tmrClear.Enabled = False
        EmployeePic.Image = My.Resources.boss_1
    End Sub


    Private Sub TimeIN()
        Using conn = New SqlConnection(conString)
            conn.Open()
            lblTimeIN.Text = DateTime.Now.ToString("HH:mm:ss tt")
            sSQL = "INSERT INTO tblDailyTimeRecord(EmployeeID, FullName, Department, WorkDate, TimeIN) VALUES(@EmployeeID, @FullName, @Department, @WorkDate, @TimeIN)"
            cmd = New SqlCommand(sSQL, conn)
            cmd = conn.CreateCommand
            With cmd
                .Connection = conn
                .CommandText = sSQL
                .CommandType = CommandType.Text
                .Parameters.AddWithValue("@EmployeeID", lblEmployeeID.Text)
                .Parameters.AddWithValue("@FullName", lblFullName.Text)
                .Parameters.AddWithValue("@Department", lblDepartment.Text)
                .Parameters.AddWithValue("@WorkDate", Date.Now.ToShortDateString)
                .Parameters.AddWithValue("@TimeIN", lblTimeIN.Text)
                .ExecuteNonQuery()
                .Dispose()
            End With
            My.Computer.Audio.Play(System.AppDomain.CurrentDomain.BaseDirectory & "\welcome.wav")
            lblStatus.Text = "Time-IN Success!"
            tmrClear.Enabled = True
        End Using

    End Sub


    Private Sub TimeOUT()
        Using conn = New SqlConnection(conString)
            conn.Open()
            lblTimeOUT.Text = DateTime.Now.ToString("HH:mm:ss tt")
            Dim sQl As String = "UPDATE tblDailyTimeRecord SET [TimeOUT]=@TimeOUT WHERE [EmployeeID]=@EmployeeID AND [FullName]=@FullName AND  [WorkDate]=@WorkDate"
            cmd = New SqlCommand(sQl, conn)
            cmd = conn.CreateCommand
            With cmd
                .Connection = conn
                .CommandText = sQl
                .CommandType = CommandType.Text
                .Parameters.AddWithValue("@EmployeeID", lblEmployeeID.Text)
                .Parameters.AddWithValue("@FullName", lblFullName.Text)
                .Parameters.AddWithValue("@WorkDate", Date.Now.ToShortDateString)
                .Parameters.AddWithValue("@TimeOUT", lblTimeOUT.Text)
                .ExecuteNonQuery()
                .Dispose()
            End With

            lblStatus.Text = "Time-OUT Success!"
            My.Computer.Audio.Play(System.AppDomain.CurrentDomain.BaseDirectory & "\goodbye.wav")
            tmrClear.Enabled = True
        End Using
    End Sub
End Class
