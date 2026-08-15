using SmartTaskOptimizer.Shared.DTOs.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.Reports.Service
{
    public interface IExportService
    {
        byte[] ExportExcel(List<TaskReportDto> data);
        byte[] ExportPdf(List<TaskReportDto> data);
    }
}
