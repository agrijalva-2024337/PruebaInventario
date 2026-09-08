using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SkiaSharp;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace SLCDM.Application.Features.Asignaciones;

internal sealed class ActaWordDatos
{
    public required string Para { get; init; }
    public required string Departamento { get; init; }
    public required string Fecha { get; init; }
    public required string TipoEquipo { get; init; }
    public required string Marca { get; init; }
    public required string Modelo { get; init; }
    public required string Serie { get; init; }
    public required string Especificaciones { get; init; }
    public required string Perifericos { get; init; }
    public required string Estado { get; init; }
    public required string Motivo { get; init; }
    public required string NombreResponsable { get; init; }
    public required string Dpi { get; init; }
    public required string NombreEntrega { get; init; }
    public required string CargoEntrega { get; init; }
    public required string NombreRecibe { get; init; }
    public required string CargoRecibe { get; init; }
    public byte[]? FirmaEntrega { get; init; }
    public byte[]? FirmaRecibe { get; init; }
}

internal static class ActaWordDocumento
{
    public static byte[] Rellenar(byte[] plantilla, ActaWordDatos datos)
    {
        using var input = new MemoryStream(plantilla);
        using var output = new MemoryStream();
        input.CopyTo(output);
        output.Position = 0;

        using (var doc = WordprocessingDocument.Open(output, true))
        {
            var body = doc.MainDocumentPart?.Document.Body
                ?? throw new InvalidOperationException("La plantilla Word no tiene cuerpo.");

            RellenarEncabezadoCarta(body, datos);
            RellenarTablaEquipo(body, datos);
            RellenarYoDpi(body, datos.NombreResponsable, datos.Dpi);
            QuitarDibujosDelCuerpo(body);
            InsertarFirmas(doc.MainDocumentPart!, body, datos);
            doc.MainDocumentPart!.Document.Save();
        }

        return output.ToArray();
    }

    private static void RellenarEncabezadoCarta(Body body, ActaWordDatos datos)
    {
        foreach (var p in body.Elements<Paragraph>())
        {
            var texto = Texto(p);
            if (texto.StartsWith("Para:", StringComparison.OrdinalIgnoreCase))
            {
                SetTexto(p, $"Para: {datos.Para}");
            }
            else if (texto.StartsWith("Departamento:", StringComparison.OrdinalIgnoreCase)
                     || texto.StartsWith("Departamen", StringComparison.OrdinalIgnoreCase))
            {
                SetTexto(p, $"Departamento: {datos.Departamento}");
            }
            else if (texto.StartsWith("Fecha:", StringComparison.OrdinalIgnoreCase))
            {
                SetTexto(p, $"Fecha: {datos.Fecha}");
            }
        }
    }

    private static void RellenarTablaEquipo(Body body, ActaWordDatos datos)
    {
        foreach (var tabla in body.Elements<Table>())
        {
            foreach (var fila in tabla.Elements<TableRow>())
            {
                var celdas = fila.Elements<TableCell>().ToList();
                if (celdas.Count < 2)
                {
                    continue;
                }

                var etiqueta = Normalizar(Texto(celdas[0]));
                var valor = etiqueta switch
                {
                    _ when etiqueta.Contains("especificacion") || etiqueta.Contains("hardware") => datos.Especificaciones,
                    _ when etiqueta.Contains("tipo") && etiqueta.Contains("equipo") => datos.TipoEquipo,
                    _ when etiqueta == "marca" || etiqueta.StartsWith("marca") => datos.Marca,
                    _ when etiqueta.Contains("modelo") => datos.Modelo,
                    _ when etiqueta.Contains("serie") => datos.Serie,
                    _ when etiqueta.Contains("perif") => datos.Perifericos,
                    _ when etiqueta.Contains("estado") => datos.Estado,
                    _ when etiqueta.Contains("motivo") => datos.Motivo,
                    _ => null
                };

                if (valor is not null)
                {
                    SetCelda(celdas[1], valor);
                }
            }
        }
    }

