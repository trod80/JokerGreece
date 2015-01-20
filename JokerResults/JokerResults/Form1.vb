Imports System
Imports System.Net
Imports System.Xml
Imports System.IO
Imports System.Net.Mail

Public Class Form1

    Private JokerURLlatestDraw As String = "http://applications.opap.gr/DrawsRestServices/joker/last.xml"
    Private NumbersWin(5) As Integer
    Private JokerWin As Integer
    Private DisableAutoFeature As Boolean = False
    Private Shared WithEvents myTimer As New System.Windows.Forms.Timer()
    Dim tspn As New TimeSpan()


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ReturnWinningNumbers()
    End Sub





    Private Sub ReturnWinningNumbers()


        Dim request As HttpWebRequest
        Dim response As HttpWebResponse = Nothing

        Try

        
        ' Create the web request   
        request = DirectCast(WebRequest.Create(JokerURLlatestDraw), HttpWebRequest)
            request.Timeout = 3000


        ' Get response   
        response = DirectCast(request.GetResponse(), HttpWebResponse)
        Dim sreader As StreamReader = New StreamReader(response.GetResponseStream)


            Dim reader As XmlTextReader = New XmlTextReader(sreader)
            lblTime.Text = "Τελευταίος ελενγχός: " & DateAndTime.Now.ToLocalTime.ToString
            'Dim reader As XmlTextReader = New XmlTextReader(JokerURLlatestDraw) '
        Dim Counter As Integer = 0
        txtNumbers.Text = String.Empty
        txtJoker.Text = String.Empty
        Dim counterWin As Integer = 1

        Do While (reader.Read())

            Select Case Counter

                Case 3 'Draw Number
                    lblID.Text = reader.Value
                    'Console.WriteLine("Name:" & reader.Name)
                    'Console.WriteLine(Counter & " Value:" & reader.Value)
                Case 6 'Date
                    Dim DateSeperator() As String = reader.Value.ToString.Split("T")
                    lblDate.Text = DateSeperator(0)
                    'Console.WriteLine("Name:" & reader.Name)
                    'Console.WriteLine(Counter & " Value:" & reader.Value)
                Case 9, 12, 15, 18, 21 'Numbers

                    NumbersWin(counterWin) = reader.Value
                    counterWin += 1

                    If txtNumbers.Text = String.Empty Then
                        txtNumbers.Text = reader.Value
                    Else
                        txtNumbers.Text = txtNumbers.Text & ", " & reader.Value
                    End If

                    'Console.WriteLine("Name:" & reader.Name)
                    'Console.WriteLine(Counter & " Value:" & reader.Value)
                Case 24 'Joker
                    txtJoker.Text = reader.Value
                    JokerWin = reader.Value
                    'Console.WriteLine("Name:" & reader.Name)
                    'Console.WriteLine(Counter & " Value:" & reader.Value)
            End Select


            Counter += 1
        Loop
            ErrorLabel.Text = ""
        Catch ex As WebException
            lblTime.Text = "Τελευταίος ελενγχός: " & DateAndTime.Now.ToLocalTime.ToString
            ErrorLabel.Text = "OPAP Site Timeout. Please Try Again."
        End Try
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        DisableAutoFeature = True
    End Sub

   

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        txtEmail.Text = My.Settings.MyEmailAddress.ToString
        lblTime.Text = ""
        ErrorLabel.Text = ""

        DGV1.Columns.Add("ID", "Στήλη")
        DGV1.Columns.Add("Numbers", "Αριθμοί")
        DGV1.Columns.Add("Joker", "Τζόκερ")
        DGV1.Columns.Add("Success", "Επιτυχίες")

        chkEmail.Enabled = False


    End Sub

   
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim fd As OpenFileDialog = New OpenFileDialog()
        'Dim strFileName As String

        fd.Title = "Open File Dialog"
        fd.InitialDirectory = "C:\"
        fd.Filter = "All files (*.txt)|*.txt|All files (*.*)|*.*"
        fd.FilterIndex = 1
        fd.RestoreDirectory = True

        If fd.ShowDialog() = DialogResult.OK Then
            Dim SR As New StreamReader(fd.FileName)
            Dim Line As String = SR.ReadLine
            Dim Counter As Integer = 1
            Dim IsNotJoker As Boolean = True
            Dim ID As Integer
            Dim Numbers, JokerNumbers As String

            Do While (Not Line Is Nothing)

                If IsNotJoker Then
                    ID = Counter
                    Numbers = Line
                    IsNotJoker = False
                Else
                    JokerNumbers = Line
                    Counter += 1
                    IsNotJoker = True
                    Me.DGV1.Rows.Add(ID, Numbers, JokerNumbers, "")
                End If

                Line = SR.ReadLine
            Loop
            SR.Close()
        End If

    End Sub



    Private Sub CheckIfiWon()
        Dim NumbersToCheck(), NumbersToCheck2() As String
        Dim CounterNumbers As Integer = 0
        Dim CounterJoker As Integer = 0
        Dim ConvertInteger As Integer = 0

        For Each row As DataGridViewRow In DGV1.Rows
            CounterNumbers = 0
            CounterJoker = 0

            NumbersToCheck = row.Cells("Numbers").Value.ToString.Split(",")
            NumbersToCheck2 = row.Cells("Joker").Value.ToString.Split(",")

            For Each NumberStr As String In NumbersToCheck
                ConvertInteger = CType(NumberStr, Integer)
                If Array.IndexOf(NumbersWin, ConvertInteger) > -1 Then
                    CounterNumbers += 1
                End If
            Next

            For Each NumberStr2 As String In NumbersToCheck2
                ConvertInteger = CType(NumberStr2, Integer)
                If ConvertInteger = JokerWin Then
                    CounterJoker += 1
                End If
            Next

            row.Cells("Success").Value = CounterNumbers.ToString & " + " & CounterJoker.ToString


        Next
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim Date1 As Date = Date.Parse(DTP1.Value.ToString("yyyy-MM-dd"))
        Dim Date2 As Date
        If Date.TryParse(lblDate.Text, "2000-01-01") Then
            Date2 = Date.Parse(lblDate.Text)
        Else
            Date2 = "2000-01-01"
        End If


        If Date1 = Date2 Then
            CheckIfiWon()
            If chkEmail.Checked Then
                CreateMessageEmail()
                chkEmail.Checked = False
            End If
            ErrorLabel.Text = "Ο έλενγχος έγινε"
            Button4.BackColor = DefaultBackColor
            DisableAutoFeature = True
        Else
            If Not ErrorLabel.Text.StartsWith("OPAP Site Timeout") Then
                ErrorLabel.Text = "Η ημερομηνία που έχει επιλεχθεί διαφέρει απο αυτήν της κλήρωσης"
            End If
            End If

    End Sub

    
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        DisableAutoFeature = False
        RunAuto()

        If myTimer.Enabled = False Then
            Button4.BackColor = Color.Aqua
            myTimer.Interval = 6000

            myTimer.Start()


            ' Runs the timer, and raises the event. 
            While DisableAutoFeature = False

                ' Processes all the events in the queue.
                Application.DoEvents()
            End While
            Button4.BackColor = DefaultBackColor
        Else
            myTimer.Enabled = False
            DisableAutoFeature = True
            ErrorLabel.Text = "Σταμάτησε ο μετρητής απο τον χρήστη"
            Button4.BackColor = DefaultBackColor
        End If

    End Sub


    Private Shared Sub TimerEventProcessor(myObject As Object, ByVal myEventArgs As EventArgs) Handles myTimer.Tick
        'Console.WriteLine("have fun")
        myTimer.Stop()

        JokerResults.Form1.RunAuto()

        If JokerResults.Form1.DisableAutoFeature = False Then
            myTimer.Enabled = True
        Else
            myTimer.Enabled = False
        End If

    End Sub


    Private Sub RunAuto()

        If DisableAutoFeature = False Then
            ReturnWinningNumbers()
            Button3.PerformClick()
            '        Else
            ' TimerC.Stop()
        End If

    End Sub

    Private Sub CreateMessageEmail()
        My.Settings.MyEmailAddress = txtEmail.Text.ToString

        DGV1.Sort(DGV1.Columns("Success"), ComponentModel.ListSortDirection.Descending)

        Dim Message As String = " Αριθμοί" & vbTab & vbTab & vbTab & " Τζόκερ" & vbTab & vbTab & vbTab & " Επιτυχίες" & vbCrLf

        For Each row As DataGridViewRow In DGV1.Rows
            Message = Message & row.Cells("Numbers").Value & vbTab & vbTab & vbTab & row.Cells("Joker").Value & vbTab & vbTab & vbTab & row.Cells("Success").Value & vbCrLf
        Next
        'Console.WriteLine(Message)

        SendEmailResult(Message)
    End Sub








    Sub SendEmailResult(ByVal Message As String)




    End Sub

End Class
