using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCalculator.Models
{
    public class CalculatorMaintenanceResponse
    {
        public FileUploadResponse CameraResponse { get; set; }
        public FileUploadResponse FramRateResponse { get; set; }
    }
}