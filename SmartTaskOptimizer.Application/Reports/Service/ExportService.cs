using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using SmartTaskOptimizer.Shared.DTOs.Reports;
using System.Text;

namespace SmartTaskOptimizer.Application.Reports.Service;

public sealed class ExportService : IExportService
{
    public ExportService(IConfiguration configuration)
    {
        var license = configuration["EPPlus:License"] ?? Environment.GetEnvironmentVariable("EPPlusLicense");
        if (string.IsNullOrWhiteSpace(license)) throw new InvalidOperationException("EPPlus:License or EPPlusLicense must be configured before using Excel export.");
        if (license.StartsWith("Commercial:", StringComparison.OrdinalIgnoreCase)) ExcelPackage.License.SetCommercial(license["Commercial:".Length..]);
        else if (license.StartsWith("NonCommercialOrganization:", StringComparison.OrdinalIgnoreCase)) ExcelPackage.License.SetNonCommercialOrganization(license["NonCommercialOrganization:".Length..]);
        else if (license.StartsWith("NonCommercialPersonal:", StringComparison.OrdinalIgnoreCase)) ExcelPackage.License.SetNonCommercialPersonal(license["NonCommercialPersonal:".Length..]);
        else throw new InvalidOperationException("EPPlus:License must use Commercial:, NonCommercialOrganization:, or NonCommercialPersonal: prefix.");
    }

    public byte[] ExportExcel(List<TaskReportDto> data)
    {
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Tasks");
        sheet.Cells[1, 1].Value = "Title"; sheet.Cells[1, 2].Value = "Status"; sheet.Cells[1, 3].Value = "Priority"; sheet.Cells[1, 4].Value = "Deadline";
        for (var i = 0; i < data.Count; i++)
        {
            sheet.Cells[i + 2, 1].Value = data[i].Title;
            sheet.Cells[i + 2, 2].Value = data[i].Status.ToString();
            sheet.Cells[i + 2, 3].Value = data[i].Priority.ToString();
            sheet.Cells[i + 2, 4].Value = data[i].Deadline.ToString("u");
        }
        if (sheet.Dimension is not null) sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] ExportPdf(List<TaskReportDto> data)
    {
        var rows = new List<string> { "SmartTaskOptimizer - Task Report", "", "Title | Status | Priority | Deadline" };
        rows.AddRange(data.Select(x => $"{x.Title} | {x.Status} | {x.Priority} | {x.Deadline:u}"));
        return CreateSimplePdf(rows);
    }

    private static byte[] CreateSimplePdf(IReadOnlyList<string> lines)
    {
        var content = new StringBuilder("BT\n/F1 10 Tf\n50 800 Td\n");
        foreach (var line in lines.Take(55)) content.Append('(').Append(Escape(line)).Append(") Tj\n0 -14 Td\n");
        content.Append("ET");
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(content.ToString())} >>\nstream\n{content}\nendstream"
        };
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.ASCII, 1024, true);
        writer.WriteLine("%PDF-1.4"); writer.Flush();
        var offsets = new List<long> { 0 };
        foreach (var (obj, index) in objects.Select((x, i) => (x, i + 1))) { offsets.Add(ms.Position); writer.WriteLine($"{index} 0 obj"); writer.WriteLine(obj); writer.WriteLine("endobj"); writer.Flush(); }
        var xref = ms.Position; writer.WriteLine($"xref\n0 {objects.Length + 1}\n0000000000 65535 f ");
        for (var i = 1; i < offsets.Count; i++) writer.WriteLine($"{offsets[i]:D10} 00000 n ");
        writer.WriteLine($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF"); writer.Flush();
        return ms.ToArray();
    }

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}