    private static void RellenarYoDpi(Body body, string nombre, string dpi)
    {
        var dpiTexto = string.IsNullOrWhiteSpace(dpi) ? "________________" : dpi.Trim();

        foreach (var p in body.Descendants<Paragraph>())
        {
            var texto = Texto(p);
            var tieneYo = texto.Contains("Yo:", StringComparison.OrdinalIgnoreCase)
                          || texto.StartsWith("Yo", StringComparison.OrdinalIgnoreCase);
            var tieneDpi = texto.Contains("DPI", StringComparison.OrdinalIgnoreCase);
            if (!tieneYo && !tieneDpi)
            {
                continue;
            }

            var idx = texto.IndexOf("Acepto", StringComparison.OrdinalIgnoreCase);
            var resto = idx >= 0 ? texto[idx..].Trim() : string.Empty;
            if (tieneYo && tieneDpi)
            {
                SetTexto(p, $"Yo: {nombre}     DPI: {dpiTexto}  {resto}");
            }
            else if (tieneYo)
            {
                SetTexto(p, string.IsNullOrWhiteSpace(resto) ? $"Yo: {nombre}" : $"Yo: {nombre}  {resto}");
            }
            else
            {
                SetTexto(p, string.IsNullOrWhiteSpace(resto) ? $"DPI: {dpiTexto}" : $"DPI: {dpiTexto}  {resto}");
            }
        }
    }

    private static void QuitarDibujosDelCuerpo(Body body)
    {
        foreach (var drawing in body.Descendants<Drawing>().ToList())
        {
            drawing.Remove();
        }

        foreach (var p in body.Elements<Paragraph>().ToList())
        {
            var t = Texto(p);
            if (t.Contains("F: _", StringComparison.OrdinalIgnoreCase)
                || t.Contains("QUIEN RECIBE", StringComparison.OrdinalIgnoreCase)
                || t.Contains("TÉCNICO IT", StringComparison.OrdinalIgnoreCase)
                || t.Contains("TECNICO IT", StringComparison.OrdinalIgnoreCase))
            {
                p.Remove();
            }
        }
    }

    private static void InsertarFirmas(MainDocumentPart main, Body body, ActaWordDatos datos)
    {
        var sect = body.Elements<SectionProperties>().LastOrDefault();
        var tabla = new Table(
            new TableProperties(
                new TableWidth { Width = "9000", Type = TableWidthUnitValues.Dxa },
                new TableBorders(
                    new TopBorder { Val = BorderValues.None },
                    new LeftBorder { Val = BorderValues.None },
                    new BottomBorder { Val = BorderValues.None },
                    new RightBorder { Val = BorderValues.None },
                    new InsideHorizontalBorder { Val = BorderValues.None },
                    new InsideVerticalBorder { Val = BorderValues.None })),
            new TableGrid(new GridColumn { Width = "4500" }, new GridColumn { Width = "4500" }));

        var imgEntrega = AsignacionDocumento.TieneTinta(datos.FirmaEntrega)
            ? datos.FirmaEntrega!
            : LineaFirmaPng();
        var imgRecibe = AsignacionDocumento.TieneTinta(datos.FirmaRecibe)
            ? datos.FirmaRecibe!
            : LineaFirmaPng();

        tabla.Append(
            FilaImagenes(main, imgEntrega, imgRecibe),
            FilaTexto(datos.NombreEntrega, datos.NombreRecibe, negrita: true),
            FilaTexto(datos.CargoEntrega, datos.CargoRecibe, negrita: false),
            FilaTexto("Firma de quien entrega", "Firma de quien recibe", negrita: false));

        if (sect is null)
        {
            body.Append(tabla);
        }
        else
        {
            body.InsertBefore(tabla, sect);
        }
    }

    private static TableRow FilaImagenes(MainDocumentPart main, byte[] izquierda, byte[] derecha)
    {
        return new TableRow(
            CeldaImagen(main, izquierda),
            CeldaImagen(main, derecha));
    }

