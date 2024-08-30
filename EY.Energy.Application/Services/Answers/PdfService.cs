using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.Extensions.Logging;


namespace EY.Energy.Application.Services.Answers
{
    public class PdfService
    {
        private readonly IConverter _converter;
        private readonly ILogger<PdfService> _logger;

        public PdfService(IConverter converter, ILogger<PdfService> logger)
        {
            _converter = converter;
            _logger = logger;
        }

        public byte[] CreatePdf(string htmlContent)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
            },
                Objects = {
                new ObjectSettings() {
                    PagesCount = true,
                    HtmlContent = htmlContent,
                    WebSettings = { DefaultEncoding = "utf-8" },
                    HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                    FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "EY Energy" }
                }
            }
            };

            return _converter.Convert(doc);
        }
    }

}