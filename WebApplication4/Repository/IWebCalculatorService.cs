using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication4.Models;

namespace WebApplication4.Repository
{
    public interface IWebCalculatorService
    {
        bool DeleteAllOldCamera();

        bool SaveBulkUploadCamara(DataTable dtCamara);

        long CamaraRowCount();

        List<CameraStatus> UpdateCamera(string CID);

        List<Camara> GetCamaraList();

        int DeleteFrameRateTable();

        bool SaveBulkUploadFrameRate(DataTable dtFrameRate);

        long GetFrameRateRowCount();
    }
}
