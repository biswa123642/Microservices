using ExcelDataReader;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication4.Models;
using WebApplication4.Repository;

namespace WebApplication4.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class WebCalculatorApiController : ApiController
    {
        private static ILog Log = LogManager.GetLogger(typeof(WebCalculatorApiController));
        private readonly int CameraExcelColumnCount = 12;
        private readonly int FramerateExcelColumnCount = 15;

        [Route("api/webcalculator")]
        [HttpGet]
        public string Get()
        {
            return "Hello";
        }
        
        [Route("api/webcalculator/uploadCamera")]
        [HttpPost]
        public HttpResponseMessage PostCamera()
        {


            HttpResponseMessage result = null;
            var httpRequest = System.Web.HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var camaraFile = httpRequest.Files["camera-file"];
                FileUploadResponse camaraFileProcessed = new FileUploadResponse();

                bool colMatch = false;
                IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(camaraFile.InputStream);
                //check invalid columns
                if (camaraFile.ContentLength > 0)
                {
                    int fieldcount = reader.FieldCount;
                    if (fieldcount == CameraExcelColumnCount)
                    {
                        List<string> colName = new List<string>(new string[]
                        { "CameraModel",
                            "Camera Type",
                            "Inactive",
                            "Sensor Mode",
                            "Memory Type",
                            "Actual Memory Size (GB)",
                            "Memory Size User Selection",
                            "Fast Option",
                            "Bit Depth",
                            "Shutter Mode",
                            "CXP Bank",
                            "Pixel Size (µm)",
                        });

                        DataTable dt_ = reader.AsDataSet().Tables[0];

                        for (int i = 0; i < colName.Count; i++)
                        {
                            if (colName[i].ToString().Trim() == dt_.Rows[0][i].ToString().Trim())
                            {
                                colMatch = true;
                            }
                            else
                            {
                                colMatch = false;
                                break;
                            }
                        }
                        if (colMatch)
                        {
                            camaraFileProcessed = SaveCamaraData(ProcessedFile(reader), camaraFile);
                        }
                        else
                        {
                            //return new System.Web.Mvc.JsonResult() { Data = new { mesasge = "error 1" }, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet };

                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Camera file has invalid column name(s)");
                        }
                    }
                    else if (fieldcount > CameraExcelColumnCount)
                    {
                        //return new System.Web.Mvc.JsonResult() { Data = new { mesasge = "error 1" }, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet };

                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Camera file has too many column(s)");
                    }
                    else
                    {
                        // return new System.Web.Mvc.JsonResult() { Data = new { mesasge = "error 1" }, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet };

                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Camera file does not have required format");
                    }
                }

                var CalculatorMaintenanceResponse = new CalculatorMaintenanceResponse
                {
                    CameraResponse = camaraFileProcessed,
                };

                var calculatorMaintenanceResponseContent = new StringContent(JsonConvert.SerializeObject(CalculatorMaintenanceResponse), System.Text.Encoding.UTF8, "application/json");

                if (camaraFileProcessed.IsValid)
                {
                    //return new System.Web.Mvc.JsonResult() { Data = new { mesasge = "error 1" }, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet };

                    return new HttpResponseMessage()
                    {
                        Content = calculatorMaintenanceResponseContent
                    };
                }
                else if (camaraFileProcessed.IsValid)
                {
                    // return new System.Web.Mvc.JsonResult() { Data = new { mesasge = "error 1" }, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet };

                    return new HttpResponseMessage()
                    {
                        Content = calculatorMaintenanceResponseContent
                    };
                }
                else
                {
                    //return new System.Web.Mvc.JsonResult() { Data = new { mesasge = "error 1" }, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet };

                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, camaraFileProcessed.Message);
                }
            }
            else
            {
                // return new System.Web.Mvc.JsonResult() { Data = new { mesasge = "error 1" }, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet };

                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            // return new System.Web.Mvc.JsonResult() { Data = new { mesasge = "error 1" }, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet };

            return result;
        }

        [Route("api/webcalculator/UpdateCamera")]
        [HttpPost]
        public HttpResponseMessage UpdateCamera(string CID)
        {
            List<CameraStatus> lstCameras = new List<CameraStatus>();
            try
            {
                if (CID != null)
                {
                    WebCalculatorService dbServiceObj = new WebCalculatorService();
                    lstCameras = dbServiceObj.UpdateCamera(CID);
                }
                else
                {
                    return new HttpResponseMessage
                    {
                        Content = new StringContent("UpdateCamera operation failed")
                    };
                }

            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent(ex.Message)
                };
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(Newtonsoft.Json.Linq.JArray.FromObject(lstCameras).ToString(), System.Text.Encoding.UTF8, "application/json")
            };
        }

        [Route("api/webcalculator/GetCameraList")]
        [HttpGet]
        public HttpResponseMessage GetCameraList()
        {
            List<Camara> lstCameras = new List<Camara>();
            try
            {
                WebCalculatorService dbServiceObj = new WebCalculatorService();
                lstCameras = dbServiceObj.GetCamaraList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return new HttpResponseMessage
                {
                    Content = new StringContent(ex.Message)
                };
            }
            return new HttpResponseMessage()
            {

                Content = new StringContent(Newtonsoft.Json.Linq.JArray.FromObject(lstCameras).ToString(), System.Text.Encoding.UTF8, "application/json")
            };
        }

        [Route("api/webcalculator/uploadframerate")]
        [HttpPost]
        public HttpResponseMessage PostFrameRate()
        {
            HttpResponseMessage result = null;
            var httpRequest = System.Web.HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var frameRateFile = httpRequest.Files["frame-rate-file"];
                FileUploadResponse frameRateFileProcessed = new FileUploadResponse();
                bool colMatch = false;
                if (frameRateFile.ContentLength > 0)
                {
                    IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(frameRateFile.InputStream);
                    int fieldcount = reader.FieldCount;
                    if (fieldcount == FramerateExcelColumnCount)
                    {
                        List<string> colName = new List<string>(new string[]
                        { "CameraModel",
                           "FW Version",
                           "Camera Type",
                           //"Inactive",
                           "Sensor Mode",
                           "Memory Type",
                           "Actual Memory Size (GB)",
                           //"Memory Size User Selection",
                           "Fast Option",
                           "Bit Depth",
                           "Shutter Mode",
                           "CXP Banks",
                           "X res",
                           "Y res",
                           "Rate (fps)",
                           "Gpx/s",
                           "Frame Ct"
                        });
                        DataTable dt_ = reader.AsDataSet().Tables[0];

                        for (int i = 0; i < colName.Count; i++)
                        {
                            if (colName[i].ToString().Trim() == dt_.Rows[0][i].ToString().Trim())
                            {
                                colMatch = true;
                            }
                            else
                            {
                                colMatch = false;
                                break;
                            }
                        }
                        if (colMatch)
                        {
                            frameRateFileProcessed = SaveFrameRateData(ProcessedFile(reader), frameRateFile);
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Frame Rate file has invalid column name(s)");
                        }
                    }
                    else if (fieldcount > FramerateExcelColumnCount)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Frame Rate file has too many column(s)");
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Frame Rate file does not have required format");
                    }
                }

                var CalculatorMaintenanceResponse = new CalculatorMaintenanceResponse
                {
                    FramRateResponse = frameRateFileProcessed
                };

                var calculatorMaintenanceResponseContent = new StringContent(JsonConvert.SerializeObject(CalculatorMaintenanceResponse), System.Text.Encoding.UTF8, "application/json");

                if (frameRateFileProcessed.IsValid)
                {
                    return new HttpResponseMessage()
                    {
                        Content = calculatorMaintenanceResponseContent
                    };
                }
                else if (frameRateFileProcessed.IsValid)
                {
                    return new HttpResponseMessage()
                    {
                        Content = calculatorMaintenanceResponseContent
                    };
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Framerate File Not found");
                }
            }
            else
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return result;
        }

        [Route("api/webcalculator/downloadErrorfile")]
        [HttpGet]
        public HttpResponseMessage DownloadErrorLog(string fullPath)
        {
            var dataBytes = File.ReadAllBytes(fullPath);
            var dataStream = new MemoryStream(dataBytes);

            HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new StreamContent(dataStream);
            httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            httpResponseMessage.Content.Headers.ContentDisposition.FileName = Path.GetFileName(fullPath);
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            return httpResponseMessage;
        }


        public FileUploadResponse SaveFrameRateData(DataTable dataTable, System.Web.HttpPostedFile file)
        {
            if (dataTable?.Rows.Count == 0)
            {
                return new FileUploadResponse { IsValid = false };
            }
            long addedRows = 0;
            string ColumnBlank = string.Empty;
            string rowNumberCount = string.Empty;
            string responsePath = string.Empty;
            bool warning = false;
            List<ExcelColResponse> excelColResponseList = new List<ExcelColResponse>();
            ExcelColResponse excelColResponse = null;
            foreach (DataRow row in dataTable.Rows)
            {
                int count = 0;
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    if (String.IsNullOrEmpty(row.ItemArray[i].ToString().Trim()))
                    {
                        excelColResponse = new ExcelColResponse
                        {
                            RowNumber = dataTable.Rows.IndexOf(row) + 2,
                            ColumnName = dataTable.Columns[i].ColumnName.ToString(),
                            Message = "Data is empty"
                        };
                        excelColResponseList.Add(excelColResponse);
                        count++;

                        if (count == FramerateExcelColumnCount)
                        {
                            //if (excelColResponseList != null && excelColResponseList.Count > 0)
                            //{
                            //    responsePath = WriteErrorLogs(excelColResponseList, "FrameRateLog");
                            //}
                            return new FileUploadResponse { IsValid = true, SuccessRows = addedRows, ErrorResponsePath = responsePath, ListExcelColResponses = excelColResponseList, Message = dataTable.Rows.IndexOf(row) + 1 + " Row number are blank! Please check your excel file" };
                        }
                    }
                }

                if (!string.IsNullOrEmpty(row["X res"].ToString()))
                {
                    if (!int.TryParse(row["X res"].ToString(), out int notinactivate))
                    {
                        excelColResponse = new ExcelColResponse
                        {
                            RowNumber = dataTable.Rows.IndexOf(row) + 2,
                            ColumnName = "X res",
                            Message = "X res must contain numeric value."
                        };
                        excelColResponseList.Add(excelColResponse);
                        warning = true;
                    }
                }
                if (!string.IsNullOrEmpty(row["Y res"].ToString()))
                {
                    if (!int.TryParse(row["Y res"].ToString(), out int notinactivate))
                    {
                        excelColResponse = new ExcelColResponse
                        {
                            RowNumber = dataTable.Rows.IndexOf(row) + 2,
                            ColumnName = "Y res",
                            Message = "Y res must contain numeric value."
                        };
                        excelColResponseList.Add(excelColResponse);
                        warning = true;
                    }
                }
                if (!string.IsNullOrEmpty(row["Rate (fps)"].ToString()))
                {
                    if (!decimal.TryParse(row["Rate (fps)"].ToString(), out decimal notinactivate))
                    {
                        excelColResponse = new ExcelColResponse
                        {
                            RowNumber = dataTable.Rows.IndexOf(row) + 2,
                            ColumnName = "Rate (fps)",
                            Message = "Rate (fps) must contain numeric value."
                        };
                        excelColResponseList.Add(excelColResponse);
                        warning = true;
                    }
                }
                if (!string.IsNullOrEmpty(row["Gpx/s"].ToString()))
                {
                    if (!decimal.TryParse(row["Gpx/s"].ToString(), out decimal notinactivate))
                    {
                        excelColResponse = new ExcelColResponse
                        {
                            RowNumber = dataTable.Rows.IndexOf(row) + 2,
                            ColumnName = "Gpx/s",
                            Message = "Gpx/s must contain numeric value."
                        };
                        excelColResponseList.Add(excelColResponse);
                        warning = true;
                    }
                }
                if (!string.IsNullOrEmpty(row["Frame Ct"].ToString()))
                {
                    if (!decimal.TryParse(row["Frame Ct"].ToString(), out decimal notdecimalvalue))
                    {
                        excelColResponse = new ExcelColResponse
                        {
                            RowNumber = dataTable.Rows.IndexOf(row) + 2,
                            ColumnName = "Frame Ct",
                            Message = "Frame Ct must contain numeric value."
                        };
                        excelColResponseList.Add(excelColResponse);
                        warning = true;
                    }
                }
            }
            if (excelColResponseList != null && excelColResponseList.Count > 0 && warning)
            {
                //if (excelColResponseList != null && excelColResponseList.Count > 0)
                //{
                //    responsePath = WriteErrorLogs(excelColResponseList, "FrameRateLog");
                //}
                return new FileUploadResponse { IsValid = true, SuccessRows = addedRows, ErrorResponsePath = responsePath, ListExcelColResponses = excelColResponseList, Message = "Unsuccessful Upload! Please check your excel file" };
            }

            WebCalculatorService dbManagerObj = new WebCalculatorService();
            dbManagerObj.DeleteFrameRateTable();
            var status = dbManagerObj.SaveBulkUploadFrameRate(dataTable);
            addedRows = dbManagerObj.GetFrameRateRowCount();

            if (status)
            {
                //if (excelColResponseList != null && excelColResponseList.Count > 0)
                //{
                //    responsePath = WriteErrorLogs(excelColResponseList, "FrameRateLog", "");
                //}
                return new FileUploadResponse { IsValid = true, SuccessRows = addedRows, ErrorResponsePath = responsePath, ListExcelColResponses = excelColResponseList, Message = "File has been successfully uploaded" };
            }
            else
            {
                return new FileUploadResponse { IsValid = false, SuccessRows = addedRows, Message = "Error in uploading FramRate Excel file" };
            }
        }

        public DataTable ProcessedFile(IExcelDataReader reader)
        {
            if (reader.FieldCount == 0)
            {
                return null;
            }
            //IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(file.InputStream);
            int fieldcount = reader.FieldCount;
            int rowcount = reader.RowCount;
            DataTable dt_ = new DataTable();
            try
            {
                dt_ = reader.AsDataSet().Tables[0];

                foreach (DataColumn column in dt_.Columns)
                {
                    dt_.Columns[column.ColumnName].ColumnName = dt_.Rows[0][column].ToString();
                }

                dt_.Rows[0].Delete();
                dt_.AcceptChanges();

                return dt_;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //private static string WriteErrorLogs(List<ExcelColResponse> excelColResponseList = null, string FileName = null, string ErrorMessage = null)
        //{
        //    string filePath = System.Web.HttpContext.Current.ApplicationInstance.Server.MapPath("~/WebCalculator_uploads");
        //    var datetime = GetTimestamp(DateTime.Now);
        //    string fileName = FileName + "-" + datetime + ".csv";
        //    string fullpath = System.IO.Path.Combine(filePath, fileName);
        //    using (StreamWriter writer = new StreamWriter(fullpath, true))
        //    {
        //        //writer.WriteLine("-----------------------------------------------------------------------------");
        //        writer.WriteLine(string.Format("Generated Date :{0} ", DateTime.Now.ToString()));
        //        writer.WriteLine(string.Format("Error Count :{0} ", excelColResponseList.Count));
        //        writer.WriteLine();
        //        string rowTitle = string.Format("Row Number, Column Name, Message");
        //        writer.WriteLine(rowTitle);
        //        if (excelColResponseList != null)
        //        {
        //            foreach (var item in excelColResponseList)
        //            {
        //                string errorRow = string.Format("{0},{1},{2} ", item.RowNumber, item.ColumnName, item.Message);
        //                writer.WriteLine(errorRow);
        //            }
        //        }
        //        else
        //        {
        //            writer.WriteLine("Message : " + ErrorMessage);

        //        }

        //        return fullpath;
        //    }
        //}

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmss");
        }

        public FileUploadResponse SaveCamaraData(DataTable dataTable, System.Web.HttpPostedFile file)
        {
            if (dataTable?.Rows.Count == 0)
            {
                return new FileUploadResponse { IsValid = false };
            }
            long addedRows = 0;
            string ColumnBlank = string.Empty;
            string rowNumberCount = string.Empty;
            string responsePath = string.Empty;
            List<ExcelColResponse> excelColResponseList = new List<ExcelColResponse>();
            ExcelColResponse excelColResponse = null;
            try
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    int count = 0;
                    for (int i = 0; i < row.ItemArray.Length; i++)
                    {
                        if (String.IsNullOrEmpty(row.ItemArray[i].ToString().Trim()))
                        {
                            excelColResponse = new ExcelColResponse
                            {
                                RowNumber = dataTable.Rows.IndexOf(row) + 2,
                                ColumnName = dataTable.Columns[i].ColumnName.ToString(),
                                Message = "Data is empty"
                            };
                            excelColResponseList.Add(excelColResponse);
                            count++;
                            if (count == CameraExcelColumnCount)
                            {
                                //if (excelColResponseList != null && excelColResponseList.Count > 0)
                                //{
                                //    responsePath = WriteErrorLogs(excelColResponseList, "CameraLog");
                                //}
                                return new FileUploadResponse { IsValid = true, SuccessRows = addedRows, ErrorResponsePath = responsePath, ListExcelColResponses = excelColResponseList, Message = dataTable.Rows.IndexOf(row) + 2 + " Row number are blank! Please check your excel file" };
                            }
                        }
                    }
                    if (row["Inactive"].ToString() != "" && row["Inactive"].ToString().ToUpper() != "N" && row["Inactive"].ToString().ToUpper() != "Y")
                    {
                        excelColResponse = new ExcelColResponse
                        {
                            RowNumber = dataTable.Rows.IndexOf(row) + 2,
                            ColumnName = "Inactive",
                            Message = "InActive column must contain N or Y value."
                        };
                        excelColResponseList.Add(excelColResponse);
                        row["Inactive"] = "N";
                    }

                    if (!string.IsNullOrEmpty(row["Actual Memory Size (GB)"].ToString()))
                    {
                        if (!int.TryParse(row["Actual Memory Size (GB)"].ToString(), out int notinactivate))
                        {
                            excelColResponse = new ExcelColResponse
                            {
                                RowNumber = dataTable.Rows.IndexOf(row) + 2,
                                ColumnName = "Memory Size",
                                Message = "Memory Size must contain integer value."
                            };
                            excelColResponseList.Add(excelColResponse);
                            row["Actual Memory Size (GB)"] = 0;
                        }
                    }
                    if (!string.IsNullOrEmpty(row["Pixel Size (µm)"].ToString()))
                    {
                        if (!decimal.TryParse(row["Pixel Size (µm)"].ToString(), out decimal notinactivate))
                        {
                            excelColResponse = new ExcelColResponse
                            {
                                RowNumber = dataTable.Rows.IndexOf(row) + 2,
                                ColumnName = "Pixel Size (µm)",
                                Message = "Pixel Size (µm) must contain decimal value."
                            };
                            excelColResponseList.Add(excelColResponse);
                            row["Pixel Size (µm)"] = 0;
                        }
                    }
                }
                if (excelColResponseList != null && excelColResponseList.Count > 0)
                {
                    //responsePath = WriteErrorLogs(excelColResponseList, "CameraLog");
                    return new FileUploadResponse { IsValid = true, SuccessRows = addedRows, ErrorResponsePath = responsePath, ListExcelColResponses = excelColResponseList, Message = "Unsuccessfully upload with 0 records." };
                }
                responsePath = string.Empty;
                WebCalculatorService dbManagerObj = new WebCalculatorService();
                dbManagerObj.DeleteAllOldCamera();
                var status = dbManagerObj.SaveBulkUploadCamara(dataTable);
                addedRows = dbManagerObj.CamaraRowCount();
                if (status)
                {
                    //if (excelColResponseList != null && excelColResponseList.Count > 0)
                    //{
                    //    responsePath = WriteErrorLogs(excelColResponseList, "CameraLog");
                    //}


                    return new FileUploadResponse { IsValid = true, ErrorResponsePath = responsePath, SuccessRows = addedRows, ListExcelColResponses = excelColResponseList, Message = "File has been successfully uploaded" };
                }
                else
                {
                    return new FileUploadResponse { IsValid = false, SuccessRows = addedRows, Message = "Error in uploading Camera Excel file" };
                }
            }
            catch (Exception ex)
            {
                return new FileUploadResponse { IsValid = false, SuccessRows = addedRows, Message = "Error in uploading Camera Excel file" };
            }
        }



    }
}
