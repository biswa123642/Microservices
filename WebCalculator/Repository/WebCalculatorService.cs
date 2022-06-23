using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebCalculator.Models;

namespace WebCalculator.Repository
{
    public class WebCalculatorService : IWebCalculatorService
    {
            private string strConnectionString;
            public static SqlConnection sqlConnection;
            private static ILog Log = LogManager.GetLogger(typeof(WebCalculatorService));

            public WebCalculatorService()
            {
                strConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AmetekWebCalculatorDatabase"].ConnectionString;
                sqlConnection = new SqlConnection(strConnectionString);
            }

            public bool DeleteAllOldCamera()
            {
                bool IsUpdated = false;
                try
                {
                    using (SqlConnection connection = new SqlConnection(strConnectionString))
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand("SP_WebCalculator_DeleteOldCameraList", connection);
                        command.CommandType = CommandType.StoredProcedure;

                        command.ExecuteScalar();
                        IsUpdated = true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
                return IsUpdated;
            }

            public bool SaveBulkUploadCamara(DataTable dtCamara)
            {
                if (dtCamara == null || dtCamara.Rows.Count == 0)
                {
                    return false;
                }
                bool status = false;
                try
                {
                    using (sqlConnection)
                    {
                        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(sqlConnection))
                        {
                            //Set the database table name.
                            sqlBulkCopy.DestinationTableName = "dbo.Camara";

                            //[OPTIONAL]: Map the Excel columns with that of the database table
                            sqlBulkCopy.ColumnMappings.Add("CameraModel", "Model");
                            sqlBulkCopy.ColumnMappings.Add("Camera Type", "CamaraType");
                            sqlBulkCopy.ColumnMappings.Add("Sensor Mode", "SenserMode");
                            sqlBulkCopy.ColumnMappings.Add("Inactive", "InActive");
                            sqlBulkCopy.ColumnMappings.Add("Memory Type", "MemoryType");
                            sqlBulkCopy.ColumnMappings.Add("Actual Memory Size (GB)", "MemorySize");
                            sqlBulkCopy.ColumnMappings.Add("Fast Option", "FastOption");
                            sqlBulkCopy.ColumnMappings.Add("Bit Depth", "BitDepth");
                            sqlBulkCopy.ColumnMappings.Add("Shutter Mode", "ShutterMode");
                            sqlBulkCopy.ColumnMappings.Add("CXP Bank", "CPX6Banks");
                            sqlBulkCopy.ColumnMappings.Add("Pixel Size (µm)", "PixelSize");
                            sqlBulkCopy.ColumnMappings.Add("Memory Size User Selection", "MemorySizeUserSelection");

                            sqlConnection.Open();
                            sqlBulkCopy.WriteToServer(dtCamara);
                            sqlConnection.Close();
                            status = true;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Log.Error(ex);
                    return status;
                }
                finally
                {
                    sqlConnection.Close();
                }
                return status;
            }

            public long CamaraRowCount()
            {
                using (SqlConnection connection = new SqlConnection(strConnectionString))
                {
                    long rowCount = 0;
                    try
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand("SP_WebCalculator_GetCameraRowCount", connection);
                        command.CommandType = CommandType.StoredProcedure;

                        long.TryParse(command.ExecuteScalar().ToString(), out rowCount);

                        return rowCount;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }

                    return rowCount;
                }
            }

            public List<CameraStatus> UpdateCamera(string CID)
            {
                List<CameraStatus> lstCameras = new List<CameraStatus>();
                CameraStatus camara = new CameraStatus();
                try
                {
                    string isActive = "Y";
                    using (SqlConnection connection = new SqlConnection(strConnectionString))
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand("SP_WebCalculator_UpdateCamera", connection);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CID", CID);
                        command.Parameters.AddWithValue("@InActive", isActive);
                        object status = command.ExecuteScalar();

                        camara.IsUpdated = true;
                        camara.SuccessMessage = "Inactivated Camera Successfully!";
                        lstCameras.Add(camara);

                    }
                }
                catch (Exception ex)
                {
                    camara.ErrorMessage = "Process fail";
                    lstCameras.Add(camara);
                    Log.Error(ex);
                }
                return lstCameras;
            }

