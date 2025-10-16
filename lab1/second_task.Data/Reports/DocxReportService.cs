using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using second_task.Data.Interfaces;

namespace second_task.Data.Reports
{
    public class DocxReportService<T> : IReportGenerator<T>
    {
        public void Generate(string filePath, IEnumerable<T> data)
        {
            using var document = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Title
            var titleParagraph = body.AppendChild(new Paragraph());
            var titleRun = titleParagraph.AppendChild(new Run());
            titleRun.AppendChild(new Text("Data Report"));
            
            var titleRunProperties = titleRun.PrependChild(new RunProperties());
            titleRunProperties.AppendChild(new Bold());
            titleRunProperties.AppendChild(new FontSize() { Val = "24" });

            // Summary
            var summaryParagraph = body.AppendChild(new Paragraph());
            var summaryRun = summaryParagraph.AppendChild(new Run());
            var dataList = data.ToList();
            summaryRun.AppendChild(new Text($"Total records: {dataList.Count}"));

            // Data type info
            var typeParagraph = body.AppendChild(new Paragraph());
            var typeRun = typeParagraph.AppendChild(new Run());
            typeRun.AppendChild(new Text($"Data type: {typeof(T).Name}"));

            // Generated timestamp
            var timestampParagraph = body.AppendChild(new Paragraph());
            var timestampRun = timestampParagraph.AppendChild(new Run());
            timestampRun.AppendChild(new Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"));

            document.Save();
        }
    }
}