    private static TableCell CeldaImagen(MainDocumentPart main, byte[] png)
    {
        var imagePart = main.AddImagePart(ImagePartType.Png);
        using (var s = new MemoryStream(png))
        {
            imagePart.FeedData(s);
        }

        var relId = main.GetIdOfPart(imagePart);
        var id = (uint)Math.Abs(relId.GetHashCode());
        const long cx = 2340000L;
        const long cy = 900000L;

        var para = new Paragraph(
            new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
            new Run(CrearDibujo(relId, id, cx, cy)));

        return new TableCell(
            new TableCellProperties(new TableCellWidth { Width = "4500", Type = TableWidthUnitValues.Dxa }),
            para);
    }

    private static TableRow FilaTexto(string izq, string der, bool negrita)
    {
        return new TableRow(CeldaTexto(izq, negrita), CeldaTexto(der, negrita));
    }

    private static TableCell CeldaTexto(string texto, bool negrita)
    {
        var runProps = new RunProperties(
            new RunFonts { Ascii = "Arial", HighAnsi = "Arial" },
            new FontSize { Val = "16" });
        if (negrita)
        {
            runProps.Append(new Bold());
        }

        return new TableCell(
            new TableCellProperties(new TableCellWidth { Width = "4500", Type = TableWidthUnitValues.Dxa }),
            new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                new Run(runProps, new Text(texto) { Space = SpaceProcessingModeValues.Preserve })));
    }

    private static Drawing CrearDibujo(string relId, uint docId, long cx, long cy)
    {
        return new Drawing(
            new DW.Inline(
                new DW.Extent { Cx = cx, Cy = cy },
                new DW.EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                new DW.DocProperties { Id = docId, Name = "Firma" },
                new DW.NonVisualGraphicFrameDrawingProperties(
                    new A.GraphicFrameLocks { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties { Id = 0U, Name = "firma.png" },
                                new PIC.NonVisualPictureDrawingProperties()),
                            new PIC.BlipFill(
                                new A.Blip { Embed = relId },
                                new A.Stretch(new A.FillRectangle())),
                            new PIC.ShapeProperties(
                                new A.Transform2D(
                                    new A.Offset { X = 0L, Y = 0L },
                                    new A.Extents { Cx = cx, Cy = cy }),
                                new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                    )
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }))
            {
                DistanceFromTop = 0U,
                DistanceFromBottom = 0U,
                DistanceFromLeft = 0U,
                DistanceFromRight = 0U
            });
    }

    private static byte[] LineaFirmaPng()
    {
        var info = new SKImageInfo(480, 120);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            StrokeWidth = 2,
            IsStroke = true,
            IsAntialias = true
        };
        canvas.DrawLine(16, 88, 464, 88, paint);
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        return data.ToArray();
    }

    private static void SetCelda(TableCell cell, string texto)
    {
        var para = cell.Elements<Paragraph>().FirstOrDefault() ?? cell.AppendChild(new Paragraph());
        para.ParagraphProperties?.NumberingProperties?.Remove();
        foreach (var extra in cell.Elements<Paragraph>().Skip(1).ToList())
        {
            extra.Remove();
        }

        SetTexto(para, texto);
    }

    private static void SetTexto(Paragraph p, string texto)
    {
        var props = p.ParagraphProperties?.CloneNode(true) as ParagraphProperties;
        p.RemoveAllChildren<Run>();
        if (props is not null)
        {
            p.ParagraphProperties?.Remove();
            p.PrependChild(props);
        }

        p.AppendChild(new Run(
            new RunProperties(
                new RunFonts { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" },
                new FontSize { Val = "22" }),
            new Text(texto) { Space = SpaceProcessingModeValues.Preserve }));
    }

    private static string Texto(OpenXmlElement element) =>
        string.Concat(element.Descendants<Text>().Select(t => t.Text)).Trim();

    private static string Normalizar(string value)
    {
        var sinAcento = value
            .Replace("é", "e", StringComparison.OrdinalIgnoreCase)
            .Replace("í", "i", StringComparison.OrdinalIgnoreCase)
            .Replace("ó", "o", StringComparison.OrdinalIgnoreCase)
            .Replace("á", "a", StringComparison.OrdinalIgnoreCase)
            .Replace("ú", "u", StringComparison.OrdinalIgnoreCase);
        return sinAcento.ToLowerInvariant();
    }
}
