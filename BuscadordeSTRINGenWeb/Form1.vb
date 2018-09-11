Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Windows.Forms.DataVisualization.Charting
Imports System.Windows.Forms

Imports GMap.NET
Imports GMap.NET.MapProviders
Imports GMap.NET.WindowsForms
Imports GMap.NET.WindowsForms.Markers

Imports Newtonsoft.Json
Imports System.ComponentModel

Public Class Form1
#Region "Variables"

    Public Class Rootobject

        Public Property Message As String
        Public Property Iss_position As Iss_Position
        Public Property Timestamp As Integer
    End Class

    Public Class Iss_Position
        Public Property Latitude As String
        Public Property Longitude As String
    End Class
    Dim marker As GMarkerGoogle
    Dim markerOverlay As GMapOverlay
    Dim dt As DataTable
    Dim LatInicial As Double = -31.5555087
    Dim LongInicial As Double = -63.5364147
    Dim ubica As New Rootobject


    Dim serieCirculo1 As New Series("ISS_Position") With {
        .ChartType = SeriesChartType.Spline}


#End Region

    Public Function Buscardatos(url As String) As String
        Dim source As String = ""

        Dim request As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
        Dim response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)

        If response.StatusCode = HttpStatusCode.OK Then
            Dim receiveStream As Stream = response.GetResponseStream()
            Dim readStream As StreamReader = Nothing

            readStream = New StreamReader(receiveStream)

            source = readStream.ReadToEnd()

            response.Close()
            readStream.Close()
        Else
            source = response.StatusDescription
        End If
        Return source
    End Function
    Public Sub DeserealizarJSON()

        Dim json As String = txtCodigo.Text
        Newtonsoft.Json.JsonConvert.PopulateObject(json, ubica)

        ListBox1.Items.Add("Latitud: " + Convert.ToString(ubica.Iss_position.Latitude))
        ListBox1.Items.Add("Longitud: " + Convert.ToString(ubica.Iss_position.Longitude))
        ListBox1.Items.Add("Hora: " + CStr(TimeOfDay))
        ListBox1.Items.Add("***************")
    End Sub
    Public Sub MarcarRuta()
        Dim ruta As GMapOverlay = New GMapOverlay("CapaRuta")
        Dim puntos As List(Of PointLatLng) = New List(Of PointLatLng)
        'markerOverlay = New GMapOverlay("Marcador")
        markerOverlay = GMapControl1.Overlays.First(Function(d) d.Id = "Marcador")

        marker = CType(markerOverlay.Markers.First, GMarkerGoogle)
        marker.Position = New PointLatLng(serieCirculo1.Points.Last.YValues(0), serieCirculo1.Points.Last.XValue)

        'markerOverlay.Markers.Add(marker)
        marker.ToolTipMode = MarkerTooltipMode.Always
        marker.ToolTipText = String.Format("Ubicacion: Latitud:{0} Longitud: {1}",
                                           serieCirculo1.Points.Last.YValues(0), serieCirculo1.Points.Last.XValue)
        GMapControl1.Overlays.Add(markerOverlay)



        For Each dato In serieCirculo1.Points
            puntos.Add(New PointLatLng(dato.YValues(0), dato.XValue))
        Next

        Dim PuntosRuta As GMapRoute = New GMapRoute(puntos, "Ruta")

        ruta.Routes.Add(PuntosRuta)
        GMapControl1.Overlays.Add(ruta)
        GMapControl1.Refresh()
        GMapControl1.ZoomAndCenterMarkers("Marcador")
        GMapControl1.Zoom = 3
    End Sub
    Public Sub DatosMapaenChart()
        Chart1.Series.Clear()

        serieCirculo1.Points.AddXY(Convert.ToDouble(ubica.Iss_position.Longitude.Replace(".", ",")),
                                 Convert.ToDouble(ubica.Iss_position.Latitude.Replace(".", ",")))

        Chart1.Series.Add(serieCirculo1)
        Chart1.ChartAreas(0).AxisY.Minimum = -90
        Chart1.ChartAreas(0).AxisY.Maximum = 90
        Chart1.ChartAreas(0).AxisX.Minimum = -180
        Chart1.ChartAreas(0).AxisX.Maximum = 180

    End Sub
    Public Sub CargarGoogleMap()
        GMapControl1.DragButton = MouseButtons.Left
        GMapControl1.CanDragMap = True
        GMapControl1.MapProvider = GMapProviders.GoogleMap
        GMapControl1.Position = New PointLatLng(LatInicial, LongInicial)
        GMapControl1.MinZoom = 0
        GMapControl1.MaxZoom = 24
        GMapControl1.Zoom = 2
        GMapControl1.AutoScroll = True

        markerOverlay = New GMapOverlay("Marcador")
        marker = New GMarkerGoogle(New PointLatLng(LatInicial, LongInicial), GMarkerGoogleType.green_dot)
        markerOverlay.Markers.Add(marker)
        marker.ToolTipMode = MarkerTooltipMode.Always
        marker.ToolTipText = String.Format("Ubicacion: Latitud:{0} Longitud: {1}", LatInicial, LongInicial)

        GMapControl1.Overlays.Add(markerOverlay)
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LblURLWiki.Text = "https://es.wikipedia.org/w/api.php?action=parse&page=Estaci%C3%B3n_Espacial_Internacional&section=0&prop=text&format=json"
        LblURL.Text = "http://api.open-notify.org/iss-now.json"
        WebBrowser1.AllowNavigation = False

        ' {"message": "success", "iss_position": {"latitude": "-35.5159", "longitude": "-27.0538"}, "timestamp": 1535083239}


        txtCodigo.Text = Buscardatos(LblURL.Text)

        DeserealizarJSON()
        DatosMapaenChart()
        CargarGoogleMap()

    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick

        txtCodigo.Text = Buscardatos(LblURL.Text)

        DeserealizarJSON()
        DatosMapaenChart()
        MarcarRuta()

        Label2.Text = "Cantidad de puntos: " + serieCirculo1.Points.Count.ToString

    End Sub


    Private Sub WebBrowser1_NewWindow(sender As Object, e As CancelEventArgs) Handles WebBrowser1.NewWindow
        e.Cancel = True



    End Sub
