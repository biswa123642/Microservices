using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication4.Models
{
        public class Camara
        {
            public string CID { get; set; }
            public string CamaraModel { get; set; }
            public string CamaraType { get; set; }
            public string SenserMode { get; set; }
            public string IsActive { get; set; }
            public string MemoryType { get; set; }
            public string MemorySize { get; set; }
            public string FastOption { get; set; }
            public string BitDepth { get; set; }
            public string ShutterMode { get; set; }
            public string CXPBank { get; set; }
            public string PixelSize { get; set; }
            public string CreatedDate { get; set; }
            public string IsDeleted { get; set; }
            public List<Camara> camaraList { get; set; }
        }

        public class CameraStatus
        {
            public bool IsDeleted { get; set; }
            public bool IsUpdated { get; set; }
            public string ErrorMessage { get; set; }
            public string SuccessMessage { get; set; }
            public List<CameraStatus> cameraStatusList { get; set; }
        }
    }