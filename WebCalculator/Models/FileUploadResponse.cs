using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCalculator.Models
{
    public class FileUploadResponse
    {
        public bool IsValid { get; set; }
        public long SuccessRows { get; set; }
        public string Message { get; set; }
        public List<ExcelColResponse> ListExcelColResponses { get; set; }
        public string ErrorResponsePath { get; set; }
    }
    public class ExcelColResponse
    {
        public int RowNumber { get; set; }
        public string ColumnName { get; set; }
        public string Message { get; set; }
    }
}