#Region "Practica"
    'Public Sub DatosTelefoniaChart()
    '    Chart1.Series.Clear()

    '    Dim serieCirculo As New Series("Mercado de Telefonía Celular")
    '    serieCirculo.ChartType = SeriesChartType.Radar

    '    serieCirculo.Points.AddXY("Claro", 5350)
    '    serieCirculo.Points.AddXY("Movistar", 3425)
    '    serieCirculo.Points.AddXY("Personal", 2450)
    '    serieCirculo.Points.AddXY("lalala", 535)
    '    serieCirculo.Points.AddXY("Otros", 535)
    '    serieCirculo.Label = "#AXISLABEL #PERCENT"
    '    serieCirculo.LegendText = "#AXISLABEL"

    '    Chart1.Series.Add(serieCirculo)
    'End Sub


    'Public Sub BuscarenelString()
    '    TextBoxDATOS.Clear() 'LIMPIA ANTES DE BUSCAR DATOS DE NUEVO
    '    Try
    '        Dim MIARRAY As New ArrayList 'PARA PODER MANIPULAR LOS DATOS(ORDENAR, DESVIAR, ETC)
    '        For Each ETIQUETA As HtmlElement In WebBrowser1.Document.All 'LEE TODO EL HTML ETIQUETA A ETIQUETA
    '            If ETIQUETA.TagName = "td" And ETIQUETA.GetAttribute("id").Contains("DisplayWorkingSatLongitude") Then 'ETIQUETA ARTICULO
    '                If ETIQUETA.OuterHtml.Contains("width=") Then
    '                    MIARRAY.Add(ETIQUETA.InnerText) 'LA PONE EN EL ARRAY
    '                End If
    '            ElseIf ETIQUETA.TagName = "SPAN" And ETIQUETA.GetAttribute("id").Contains("price_index_page_") Then 'ETIQUETA PRECIO
    '                MIARRAY.Add(ETIQUETA.InnerText) 'LA PONE EN EL ARRAY
    '            ElseIf ETIQUETA.TagName = "SPAN" And ETIQUETA.GetAttribute("id").Contains("counter_index_page_") Then 'ETIQUETA TIEMPO
    '                If ETIQUETA.OuterHtml.Contains("Finalizada") = False Then
    '                    MIARRAY.Add(ETIQUETA.InnerText) 'LA PONE EN EL ARRAY
    '                Else
    '                    Exit For ' SI HA LLEGADO A UNA ETIQUETA DE FINALIZADA PARA EL BUCLE
    '                End If
    '            End If
    '        Next
    '        For I = 0 To MIARRAY.Count - 3 'PRESENTA LOS DATOS DE LAS SUBASTAS VIVAS EN EL TEXTBOX ELIMINANDO LOS DATOS QUE CORRESPONDEN A LA FINALIZADA 
    '            If MIARRAY(I + 2).ToString.Contains("Finalizada") = False Then
    '                TextBoxDATOS.Text += MIARRAY(I).ToString & vbCrLf
    '            End If
    '        Next
    '    Catch ex As Exception
    '        MsgBox(ex.Message)
    '    End Try
    'End Sub
    'If array.Length <> Nothing Then

    '    For Each dato In array
    '        Dim posicion As Integer = dato.IndexOf("latitude")

    '        ' MsgBox(dato.Substring(posicion, 8))

    '    Next
    'End If

    ''Dim asd As String = txtCodigo.Text
    ''Dim starIndex As Integer = asd.IndexOf("latitude" + "'" + ":")
    ''Dim endIndex As Integer = asd.IndexOf("'" + "long")
    ''latitude = asd.Substring(starIndex + 1, asd.Length - endIndex + 1)
    ''Dim numero As String() = latitude.Split(":"c)
    ''Dim lat As String = "-" + numero(0)
    ''MsgBox(lat)


    '    Int starIndex = asd.IndexOf("latitude" + '"' + ":");
    '    Int() endIndex = asd.IndexOf('"' + "long");
    'String nr2 = asd.Substring(starIndex, asd.Length - endIndex);
    '    String[] numero = nr2.Split('-');
    '    String link = "-" + numero[1];           
    '    MessageBox.Show(link);

    'Private Sub GMapControl1_DoubleClick(sender As Object, e As EventArgs) Handles GMapControl1.DoubleClick
    '    Dim lat As Double = GMapControl1.FromLocalToLatLng(e.X, e.Y).Lat
    '    Dim lng As Double = GMapControl1.FromLocalToLatLng(e.X, e.Y).Lng


    '    'Creamos el marcador para moverlo al lugar indicado 
    '    '  marker.Position = New PointLatLng(lat, lng)
    '    'Tambien se agrega el mensaje al marcador (tooltip) 
    '    ' marker.ToolTipText = String.Format("Ubicacion: \n Latitud: {0} \n Longitud: {1}", lat, lng)
    'End Sub

    'Private Sub GMapControl1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles GMapControl1.MouseDoubleClick
    '    Dim lat As Double = GMapControl1.FromLocalToLatLng(e.X, e.Y).Lat
    '    Dim lng As Double = GMapControl1.FromLocalToLatLng(e.X, e.Y).Lng


    '    ' Creamos el marcador para moverlo al lugar indicado 
    '    marker.Position = New PointLatLng(lat, lng)
    '    '  Tambien se agrega el mensaje al marcador (tooltip) 
    '    marker.ToolTipText = String.Format("Ubicacion: \n Latitud: {0} \n Longitud: {1}", lat, lng)
    'End Sub

    'markerOverlay = New GMapOverlay("Marcador")
    'marker = New GMarkerGoogle(New PointLatLng(LatInicial, LongInicial), GMarkerGoogleType.green_dot)
    'markerOverlay.Markers.Add(marker)
    'marker.ToolTipMode = MarkerTooltipMode.Always
    'marker.ToolTipText = String.Format("Ubicacion: Latitud:{0} Longitud: {1}", LatInicial, LongInicial)

    'GMapControl1.Overlays.Add(markerOverlay)

#End Region
End Class