            public List<Camara> GetCamaraList()
            {
                List<Camara> lstCamera = new List<Camara>();

                using (SqlConnection connection = new SqlConnection(strConnectionString))
                {
                    try
                    {
                        connection.Open();

                        SqlCommand command = new SqlCommand("SP_WebCalculator_GetCameraList", connection);
                        command.CommandType = CommandType.StoredProcedure;

                        SqlDataReader dr = command.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                Camara camaraObj = new Camara()
                                {
                                    CID = !string.IsNullOrEmpty(dr["CID"].ToString()) ? dr["CID"].ToString() : string.Empty,
                                    CamaraModel = !string.IsNullOrEmpty(dr["Model"].ToString()) ? dr["Model"].ToString() : string.Empty,
                                    CamaraType = !string.IsNullOrEmpty(dr["CamaraType"].ToString()) ? dr["CamaraType"].ToString() : string.Empty,
                                    SenserMode = !string.IsNullOrEmpty(dr["SenserMode"].ToString()) ? dr["SenserMode"].ToString() : string.Empty,
                                    IsActive = !string.IsNullOrEmpty(dr["InActive"].ToString()) ? dr["InActive"].ToString() : string.Empty,
                                    MemoryType = !string.IsNullOrEmpty(dr["MemoryType"].ToString()) ? dr["MemoryType"].ToString() : string.Empty,
                                    MemorySize = !string.IsNullOrEmpty(dr["MemorySize"].ToString()) ? dr["MemorySize"].ToString() : string.Empty,
                                    FastOption = !string.IsNullOrEmpty(dr["FastOption"].ToString()) ? dr["FastOption"].ToString() : string.Empty,
                                    BitDepth = !string.IsNullOrEmpty(dr["BitDepth"].ToString()) ? dr["BitDepth"].ToString() : string.Empty,
                                    ShutterMode = !string.IsNullOrEmpty(dr["ShutterMode"].ToString()) ? dr["ShutterMode"].ToString() : string.Empty,
                                    CXPBank = !string.IsNullOrEmpty(dr["CPX6Banks"].ToString()) ? dr["CPX6Banks"].ToString() : string.Empty,
                                    PixelSize = !string.IsNullOrEmpty(dr["PixelSize"].ToString()) ? dr["PixelSize"].ToString() : string.Empty,
                                };
                                lstCamera.Add(camaraObj);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                        return null;
                    }
                }
                return lstCamera;
            }

            public int DeleteFrameRateTable()
            {
                using (SqlConnection connection = new SqlConnection(strConnectionString))
                {
                    int rowAffected = 0;
                    try
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand("SP_WebCalculator_DeleteFrameRateTable", connection);
                        command.CommandType = CommandType.StoredProcedure;
                        rowAffected = command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }

                    return rowAffected;
                }
            }

            public bool SaveBulkUploadFrameRate(DataTable dtFrameRate)
            {
                if (dtFrameRate == null || dtFrameRate.Rows.Count == 0)
                {
                    return false;
                }
                bool status = false;
                try
                {
                    using (sqlConnection)
                    {
                        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(sqlConnection))
                        {
                            //Set the database table name.
                            sqlBulkCopy.DestinationTableName = "dbo.FrameRate";

                            //[OPTIONAL]: Map the Excel columns with that of the database table
                            sqlBulkCopy.ColumnMappings.Add("CameraModel", "CamaraModel");
                            sqlBulkCopy.ColumnMappings.Add("FW Version", "FWVersion");
                            sqlBulkCopy.ColumnMappings.Add("Camera Type", "CamaraType");
                            //sqlBulkCopy.ColumnMappings.Add("Inactive", "InActive");
                            sqlBulkCopy.ColumnMappings.Add("Sensor Mode", "SenserMode");
                            sqlBulkCopy.ColumnMappings.Add("Memory Type", "MemoryType");
                            sqlBulkCopy.ColumnMappings.Add("Actual Memory Size (GB)", "MemorySize");
                            //sqlBulkCopy.ColumnMappings.Add("Memory Size User Selection", "MemorySizeUserSelection");
                            sqlBulkCopy.ColumnMappings.Add("Fast Option", "FastOption");
                            sqlBulkCopy.ColumnMappings.Add("Bit Depth", "BitDepth");
                            sqlBulkCopy.ColumnMappings.Add("Shutter Mode", "ShutterMode");
                            sqlBulkCopy.ColumnMappings.Add("CXP Banks", "CPX6Banks");
                            sqlBulkCopy.ColumnMappings.Add("X res", "XRes");
                            sqlBulkCopy.ColumnMappings.Add("Y res", "YRes");
                            sqlBulkCopy.ColumnMappings.Add("Rate (fps)", "RateFPS");
                            sqlBulkCopy.ColumnMappings.Add("Gpx/s", "GPXPS");
                            sqlBulkCopy.ColumnMappings.Add("Frame Ct", "FrameCt");

                            sqlConnection.Open();
                            sqlBulkCopy.WriteToServer(dtFrameRate);
                            sqlConnection.Close();
                            status = true;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Log.Error(ex);
                    return status;
                }
                finally
                {
                    sqlConnection.Close();
                }
                return status;
            }

            public long GetFrameRateRowCount()
            {
                using (SqlConnection connection = new SqlConnection(strConnectionString))
                {
                    long rowCount = 0;
                    try
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand("SP_WebCalculator_GetFrameRateRowCount", connection);
                        command.CommandType = CommandType.StoredProcedure;

                        long.TryParse(command.ExecuteScalar().ToString(), out rowCount);

                        return rowCount;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }

                    return rowCount;
                }
            }
    }
}